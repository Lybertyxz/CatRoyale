package matchmaking

import (
	"context"
	"encoding/json"
	"fmt"
	"time"

	"github.com/redis/go-redis/v9"
)

const (
	matchmakingQueueKey = "matchmaking:queue"
	tickInterval        = 2 * time.Second
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
func (q *Queue) Join(ctx context.Context, player Player) error {
	data, err := json.Marshal(player)
	if err != nil {
		return err
	}
	return q.redis.RPush(ctx, matchmakingQueueKey, data).Err()
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
			return q.redis.LRem(ctx, matchmakingQueueKey, 1, p).Err()
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
		// Remet p1 dans la queue
		q.redis.LPush(ctx, matchmakingQueueKey, p1Data)
		return
	}

	var p1, p2 Player
	json.Unmarshal([]byte(p1Data), &p1)
	json.Unmarshal([]byte(p2Data), &p2)

	matchID := fmt.Sprintf("match:%s:%s:%d", p1.UserID, p2.UserID, time.Now().UnixNano())

	if q.OnMatch != nil {
		q.OnMatch(MatchFound{
			MatchID: matchID,
			Player1: p1,
			Player2: p2,
		})
	}
}
