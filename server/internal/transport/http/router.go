package http

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/middleware"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
	"github.com/gofiber/contrib/websocket"
	"github.com/gofiber/fiber/v2"
)

func NewRouter(cfg *config.Config, hub *ws.Hub, firebase *auth.FirebaseManager, roomManager *game.RoomManager, queue *matchmaking.Queue, userService *auth.UserService, pgStore *postgres.Store, boosterService *postgres.BoosterService) *fiber.App {
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

	// Boosters
	protected.Get("/boosters", func(c *fiber.Ctx) error {
		rows, err := pgStore.Pool().Query(c.Context(), "SELECT id, name, description, price_coins, price_gems, pieces_count FROM booster_types")
		if err != nil {
			return c.Status(500).JSON(fiber.Map{"error": err.Error()})
		}
		defer rows.Close()

		var boosters []map[string]interface{}
		for rows.Next() {
			var id, name, description string
			var priceCoins, priceGems, piecesCount int
			rows.Scan(&id, &name, &description, &priceCoins, &priceGems, &piecesCount)
			boosters = append(boosters, map[string]interface{}{
				"id": id, "name": name, "description": description,
				"price_coins": priceCoins, "price_gems": priceGems,
				"pieces_count": piecesCount,
			})
		}
		return c.JSON(boosters)
	})

	protected.Post("/boosters/:id/open", func(c *fiber.Ctx) error {
		userID := c.Locals("userID").(string)
		boosterID := c.Params("id")

		pieces, err := boosterService.OpenBooster(c.Context(), userID, boosterID)
		if err != nil {
			return c.Status(400).JSON(fiber.Map{"error": err.Error()})
		}
		return c.JSON(fiber.Map{"pieces": pieces})
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
