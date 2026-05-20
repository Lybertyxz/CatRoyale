package http

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/middleware"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
	"github.com/gofiber/contrib/websocket"
	"github.com/gofiber/fiber/v2"
)

func NewRouter(cfg *config.Config, hub *ws.Hub, firebase *auth.FirebaseManager, roomManager *game.RoomManager, queue *matchmaking.Queue, userService *auth.UserService) *fiber.App {
	app := fiber.New()

	app.Get("/health", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{"status": "ok"})
	})

	api := app.Group("/api/v1")

	// Routes protégées HTTP
	protected := api.Group("", middleware.Protected(firebase))
	protected.Get("/me", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{
			"user_id":  c.Locals("userID"),
			"username": c.Locals("username"),
		})
	})

	api.Use("/ws", func(c *fiber.Ctx) error {
		if websocket.IsWebSocketUpgrade(c) {
			return c.Next()
		}
		return fiber.ErrUpgradeRequired
	})
	api.Get("/ws", ws.Handler(hub, firebase, roomManager, queue))

	api.Post("/auth/login", func(c *fiber.Ctx) error {
		var body struct {
			Token string `json:"token"`
		}
		if err := c.BodyParser(&body); err != nil {
			return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"error": "invalid body"})
		}
		user, err := userService.GetOrCreateUser(c.Context(), body.Token)
		if err != nil {
			return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{"error": err.Error()})
		}
		return c.JSON(user)
	})

	return app
}
