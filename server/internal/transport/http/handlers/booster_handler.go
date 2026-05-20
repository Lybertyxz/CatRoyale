package handlers

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
	"github.com/gofiber/fiber/v2"
)

type BoosterHandler struct {
	boosterService *postgres.BoosterService
	store          *postgres.Store
}

func NewBoosterHandler(store *postgres.Store, boosterService *postgres.BoosterService) *BoosterHandler {
	return &BoosterHandler{
		boosterService: boosterService,
		store:          store,
	}
}

func (h *BoosterHandler) ListBoosters(c *fiber.Ctx) error {
	rows, err := h.store.Pool().Query(c.Context(),
		"SELECT id, name, description, price_coins, price_gems, pieces_count FROM booster_types",
	)
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
			"id":           id,
			"name":         name,
			"description":  description,
			"price_coins":  priceCoins,
			"price_gems":   priceGems,
			"pieces_count": piecesCount,
		})
	}
	return c.JSON(boosters)
}

func (h *BoosterHandler) OpenBooster(c *fiber.Ctx) error {
	userID := c.Locals("userID").(string)
	boosterID := c.Params("id")

	pieces, err := h.boosterService.OpenBooster(c.Context(), userID, boosterID)
	if err != nil {
		return c.Status(400).JSON(fiber.Map{"error": err.Error()})
	}
	return c.JSON(fiber.Map{"pieces": pieces})
}
