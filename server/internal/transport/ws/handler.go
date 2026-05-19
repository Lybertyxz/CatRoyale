package ws

import (
	"context"

	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/gofiber/contrib/websocket"
	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
)

func Handler(hub *Hub, firebase *auth.FirebaseManager) fiber.Handler {
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

		go client.WritePump()
		client.ReadPump()
	})
}
