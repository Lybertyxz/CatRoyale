package postgres

import (
	"context"
	"encoding/json"
	"fmt"
	"math/rand"

	"github.com/google/uuid"
)

type RarityWeights struct {
	Common    int `json:"common"`
	Rare      int `json:"rare"`
	Epic      int `json:"epic"`
	Legendary int `json:"legendary"`
}

type BoosterService struct {
	store *Store
}

func NewBoosterService(store *Store) *BoosterService {
	return &BoosterService{store: store}
}

// OpenBooster ouvre un booster et attribue les pièces au joueur
func (b *BoosterService) OpenBooster(ctx context.Context, userID, boosterTypeID string) ([]PieceTemplate, error) {
	var boosterID string
	var piecesCount int
	var rarityWeightsRaw string

	err := b.store.pool.QueryRow(ctx,
		"SELECT id, pieces_count, rarity_weights FROM booster_types WHERE id = $1",
		boosterTypeID,
	).Scan(&boosterID, &piecesCount, &rarityWeightsRaw)
	if err != nil {
		return nil, fmt.Errorf("booster type not found: %w", err)
	}

	var weights RarityWeights
	if err := json.Unmarshal([]byte(rarityWeightsRaw), &weights); err != nil {
		return nil, fmt.Errorf("invalid rarity weights: %w", err)
	}

	allPieces, err := b.store.ListPieceTemplates(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to list pieces: %w", err)
	}

	obtained := make([]PieceTemplate, 0, piecesCount)
	for i := 0; i < piecesCount; i++ {
		rarity := rollRarity(weights)
		piece := pickRandomPieceByRarity(allPieces, rarity)
		if piece == nil {
			continue
		}
		obtained = append(obtained, *piece)

		b.store.AddPieceToUser(ctx, AddPieceToUserParams{
			ID:         uuid.New().String(),
			UserID:     userID,
			TemplateID: piece.ID,
		})
	}

	obtainedJSON, _ := json.Marshal(obtained)
	b.store.pool.Exec(ctx,
		"INSERT INTO booster_openings (id, user_id, booster_type_id, pieces_obtained) VALUES ($1, $2, $3, $4)",
		uuid.New().String(), userID, boosterTypeID, string(obtainedJSON),
	)

	return obtained, nil
}

func rollRarity(w RarityWeights) string {
	total := w.Common + w.Rare + w.Epic + w.Legendary
	roll := rand.Intn(total)

	if roll < w.Common {
		return "common"
	} else if roll < w.Common+w.Rare {
		return "rare"
	} else if roll < w.Common+w.Rare+w.Epic {
		return "epic"
	}
	return "legendary"
}

func pickRandomPieceByRarity(pieces []PieceTemplate, rarity string) *PieceTemplate {
	filtered := make([]PieceTemplate, 0)
	for _, p := range pieces {
		if p.Rarity == rarity {
			filtered = append(filtered, p)
		}
	}
	if len(filtered) == 0 {
		for _, p := range pieces {
			if p.Rarity == "common" {
				filtered = append(filtered, p)
			}
		}
	}
	if len(filtered) == 0 {
		return nil
	}
	return &filtered[rand.Intn(len(filtered))]
}
