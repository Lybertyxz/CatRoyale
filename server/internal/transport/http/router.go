package http

import (
	"time"

	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	matchmakingpkg "github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
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
	deckHandler := handlers.NewDeckHandler(pgStore)

	// ─── Health ──────────────────────────────────────────
	app.Get("/health", func(c *fiber.Ctx) error {
		return c.JSON(fiber.Map{"status": "ok"})
	})

	api := app.Group("/api/v1")

	// ─── Dev Mode ────────────────────────────────────────
	if cfg.DevMode {
		// Injecte un user fictif sur toutes les routes
		api.Use(func(c *fiber.Ctx) error {
			c.Locals("userID", "test_user_test_token")
			c.Locals("username", "TestPlayer")
			return c.Next()
		})

		// Routes de test uniquement
		api.Get("/test/join-queue", func(c *fiber.Ctx) error {
			player := matchmakingpkg.Player{
				UserID:   "test_user_2",
				Username: "TestPlayer2",
				JoinedAt: time.Now(),
			}
			queue.Join(c.Context(), player)
			return c.JSON(fiber.Map{"status": "joined"})
		})
	}

	// ─── Auth (public) ───────────────────────────────────
	api.Post("/auth/login", authHandler.Login)

	// ─── Boosters (public) ───────────────────────────────
	api.Get("/boosters", boosterHandler.ListBoosters)

	// ─── WebSocket ───────────────────────────────────────
	api.Use("/ws", func(c *fiber.Ctx) error {
		if websocket.IsWebSocketUpgrade(c) {
			return c.Next()
		}
		return fiber.ErrUpgradeRequired
	})
	api.Get("/ws", ws.Handler(hub, firebase, roomManager, queue))

	// ─── Protected ───────────────────────────────────────
	var protected fiber.Router
	if cfg.DevMode {
		protected = api.Group("")
	} else {
		protected = api.Group("", middleware.Protected(firebase))
	}

	// User
	protected.Get("/me", authHandler.Me)
	protected.Get("/profile", userHandler.GetProfile)
	protected.Get("/pieces", userHandler.GetPieces)

	// Boosters
	protected.Post("/boosters/:id/open", boosterHandler.OpenBooster)

	// Decks
	protected.Get("/decks", deckHandler.ListDecks)
	protected.Post("/decks", deckHandler.CreateDeck)
	protected.Get("/decks/:id", deckHandler.GetDeck)
	protected.Put("/decks/:id", deckHandler.SaveDeck)
	protected.Put("/decks/:id/active", deckHandler.SetActiveDeck)
	protected.Delete("/decks/:id", deckHandler.DeleteDeck)

	return app
}