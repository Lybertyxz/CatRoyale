package matchmaking

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"time"

	"github.com/redis/go-redis/v9"
)

const (
	matchmakingQueueKey    = "matchmaking:queue"
	matchmakingInQueueKey  = "matchmaking:in_queue" // set des userIDs en attente
	tickInterval           = 2 * time.Second
)

type Player struct {
	UserID   string    `json:"user_id"`
	Username string    `json:"username"`
	JoinedAt time.Time `json:"joined_at"`
}

type MatchFound struct {
	MatchID string
	Player1 Player
	Player2 Player
}

type Queue struct {
	redis   *redis.Client
	OnMatch func(match MatchFound)
}

func NewQueue(redisClient *redis.Client, onMatch func(MatchFound)) *Queue {
	return &Queue{
		redis:   redisClient,
		OnMatch: onMatch,
	}
}

// Join ajoute un joueur dans la file d'attente
// Retourne une erreur si le joueur est déjà en queue
func (q *Queue) Join(ctx context.Context, player Player) error {
	// Vérifie si déjà en queue
	inQueue, err := q.redis.SIsMember(ctx, matchmakingInQueueKey, player.UserID).Result()
	if err != nil {
		return fmt.Errorf("failed to check queue status: %w", err)
	}
	if inQueue {
		return fmt.Errorf("player already in queue")
	}

	data, err := json.Marshal(player)
	if err != nil {
		return err
	}

	// Ajoute dans la liste et le set atomiquement
	pipe := q.redis.Pipeline()
	pipe.RPush(ctx, matchmakingQueueKey, data)
	pipe.SAdd(ctx, matchmakingInQueueKey, player.UserID)
	_, err = pipe.Exec(ctx)
	if err != nil {
		return fmt.Errorf("failed to join queue: %w", err)
	}

	log.Printf("[Queue] Player joined: %s", player.UserID)
	return nil
}

// Leave retire un joueur de la file d'attente
func (q *Queue) Leave(ctx context.Context, userID string) error {
	players, err := q.redis.LRange(ctx, matchmakingQueueKey, 0, -1).Result()
	if err != nil {
		return err
	}

	for _, p := range players {
		var player Player
		if err := json.Unmarshal([]byte(p), &player); err != nil {
			continue
		}
		if player.UserID == userID {
			pipe := q.redis.Pipeline()
			pipe.LRem(ctx, matchmakingQueueKey, 1, p)
			pipe.SRem(ctx, matchmakingInQueueKey, userID)
			_, err = pipe.Exec(ctx)
			log.Printf("[Queue] Player left: %s", userID)
			return err
		}
	}
	return nil
}

// Run démarre la boucle de matchmaking
func (q *Queue) Run(ctx context.Context) {
	ticker := time.NewTicker(tickInterval)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
			q.tick(ctx)
		}
	}
}

// tick vérifie s'il y a assez de joueurs pour créer une partie
func (q *Queue) tick(ctx context.Context) {
	count, err := q.redis.LLen(ctx, matchmakingQueueKey).Result()
	if err != nil || count < 2 {
		return
	}

	p1Data, err := q.redis.LPop(ctx, matchmakingQueueKey).Result()
	if err != nil {
		return
	}
	p2Data, err := q.redis.LPop(ctx, matchmakingQueueKey).Result()
	if err != nil {
		q.redis.LPush(ctx, matchmakingQueueKey, p1Data)
		return
	}

	var p1, p2 Player
	json.Unmarshal([]byte(p1Data), &p1)
	json.Unmarshal([]byte(p2Data), &p2)

	// Empêche un joueur de se matcher avec lui-même
	if p1.UserID == p2.UserID {
		log.Printf("[Queue] Same player matched with itself, skipping: %s", p1.UserID)
		q.redis.LPush(ctx, matchmakingQueueKey, p1Data)
		return
	}

	// Retire les joueurs du set in_queue
	pipe := q.redis.Pipeline()
	pipe.SRem(ctx, matchmakingInQueueKey, p1.UserID)
	pipe.SRem(ctx, matchmakingInQueueKey, p2.UserID)
	pipe.Exec(ctx)

	matchID := fmt.Sprintf("match:%s:%s:%d", p1.UserID, p2.UserID, time.Now().UnixNano())
	log.Printf("[Queue] Match created: %s vs %s", p1.Username, p2.Username)

	if q.OnMatch != nil {
		q.OnMatch(MatchFound{
			MatchID: matchID,
			Player1: p1,
			Player2: p2,
		})
	}
}