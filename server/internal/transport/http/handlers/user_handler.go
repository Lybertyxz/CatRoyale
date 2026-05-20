package handlers

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
	"github.com/gofiber/fiber/v2"
)

type UserHandler struct {
	store *postgres.Store
}

func NewUserHandler(store *postgres.Store) *UserHandler {
	return &UserHandler{store: store}
}

func (h *UserHandler) GetProfile(c *fiber.Ctx) error {
	userID := c.Locals("userID").(string)

	user, err := h.store.GetUserByID(c.Context(), userID)
	if err != nil {
		return c.Status(404).JSON(fiber.Map{"error": "user not found"})
	}
	return c.JSON(user)
}

func (h *UserHandler) GetPieces(c *fiber.Ctx) error {
	userID := c.Locals("userID").(string)

	pieces, err := h.store.GetUserPieces(c.Context(), userID)
	if err != nil {
		return c.Status(500).JSON(fiber.Map{"error": err.Error()})
	}
	return c.JSON(pieces)
}
