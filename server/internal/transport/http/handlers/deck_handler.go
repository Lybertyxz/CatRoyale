package handlers

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
)

type DeckHandler struct {
	store *postgres.Store
}

func NewDeckHandler(store *postgres.Store) *DeckHandler {
	return &DeckHandler{store: store}
}

// ListDecks retourne tous les decks d'un joueur
func (h *DeckHandler) ListDecks(c *fiber.Ctx) error {
	userID := c.Locals("userID").(string)

	decks, err := h.store.GetDecksByUser(c.Context(), userID)
	if err != nil {
		return c.Status(500).JSON(fiber.Map{"error": err.Error()})
	}
	return c.JSON(decks)
}

// CreateDeck crée un nouveau deck vide
func (h *DeckHandler) CreateDeck(c *fiber.Ctx) error {
	userID := c.Locals("userID").(string)

	var body struct {
		Name string `json:"name"`
	}
	if err := c.BodyParser(&body); err != nil || body.Name == "" {
		body.Name = "Mon Deck"
	}

	deck, err := h.store.CreateDeck(c.Context(), postgres.CreateDeckParams{
		ID:     uuid.New().String(),
		UserID: userID,
		Name:   body.Name,
	})
	if err != nil {
		return c.Status(500).JSON(fiber.Map{"error": err.Error()})
	}
	return c.Status(201).JSON(deck)
}

// GetDeck retourne un deck avec ses entrées
func (h *DeckHandler) GetDeck(c *fiber.Ctx) error {
	deckID := c.Params("id")

	deck, err := h.store.GetDeckByID(c.Context(), deckID)
	if err != nil {
		return c.Status(404).JSON(fiber.Map{"error": "deck not found"})
	}

	entries, err := h.store.GetDeckEntries(c.Context(), deckID)
	if err != nil {
		return c.Status(500).JSON(fiber.Map{"error": err.Error()})
	}

	return c.JSON(fiber.Map{
		"deck":    deck,
		"entries": entries,
	})
}

// SaveDeck sauvegarde les entrées d'un deck
func (h *DeckHandler) SaveDeck(c *fiber.Ctx) error {
	userID := c.Locals("userID").(string)
	deckID := c.Params("id")

	// Vérifie que le deck appartient au joueur
	deck, err := h.store.GetDeckByID(c.Context(), deckID)
	if err != nil || deck.UserID != userID {
		return c.Status(403).JSON(fiber.Map{"error": "forbidden"})
	}

	var body struct {
		Entries []struct {
			TemplateID string `json:"template_id"`
			StartX     int    `json:"start_x"`
			StartY     int    `json:"start_y"`
		} `json:"entries"`
	}
	if err := c.BodyParser(&body); err != nil {
		return c.Status(400).JSON(fiber.Map{"error": "invalid body"})
	}

	// Supprime les anciennes entrées
	h.store.DeleteDeckEntries(c.Context(), deckID)

	// Ajoute les nouvelles
	for _, entry := range body.Entries {
		h.store.AddDeckEntry(c.Context(), postgres.AddDeckEntryParams{
			ID:         uuid.New().String(),
			DeckID:     deckID,
			TemplateID: entry.TemplateID,
			StartX:     int32(entry.StartX),
			StartY:     int32(entry.StartY),
		})
	}

	return c.JSON(fiber.Map{"success": true})
}

// SetActiveDeck définit le deck actif du joueur
func (h *DeckHandler) SetActiveDeck(c *fiber.Ctx) error {
	userID := c.Locals("userID").(string)
	deckID := c.Params("id")

	deck, err := h.store.GetDeckByID(c.Context(), deckID)
	if err != nil || deck.UserID != userID {
		return c.Status(403).JSON(fiber.Map{"error": "forbidden"})
	}

	h.store.UpdateDeckActive(c.Context(), postgres.UpdateDeckActiveParams{
		ID:       deckID,
		IsActive: true,
	})

	return c.JSON(fiber.Map{"success": true})
}

// DeleteDeck supprime un deck
func (h *DeckHandler) DeleteDeck(c *fiber.Ctx) error {
	userID := c.Locals("userID").(string)
	deckID := c.Params("id")

	deck, err := h.store.GetDeckByID(c.Context(), deckID)
	if err != nil || deck.UserID != userID {
		return c.Status(403).JSON(fiber.Map{"error": "forbidden"})
	}

	h.store.DeleteDeck(c.Context(), deckID)
	return c.JSON(fiber.Map{"success": true})
}
