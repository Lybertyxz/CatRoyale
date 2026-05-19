package http

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/middleware"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
	"github.com/gofiber/fiber/v2"
)

func NewRouter(cfg *config.Config, hub *ws.Hub, firebase *auth.FirebaseManager) *fiber.App {
	app := fiber.New()

	app.Get("/health", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{"status": "ok"})
	})

	api := app.Group("/api/v1")

	// Routes protégées
	protected := api.Group("", middleware.Protected(firebase))
	protected.Get("/me", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{
			"user_id":  c.Locals("userID"),
			"username": c.Locals("username"),
		})
	})

	// WebSocket
	api.Get("/ws", wsHandler(hub))

	return app
}

func wsHandler(hub *ws.Hub) fiber.Handler {
	return func(c *fiber.Ctx) error {
		return c.SendString("ws endpoint")
	}
}
