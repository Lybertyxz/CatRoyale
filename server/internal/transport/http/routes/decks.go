package routes

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/handlers"
	"github.com/gofiber/fiber/v2"
)

func RegisterDeckRoutes(r fiber.Router, h *handlers.DeckHandler) {
	r.Get("/decks", h.ListDecks)
	r.Post("/decks", h.CreateDeck)
	r.Get("/decks/:id", h.GetDeck)
	r.Put("/decks/:id", h.SaveDeck)
	r.Put("/decks/:id/active", h.SetActiveDeck)
	r.Delete("/decks/:id", h.DeleteDeck)
}