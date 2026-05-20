package handlers

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/gofiber/fiber/v2"
)

type AuthHandler struct {
	userService *auth.UserService
}

func NewAuthHandler(userService *auth.UserService) *AuthHandler {
	return &AuthHandler{userService: userService}
}

func (h *AuthHandler) Login(c *fiber.Ctx) error {
	var body struct {
		Token string `json:"token"`
	}
	if err := c.BodyParser(&body); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"error": "invalid body"})
	}
	user, err := h.userService.GetOrCreateUser(c.Context(), body.Token)
	if err != nil {
		return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{"error": err.Error()})
	}
	return c.JSON(user)
}

func (h *AuthHandler) Me(c *fiber.Ctx) error {
	return c.JSON(fiber.Map{
		"user_id":  c.Locals("userID"),
		"username": c.Locals("username"),
	})
}
