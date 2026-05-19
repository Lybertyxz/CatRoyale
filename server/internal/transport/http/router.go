package http

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
	"github.com/gofiber/fiber/v2"
)

func NewRouter(cfg *config.Config, hub *ws.Hub) *fiber.App {
	app := fiber.New()

	app.Get("/health", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{"status": "ok"})
	})

	api := app.Group("/api/v1")
	api.Get("/ws", wsHandler(hub))

	return app
}

func wsHandler(hub *ws.Hub) fiber.Handler {
	return func(c *fiber.Ctx) error {
		return c.SendString("ws endpoint")
	}
}
