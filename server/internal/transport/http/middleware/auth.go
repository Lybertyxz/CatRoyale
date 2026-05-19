package middleware

import (
	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/gofiber/fiber/v2"
)

func Protected(firebaseManager *auth.FirebaseManager) fiber.Handler {
	return func(c *fiber.Ctx) error {
		token := c.Get("Authorization")
		if token == "" {
			return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
				"error": "missing token",
			})
		}

		if len(token) > 7 && token[:7] == "Bearer " {
			token = token[7:]
		}

		claims, err := firebaseManager.VerifyToken(c.Context(), token)
		if err != nil {
			return c.Status(fiber.StatusUnauthorized).JSON(fiber.Map{
				"error": "invalid token",
			})
		}

		c.Locals("userID", claims.UID)
		c.Locals("username", claims.Name)
		c.Locals("email", claims.Email)

		return c.Next()
	}
}
