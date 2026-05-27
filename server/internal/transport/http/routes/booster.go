package routes

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/handlers"
	"github.com/gofiber/fiber/v2"
)

// Public
func RegisterPublicBoosterRoutes(r fiber.Router, h *handlers.BoosterHandler) {
	r.Get("/boosters", h.ListBoosters)
}

// Protected
func RegisterBoosterRoutes(r fiber.Router, h *handlers.BoosterHandler) {
	r.Post("/boosters/:id/open", h.OpenBooster)
}