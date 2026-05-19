package auth

import (
	"context"

	firebase "firebase.google.com/go/v4"
	"firebase.google.com/go/v4/auth"
	"google.golang.org/api/option"
)

type FirebaseManager struct {
	client *auth.Client
}

func NewFirebaseManager(serviceAccountPath string) (*FirebaseManager, error) {
	opt := option.WithCredentialsFile(serviceAccountPath)
	app, err := firebase.NewApp(context.Background(), nil, opt)
	if err != nil {
		return nil, err
	}

	client, err := app.Auth(context.Background())
	if err != nil {
		return nil, err
	}

	return &FirebaseManager{client: client}, nil
}

// VerifyToken vérifie un token Firebase et retourne l'UID utilisateur
func (f *FirebaseManager) VerifyToken(ctx context.Context, idToken string) (*FirebaseClaims, error) {
	token, err := f.client.VerifyIDToken(ctx, idToken)
	if err != nil {
		return nil, ErrInvalidToken
	}

	name, _ := token.Claims["name"].(string)
	email, _ := token.Claims["email"].(string)

	return &FirebaseClaims{
		UID:   token.UID,
		Name:  name,
		Email: email,
	}, nil
}

type FirebaseClaims struct {
	UID   string
	Name  string
	Email string
}
