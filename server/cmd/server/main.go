package main

import (
	"context"
	"log"
	"time"

	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	redisstore "github.com/Lybertyxz/CatRoyale/server/internal/store/redis"
	transporthttp "github.com/Lybertyxz/CatRoyale/server/internal/transport/http"
	"github.com/Lybertyxz/CatRoyale/server/internal/transport/ws"
	"github.com/Lybertyxz/CatRoyale/server/pkg/protocol"
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

	roomManager := game.NewRoomManager()

	queue := matchmaking.NewQueue(redisStore.Client(), func(match matchmaking.MatchFound) {
		log.Printf("[Matchmaking] Match found: %s vs %s (match: %s)",
			match.Player1.Username,
			match.Player2.Username,
			match.MatchID,
		)

		sender := func(playerID string, msgType string, payload interface{}) {
			for _, client := range hub.Clients {
				if client.UserID == playerID {
					client.SendEnvelope(protocol.MessageType(msgType), payload)
				}
			}
		}

		room := roomManager.CreateRoom(
			match.MatchID,
			match.Player1.UserID,
			match.Player2.UserID,
			make(map[string]*game.PieceTemplate),
			sender,
		)

		room.Match.Status = game.MatchStatusInProgress
		room.Match.TurnStartedAt = time.Now()

		sender(match.Player1.UserID, "game_start", map[string]interface{}{
			"match_id":      match.MatchID,
			"opponent":      match.Player2.Username,
			"your_turn":     true,
			"turn_duration": game.DefaultTurnTimeSeconds,
		})
		sender(match.Player2.UserID, "game_start", map[string]interface{}{
			"match_id":      match.MatchID,
			"opponent":      match.Player1.Username,
			"your_turn":     false,
			"turn_duration": game.DefaultTurnTimeSeconds,
		})
	})
	go queue.Run(context.Background())

	app := transporthttp.NewRouter(cfg, hub, firebase, roomManager, queue)

	log.Printf("Server starting on port %s", cfg.ServerPort)
	if err := app.Listen(":" + cfg.ServerPort); err != nil {
		log.Fatal("failed to start server:", err)
	}
}
