package protocol

import "encoding/json"

type MessageType string

const (
	MsgJoinQueue  MessageType = "join_queue"
	MsgGameStart  MessageType = "game_start"
	MsgPlayTurn   MessageType = "play_turn"
	MsgTurnResult MessageType = "turn_result"
	MsgGameOver   MessageType = "game_over"
	MsgError      MessageType = "error"
)

type Envelope struct {
	Type    MessageType     `json:"type"`
	Payload json.RawMessage `json:"payload"`
}

type ErrorPayload struct {
	Message string `json:"message"`
}
