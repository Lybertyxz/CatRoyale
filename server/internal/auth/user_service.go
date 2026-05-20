package auth

import (
	"context"
	"fmt"

	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
	"github.com/google/uuid"
)

type UserService struct {
	store    *postgres.Store
	firebase *FirebaseManager
}

func NewUserService(store *postgres.Store, firebase *FirebaseManager) *UserService {
	return &UserService{
		store:    store,
		firebase: firebase,
	}
}

// GetOrCreateUser récupère ou crée un utilisateur depuis son token Firebase
func (s *UserService) GetOrCreateUser(ctx context.Context, idToken string) (*postgres.User, error) {
	claims, err := s.firebase.VerifyToken(ctx, idToken)
	if err != nil {
		return nil, fmt.Errorf("invalid token: %w", err)
	}

	// Cherche l'utilisateur en DB
	user, err := s.store.GetUserByFirebaseUID(ctx, claims.UID)
	if err == nil {
		return &user, nil
	}

	// Crée l'utilisateur s'il n'existe pas
	newUser, err := s.store.CreateUser(ctx, postgres.CreateUserParams{
		ID:          uuid.New().String(),
		Username:    claims.Name,
		Email:       claims.Email,
		FirebaseUid: claims.UID,
	})
	if err != nil {
		return nil, fmt.Errorf("failed to create user: %w", err)
	}

	return &newUser, nil
}
