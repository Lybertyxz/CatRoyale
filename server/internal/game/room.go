package game

import (
	"encoding/json"
	"fmt"
	"log"
	"sync"
	"time"

	"github.com/Lybertyxz/CatRoyale/server/pkg/protocol"
)

type RoomManager struct {
	mu      sync.RWMutex
	matches map[string]*MatchRoom
}

type MatchRoom struct {
	Match        *Match
	Processor    *TurnProcessor
	DecksReady   [2]bool
	SendToPlayer func(playerID string, msgType string, payload interface{})
}

func NewRoomManager() *RoomManager {
	return &RoomManager{
		matches: make(map[string]*MatchRoom),
	}
}

func (rm *RoomManager) CreateRoom(matchID, p1ID, p2ID string, templates map[string]*PieceTemplate, sender func(string, string, interface{})) *MatchRoom {
	match := NewMatch(matchID, p1ID, p2ID)
	match.Templates = templates

	room := &MatchRoom{
		Match:        match,
		Processor:    NewTurnProcessor(match),
		SendToPlayer: sender,
	}

	rm.mu.Lock()
	rm.matches[matchID] = room
	rm.mu.Unlock()

	log.Printf("[RoomManager] Room created: %s", matchID)
	return room
}

func (rm *RoomManager) GetRoom(matchID string) (*MatchRoom, bool) {
	rm.mu.RLock()
	defer rm.mu.RUnlock()
	room, ok := rm.matches[matchID]
	return room, ok
}

func (rm *RoomManager) GetRoomByPlayer(playerID string) (*MatchRoom, bool) {
	rm.mu.RLock()
	defer rm.mu.RUnlock()
	for _, room := range rm.matches {
		for _, id := range room.Match.PlayerIDs {
			if id == playerID {
				return room, true
			}
		}
	}
	return nil, false
}

func (rm *RoomManager) RemoveRoom(matchID string) {
	rm.mu.Lock()
	delete(rm.matches, matchID)
	rm.mu.Unlock()
	log.Printf("[RoomManager] Room removed: %s", matchID)
}

func (rm *RoomManager) HandleAction(playerID string, action PlayerAction) error {
	room, ok := rm.GetRoomByPlayer(playerID)
	if !ok {
		return fmt.Errorf("player not in any room")
	}

	err := room.Processor.ProcessAction(action)
	if err != nil {
		room.SendToPlayer(playerID, "error", map[string]string{"message": err.Error()})
		return err
	}

	rm.broadcastState(room)

	if winnerID := room.Match.CheckWinner(); winnerID != "" {
		room.Match.Status = MatchStatusFinished
		room.Match.WinnerID = winnerID
		rm.broadcastGameOver(room, winnerID)
		rm.RemoveRoom(room.Match.ID)
	}

	return nil
}

func (rm *RoomManager) broadcastState(room *MatchRoom) {
	state := rm.buildStatePayload(room)
	for _, playerID := range room.Match.PlayerIDs {
		room.SendToPlayer(playerID, "turn_result", state)
	}
}

func (rm *RoomManager) broadcastGameOver(room *MatchRoom, winnerID string) {
	payload := map[string]string{"winner_id": winnerID}
	for _, playerID := range room.Match.PlayerIDs {
		room.SendToPlayer(playerID, "game_over", payload)
	}
}

func (rm *RoomManager) buildStatePayload(room *MatchRoom) map[string]interface{} {
	pieces := make([]interface{}, 0)
	for y := 0; y < BoardSize; y++ {
		for x := 0; x < BoardSize; x++ {
			piece := room.Match.Board.Cells[y][x]
			if piece != nil {
				data, _ := json.Marshal(piece)
				var obj interface{}
				json.Unmarshal(data, &obj)
				pieces = append(pieces, obj)
			}
		}
	}

	m := room.Match
	return map[string]interface{}{
		"match_id":        m.ID,
		"turn_number":     m.TurnNumber,
		"current_player":  m.CurrentPlayerID(),
		"time_bank_p0":    m.TimeBanks[0].Seconds(),
		"time_bank_p1":    m.TimeBanks[1].Seconds(),
		"remaining_pa":    m.Turn.RemainingPA,
		"remaining_pm":    m.Turn.RemainingPM,
		"pieces":          pieces,
	}
}

func (rm *RoomManager) SubmitDeck(playerID string, payload protocol.SubmitDeckPayload) error {
	room, ok := rm.GetRoomByPlayer(playerID)
	if !ok {
		return fmt.Errorf("player not in any room")
	}

	if room.Match.Status != MatchStatusInProgress {
		return fmt.Errorf("match not accepting decks")
	}

	playerIndex := -1
	for i, id := range room.Match.PlayerIDs {
		if id == playerID {
			playerIndex = i
			break
		}
	}
	if playerIndex == -1 {
		return fmt.Errorf("player not found in match")
	}

	for _, entry := range payload.Entries {
		tmpl, ok := room.Match.Templates[entry.TemplateID]
		if !ok {
			return fmt.Errorf("unknown piece template: %s", entry.TemplateID)
		}

		pos := Position{X: entry.StartX, Y: entry.StartY}
		if playerIndex == 1 {
			pos.Y = BoardSize - 1 - entry.StartY
		}

		if !room.Match.Board.IsPlayerZone(pos, playerIndex) {
			return fmt.Errorf("position not in player zone: %v (playerIndex: %d)", pos, playerIndex)
		}

		instance := &PieceInstance{
			TemplateID:       tmpl.ID,
			OwnerID:          playerID,
			CurrentHP:        tmpl.MaxHP,
			Position:         pos,
			AbilityCooldowns: make(map[string]int),
			ActiveStates:     []StatusEffect{},
			IsAlive:          true,
		}

		if !room.Match.Board.PlacePiece(instance, pos) {
			return fmt.Errorf("cannot place piece at %v", pos)
		}
	}

	room.DecksReady[playerIndex] = true
	room.SendToPlayer(playerID, "deck_ready", map[string]bool{"ready": true})

	if room.DecksReady[0] && room.DecksReady[1] {
		rm.startMatch(room)
	}

	return nil
}

func (rm *RoomManager) startMatch(room *MatchRoom) {
	room.Match.TurnStartedAt = time.Now()

	state := rm.buildStatePayload(room)
	for i, playerID := range room.Match.PlayerIDs {
		room.SendToPlayer(playerID, "game_ready", map[string]interface{}{
			"message":      "both players ready, game starts",
			"state":        state,
			"player_index": i,
		})
	}

	log.Printf("[RoomManager] Match started: %s", room.Match.ID)
}