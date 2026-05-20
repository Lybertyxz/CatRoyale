package main

import (
	"context"
	"log"

	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	redisstore "github.com/Lybertyxz/CatRoyale/server/internal/store/redis"
	transporthttp "github.com/Lybertyxz/CatRoyale/server/internal/transport/http"
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

	redisStore, err := redisstore.NewStore(cfg.RedisURL)
	if err != nil {
		log.Fatal("failed to connect to redis:", err)
	}

	hub := ws.NewHub()
	go hub.Run()

	queue := matchmaking.NewQueue(redisStore.Client(), func(match matchmaking.MatchFound) {
		log.Printf("[Matchmaking] Match found: %s vs %s (match: %s)",
			match.Player1.Username,
			match.Player2.Username,
			match.MatchID,
		)
		// TODO: notifier les joueurs via WebSocket
	})
	go queue.Run(context.Background())

	app := transporthttp.NewRouter(cfg, hub, firebase)

	log.Printf("Server starting on port %s", cfg.ServerPort)
	if err := app.Listen(":" + cfg.ServerPort); err != nil {
		log.Fatal("failed to start server:", err)
	}
}
