package ws

import (
	"context"
	"encoding/json"
	"log"
	"time"

	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	matchmakingpkg "github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	"github.com/Lybertyxz/CatRoyale/server/pkg/protocol"
	"github.com/gofiber/contrib/websocket"
	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
)

func Handler(hub *Hub, firebase *auth.FirebaseManager, roomManager *game.RoomManager, queue *matchmakingpkg.Queue) fiber.Handler {
	return websocket.New(func(conn *websocket.Conn) {
		token := conn.Query("token")
		if token == "" {
			conn.Close()
			return
		}

		claims, err := firebase.VerifyToken(context.Background(), token)
		if err != nil {
			conn.Close()
			return
		}

		client := NewClient(
			uuid.New().String(),
			claims.UID,
			claims.Name,
			conn,
			hub,
		)

		hub.Register <- client
		log.Printf("[WS] Player connected: %s", claims.UID)

		go client.WritePump()
		go dispatchMessages(hub, roomManager, queue)

		client.ReadPump()

		log.Printf("[WS] Player disconnected: %s", claims.UID)
	})
}

func dispatchMessages(hub *Hub, roomManager *game.RoomManager, queue *matchmakingpkg.Queue) {
	for msg := range hub.Incoming {
		switch msg.Envelope.Type {
		case protocol.MsgPlayTurn:
			var action game.PlayerAction
			if err := json.Unmarshal(msg.Envelope.Payload, &action); err != nil {
				log.Printf("[WS] Invalid action payload: %v", err)
				continue
			}
			action.PlayerID = msg.Client.UserID
			if err := roomManager.HandleAction(msg.Client.UserID, action); err != nil {
				log.Printf("[WS] Action error: %v", err)
			}

		case protocol.MsgJoinQueue:
			log.Printf("[WS] Player joining queue: %s", msg.Client.UserID)
			player := matchmakingpkg.Player{
				UserID:   msg.Client.UserID,
				Username: msg.Client.Username,
				JoinedAt: time.Now(),
			}
			if err := queue.Join(context.Background(), player); err != nil {
				log.Printf("[WS] JoinQueue error: %v", err)
				msg.Client.SendEnvelope(protocol.MsgError, map[string]string{"message": err.Error()})
			}

		case protocol.MsgSubmitDeck:
			var payload protocol.SubmitDeckPayload
			if err := json.Unmarshal(msg.Envelope.Payload, &payload); err != nil {
				log.Printf("[WS] Invalid deck payload: %v", err)
				continue
			}
			if err := roomManager.SubmitDeck(msg.Client.UserID, payload); err != nil {
				log.Printf("[WS] SubmitDeck error: %v", err)
				msg.Client.SendEnvelope(protocol.MsgError, map[string]string{"message": err.Error()})
			}
		}
	}
}
