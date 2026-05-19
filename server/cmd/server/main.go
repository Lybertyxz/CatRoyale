package main

import (
	"log"

	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/http"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
)

func main() {
	cfg, err := config.Load()
	if err != nil {
		log.Fatal("failed to load config:", err)
	}

	firebase, err := auth.NewFirebaseManager(cfg.FirebaseServiceAccount)
	if err != nil {
		log.Fatal("failed to init firebase:", err)
	}

	hub := ws.NewHub()
	go hub.Run()

	app := http.NewRouter(cfg, hub, firebase)

	log.Printf("Server starting on port %s", cfg.ServerPort)
	if err := app.Listen(":" + cfg.ServerPort); err != nil {
		log.Fatal("failed to start server:", err)
	}
}
