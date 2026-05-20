package postgres

import (
	"context"
	"fmt"

	"github.com/jackc/pgx/v5/pgxpool"
)

type Store struct {
	*Queries
	pool *pgxpool.Pool
}

func NewStore(databaseURL string) (*Store, error) {
	pool, err := pgxpool.New(context.Background(), databaseURL)
	if err != nil {
		return nil, fmt.Errorf("failed to connect to postgres: %w", err)
	}

	if err := pool.Ping(context.Background()); err != nil {
		return nil, fmt.Errorf("postgres ping failed: %w", err)
	}

	return &Store{
		Queries: New(pool),
		pool:    pool,
	}, nil
}

func (s *Store) Close() {
	s.pool.Close()
}
