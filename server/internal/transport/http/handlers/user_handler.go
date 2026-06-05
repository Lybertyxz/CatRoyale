package handlers

import (
	"encoding/json"

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

// GetPieces retourne les templates de toutes les pièces (catalogue)
func (h *UserHandler) GetPieces(c *fiber.Ctx) error {
	pieces, err := h.store.ListPieceTemplates(c.Context())
	if err != nil {
		return c.Status(500).JSON(fiber.Map{"error": err.Error()})
	}

	if pieces == nil {
		return c.JSON([]interface{}{})
	}

	return c.JSON(toPieceResponseList(pieces))
}

// GetUserPieces retourne les pièces possédées par l'utilisateur connecté
func (h *UserHandler) GetUserPieces(c *fiber.Ctx) error {
	userID, ok := c.Locals("userID").(string)
	if !ok || userID == "" {
		return c.Status(401).JSON(fiber.Map{"error": "unauthorized"})
	}

	pieces, err := h.store.GetUserPieces(c.Context(), userID)
	if err != nil {
		return c.Status(500).JSON(fiber.Map{"error": err.Error()})
	}

	if pieces == nil {
		return c.JSON([]interface{}{})
	}

	return c.JSON(pieces)
}

// ─── DTO ──────────────────────────────────────────────────

type pieceResponseDTO struct {
	ID             string          `json:"id"`
	Name           string          `json:"name"`
	Role           string          `json:"role"`
	Rarity         string          `json:"rarity"`
	SlotCost       int32           `json:"slot_cost"`
	MaxHP          int32           `json:"max_hp"`
	Attack         int32           `json:"attack"`
	Armor          int32           `json:"armor"`
	AttackRange    int32           `json:"attack_range"`
	MoveRange      int32           `json:"move_range"`
	CanJump        bool            `json:"can_jump"`
	MovementType   string          `json:"movement_type"`
	MovementCustom json.RawMessage `json:"movement_custom"`
	Abilities      json.RawMessage `json:"abilities"`
}

func toPieceResponseList(pieces []postgres.PieceTemplate) []pieceResponseDTO {
	result := make([]pieceResponseDTO, 0, len(pieces))
	for _, p := range pieces {
		var movCustom json.RawMessage
		if len(p.MovementCustom) > 0 {
			movCustom = json.RawMessage(p.MovementCustom)
		}

		var abilities json.RawMessage
		if len(p.Abilities) > 0 {
			abilities = json.RawMessage(p.Abilities)
		}

		result = append(result, pieceResponseDTO{
			ID:             p.ID,
			Name:           p.Name,
			Role:           p.Role,
			Rarity:         p.Rarity,
			SlotCost:       p.SlotCost,
			MaxHP:          p.MaxHp,
			Attack:         p.Attack,
			Armor:          p.Armor,
			AttackRange:    p.AttackRange,
			MoveRange:      p.MoveRange,
			CanJump:        p.CanJump,
			MovementType:   p.MovementType,
			MovementCustom: movCustom,
			Abilities:      abilities,
		})
	}
	return result
}