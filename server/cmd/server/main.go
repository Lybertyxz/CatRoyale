package main

import (
	"context"
	"log"

	"github.com/Lybertyxz/CatRoyale/server/internal/auth"
	"github.com/Lybertyxz/CatRoyale/server/internal/config"
	"github.com/Lybertyxz/CatRoyale/server/internal/game"
	"github.com/Lybertyxz/CatRoyale/server/internal/matchmaking"
	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
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

	pgStore, err := postgres.NewStore(cfg.DatabaseURL)
	if err != nil {
		log.Fatal("failed to connect to postgres:", err)
	}
	defer pgStore.Close()

	redisStore, err := redisstore.NewStore(cfg.RedisURL)
	if err != nil {
		log.Fatal("failed to connect to redis:", err)
	}

	userService  := auth.NewUserService(pgStore, firebase)
	hub          := ws.NewHub()
	roomManager  := game.NewRoomManager()

	go hub.Run()

	queue := matchmaking.NewQueue(redisStore.Client(), func(match matchmaking.MatchFound) {
		ctx := context.Background()

		log.Printf("[Matchmaking] Match found: %s vs %s (match: %s)",
			match.Player1.Username, match.Player2.Username, match.MatchID)

		// ── Charge les templates ──────────────────────────────
		rows, err := pgStore.ListPieceTemplates(ctx)
		if err != nil {
			log.Printf("[Matchmaking] Failed to load templates: %v", err)
			return
		}
		templates := game.ConvertTemplates(rows)

		// ── Crée la room ──────────────────────────────────────
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
			templates,
			sender,
		)
		room.Match.Status = game.MatchStatusInProgress

		// ── Notifie les joueurs ───────────────────────────────
		sender(match.Player1.UserID, "game_start", map[string]interface{}{
		    "match_id":      match.MatchID,
		    "opponent":      match.Player2.Username,
		    "your_turn":     true,
		    "turn_duration": game.DefaultTurnTimeSeconds,
		    "player_index":  0,
		    "player_id":     match.Player1.UserID,
		})
		sender(match.Player2.UserID, "game_start", map[string]interface{}{
		    "match_id":      match.MatchID,
		    "opponent":      match.Player1.Username,
		    "your_turn":     false,
		    "turn_duration": game.DefaultTurnTimeSeconds,
		    "player_index":  1,
		    "player_id":     match.Player2.UserID,
		})
		
		// ── Auto-submit des decks actifs ──────────────────────
		players := []matchmaking.Player{match.Player1, match.Player2}
		for i, player := range players {
			deck, err := pgStore.GetActiveDeck(ctx, player.UserID)
			if err != nil {
				log.Printf("[Matchmaking] No active deck for %s: %v", player.UserID, err)
				continue
			}

			entries, err := pgStore.GetDeckEntries(ctx, deck.ID)
			if err != nil {
				log.Printf("[Matchmaking] Failed to load deck entries for %s: %v", player.UserID, err)
				continue
			}

			deckPayload := protocol.SubmitDeckPayload{
				MatchID: match.MatchID,
				Entries: make([]protocol.DeckEntryPayload, 0, len(entries)),
			}
			for _, e := range entries {
				deckPayload.Entries = append(deckPayload.Entries, protocol.DeckEntryPayload{
					TemplateID: e.TemplateID,
					StartX:     int(e.StartX),
					StartY:     int(e.StartY),
				})
			}

			if err := roomManager.SubmitDeck(player.UserID, deckPayload); err != nil {
				log.Printf("[Matchmaking] SubmitDeck failed for %s: %v", player.UserID, err)
				continue
			}

			log.Printf("[Matchmaking] Deck auto-submitted for %s (playerIndex: %d)", player.UserID, i)
		}
	})

	go queue.Run(context.Background())
	go ws.DispatchMessages(hub, roomManager, queue)

	boosterService := postgres.NewBoosterService(pgStore)
	app := transporthttp.NewRouter(cfg, hub, firebase, roomManager, queue, userService, pgStore, boosterService)

	log.Printf("Server starting on port %s", cfg.ServerPort)
	if err := app.Listen(":" + cfg.ServerPort); err != nil {
		log.Fatal("failed to start server:", err)
	}
}