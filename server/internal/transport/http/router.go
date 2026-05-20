package http

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/handlers"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/middleware"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
	"github.com/gofiber/contrib/websocket"
	"github.com/gofiber/fiber/v2"
)

func NewRouter(
	cfg *config.Config,
	hub *ws.Hub,
	firebase *auth.FirebaseManager,
	roomManager *game.RoomManager,
	queue *matchmaking.Queue,
	userService *auth.UserService,
	pgStore *postgres.Store,
	boosterService *postgres.BoosterService,
) *fiber.App {
	app := fiber.New()

	// Handlers
	authHandler := handlers.NewAuthHandler(userService)
	boosterHandler := handlers.NewBoosterHandler(pgStore, boosterService)
	userHandler := handlers.NewUserHandler(pgStore)

	// Health
	app.Get("/health", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{"status": "ok"})
	})

	api := app.Group("/api/v1")

	// Auth — public
	api.Post("/auth/login", authHandler.Login)

	// Boosters — public (liste)
	api.Get("/boosters", boosterHandler.ListBoosters)

	// WebSocket
	api.Use("/ws", func(c *fiber.Ctx) error {
		if websocket.IsWebSocketUpgrade(c) {
			return c.Next()
		}
		return fiber.ErrUpgradeRequired
	})
	api.Get("/ws", ws.Handler(hub, firebase, roomManager, queue))

	// Routes protégées
	protected := api.Group("", middleware.Protected(firebase))
	protected.Get("/me", authHandler.Me)
	protected.Get("/profile", userHandler.GetProfile)
	protected.Get("/pieces", userHandler.GetPieces)
	protected.Post("/boosters/:id/open", boosterHandler.OpenBooster)

	return app
}
