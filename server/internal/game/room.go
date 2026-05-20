package game

import (
	"encoding/json"
	"fmt"
	"log"
	"sync"
)

// RoomManager gère toutes les parties en cours
type RoomManager struct {
	mu      sync.RWMutex
	matches map[string]*MatchRoom
}

// MatchRoom associe une partie à ses connexions WebSocket
type MatchRoom struct {
	Match        *Match
	Processor    *TurnProcessor
	SendToPlayer func(playerID string, msgType string, payload interface{})
}

func NewRoomManager() *RoomManager {
	return &RoomManager{
		matches: make(map[string]*MatchRoom),
	}
}

// CreateRoom crée une nouvelle salle de jeu
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

// GetRoom retourne une salle par son ID
func (rm *RoomManager) GetRoom(matchID string) (*MatchRoom, bool) {
	rm.mu.RLock()
	defer rm.mu.RUnlock()
	room, ok := rm.matches[matchID]
	return room, ok
}

// GetRoomByPlayer retourne la salle d'un joueur
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

// RemoveRoom supprime une salle terminée
func (rm *RoomManager) RemoveRoom(matchID string) {
	rm.mu.Lock()
	delete(rm.matches, matchID)
	rm.mu.Unlock()
	log.Printf("[RoomManager] Room removed: %s", matchID)
}

// HandleAction traite l'action d'un joueur
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

	// Envoie le nouvel état aux deux joueurs
	rm.broadcastState(room)

	// Vérifie si la partie est terminée
	if winnerID := room.Match.CheckWinner(); winnerID != "" {
		room.Match.Status = MatchStatusFinished
		room.Match.WinnerID = winnerID
		rm.broadcastGameOver(room, winnerID)
		rm.RemoveRoom(room.Match.ID)
	}

	return nil
}

// broadcastState envoie l'état complet de la partie aux deux joueurs
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
	boardState := make([][]interface{}, BoardSize)
	for y := 0; y < BoardSize; y++ {
		boardState[y] = make([]interface{}, BoardSize)
		for x := 0; x < BoardSize; x++ {
			piece := room.Match.Board.Cells[y][x]
			if piece != nil {
				data, _ := json.Marshal(piece)
				var obj interface{}
				json.Unmarshal(data, &obj)
				boardState[y][x] = obj
			}
		}
	}

	return map[string]interface{}{
		"match_id":       room.Match.ID,
		"turn_number":    room.Match.TurnNumber,
		"current_player": room.Match.CurrentPlayerID(),
		"time_remaining": room.Match.TurnTimeRemaining().Seconds(),
		"board":          boardState,
	}
}
