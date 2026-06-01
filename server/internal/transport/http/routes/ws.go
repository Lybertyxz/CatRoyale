package routes

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
	"github.com/gofiber/contrib/websocket"
	"github.com/gofiber/fiber/v2"
)

func RegisterWSRoutes(r fiber.Router, hub *ws.Hub, firebase *auth.FirebaseManager, roomManager *game.RoomManager, queue *matchmaking.Queue, cfg *config.Config) {
	r.Use("/ws", func(c *fiber.Ctx) error {
		if websocket.IsWebSocketUpgrade(c) {
			return c.Next()
		}
		return fiber.ErrUpgradeRequired
	})
	r.Get("/ws", ws.Handler(hub, firebase, roomManager, queue, cfg))
}