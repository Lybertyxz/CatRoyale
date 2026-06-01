package http

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/handlers"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/middleware"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/routes"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
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
	authHandler    := handlers.NewAuthHandler(userService)
	boosterHandler := handlers.NewBoosterHandler(pgStore, boosterService)
	userHandler    := handlers.NewUserHandler(pgStore)
	deckHandler    := handlers.NewDeckHandler(pgStore)

	// ─── Health ──────────────────────────────────────────
	app.Get("/health", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{"status": "ok"})
	})

	api := app.Group("/api/v1")

	// ─── Dev Mode ────────────────────────────────────────
	if cfg.DevMode {
		middleware.InjectDevUser(api)
		routes.RegisterDevRoutes(api, queue)
	}

	// ─── Public ──────────────────────────────────────────
	routes.RegisterAuthRoutes(api, authHandler)
	routes.RegisterPublicBoosterRoutes(api, boosterHandler)
	routes.RegisterWSRoutes(api, hub, firebase, roomManager, queue, cfg)

	// ─── Protected ───────────────────────────────────────
	var protected fiber.Router
	if cfg.DevMode {
		protected = api.Group("")
	} else {
		protected = api.Group("", middleware.Protected(firebase))
	}

	routes.RegisterUserRoutes(protected, authHandler, userHandler)
	routes.RegisterBoosterRoutes(protected, boosterHandler)
	routes.RegisterDeckRoutes(protected, deckHandler)

	return app
}