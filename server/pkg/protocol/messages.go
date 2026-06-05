package protocol

import "encoding/json"

type MessageType string

const (
	MsgJoinQueue  MessageType = "join_queue"
	MsgGameStart  MessageType = "game_start"
	MsgSubmitDeck MessageType = "submit_deck"
	MsgDeckReady  MessageType = "deck_ready"
	MsgGameReady  MessageType = "game_ready"
	MsgPlayTurn   MessageType = "play_turn"
	MsgTurnResult MessageType = "turn_result"
	MsgGameOver   MessageType = "game_over"
	MsgError      MessageType = "error"
	MsgOpponentDisconnected MessageType = "opponent_disconnected"
)

type Envelope struct {
	Type    MessageType     `json:"type"`
	Payload json.RawMessage `json:"payload"`
}

type ErrorPayload struct {
	Message string `json:"message"`
}

// SubmitDeckPayload — envoyé par le client avant la partie
type SubmitDeckPayload struct {
	MatchID string             `json:"match_id"`
	Entries []DeckEntryPayload `json:"entries"`
}

type DeckEntryPayload struct {
	TemplateID string `json:"template_id"`
	StartX     int    `json:"start_x"`
	StartY     int    `json:"start_y"`
}
