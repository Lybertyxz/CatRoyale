package game

import (
	"encoding/json"

	"github.com/Lybertyxz/CatRoyale/server/internal/store/postgres"
)

// ConvertTemplates convertit les templates postgres en templates game
func ConvertTemplates(rows []postgres.PieceTemplate) map[string]*PieceTemplate {
	templates := make(map[string]*PieceTemplate)
	for _, row := range rows {
		tmpl := convertTemplate(row)
		templates[tmpl.ID] = tmpl
	}
	return templates
}

func convertTemplate(row postgres.PieceTemplate) *PieceTemplate {
	tmpl := &PieceTemplate{
		ID:          row.ID,
		Name:        row.Name,
		Role:        PieceRole(row.Role),
		Rarity:      Rarity(row.Rarity),
		SlotCost:    int(row.SlotCost),
		MaxHP:       int(row.MaxHp),
		Attack:      int(row.Attack),
		Armor:       int(row.Armor),
		AttackRange: int(row.AttackRange),
		MovementPattern: MovementPattern{
			Type:    MovementType(row.MovementType),
			Range:   int(row.MoveRange),
			CanJump: row.CanJump,
		},
	}

	// Parse custom movement pattern
	if len(row.MovementCustom) > 0 {
		var custom []Position
		if err := json.Unmarshal(row.MovementCustom, &custom); err == nil {
			tmpl.MovementPattern.Custom = custom
		}
	}

	// Parse abilities
	if len(row.Abilities) > 0 {
		var abilities []Ability
		if err := json.Unmarshal(row.Abilities, &abilities); err == nil {
			tmpl.Abilities = abilities
		}
	}

	return tmpl
}