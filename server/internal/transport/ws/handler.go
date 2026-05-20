package ws

import (
	"context"
	"encoding/json"
	"log"

	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/pkg/protocol"
	"github.com/gofiber/contrib/websocket"
	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
)

func Handler(hub *Hub, firebase *auth.FirebaseManager, roomManager *game.RoomManager) fiber.Handler {
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
		go dispatchMessages(hub, roomManager)

		client.ReadPump()

		log.Printf("[WS] Player disconnected: %s", claims.UID)
	})
}

func dispatchMessages(hub *Hub, roomManager *game.RoomManager) {
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
		}
	}
}
