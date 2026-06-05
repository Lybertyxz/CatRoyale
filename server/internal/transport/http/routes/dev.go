package routes

import (
	"time"

	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	matchmakingpkg "github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	"github.com/gofiber/fiber/v2"
)

func RegisterDevRoutes(r fiber.Router, queue *matchmakingpkg.Queue, roomManager *game.RoomManager) {
	r.Get("/test/join-queue", func(c *fiber.Ctx) error {
		player := matchmakingpkg.Player{
			UserID:   "test_user_2",
			Username: "TestPlayer2",
			JoinedAt: time.Now(),
		}
		queue.Join(c.Context(), player)
		return c.JSON(fiber.Map{"status": "joined"})
	})

	r.Get("/test/rooms", func(c *fiber.Ctx) error {
		return c.JSON(roomManager.DebugRooms())
	})
}