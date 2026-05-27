package routes

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http/handlers"
	"github.com/gofiber/fiber/v2"
)

func RegisterAuthRoutes(r fiber.Router, h *handlers.AuthHandler) {
	r.Post("/auth/login", h.Login)
}

func RegisterUserRoutes(r fiber.Router, authHandler *handlers.AuthHandler, userHandler *handlers.UserHandler) {
	r.Get("/me", authHandler.Me)
	r.Get("/profile", userHandler.GetProfile)
	r.Get("/pieces", userHandler.GetPieces)
	r.Get("/user/pieces", userHandler.GetUserPieces)
}