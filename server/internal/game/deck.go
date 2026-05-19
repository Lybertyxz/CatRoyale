package game

const MaxDeckSlots = 20 // slots totaux disponibles par deck

// DeckEntry représente une pièce placée dans le deck avec sa position initiale
type DeckEntry struct {
	TemplateID    string   `json:"template_id"`
	StartPosition Position `json:"start_position"` // position choisie par le joueur
}

// Deck représente le deck construit par un joueur
type Deck struct {
	ID         string      `json:"id"`
	OwnerID    string      `json:"owner_id"`
	Name       string      `json:"name"`
	Entries    []DeckEntry `json:"entries"`
	TotalSlots int         `json:"total_slots"` // slots utilisés
}

// IsValid vérifie que le deck est valide avant une partie
func (d *Deck) IsValid(templates map[string]*PieceTemplate, playerIndex int, board *Board) bool {
	if len(d.Entries) == 0 {
		return false
	}

	totalSlots := 0
	hasKing := false

	for _, entry := range d.Entries {
		tmpl, ok := templates[entry.TemplateID]
		if !ok {
			return false
		}

		// Vérifie que la position est dans la zone du joueur
		if !board.IsPlayerZone(entry.StartPosition, playerIndex) {
			return false
		}

		if tmpl.Role == RoleKing {
			hasKing = true
		}

		totalSlots += tmpl.SlotCost
	}

	// Un deck doit avoir un roi et ne pas dépasser les slots max
	return hasKing && totalSlots <= MaxDeckSlots
}

// ToInstances convertit le deck en pièces instanciées pour une partie
func (d *Deck) ToInstances(templates map[string]*PieceTemplate) []*PieceInstance {
	instances := make([]*PieceInstance, 0, len(d.Entries))
	for _, entry := range d.Entries {
		tmpl, ok := templates[entry.TemplateID]
		if !ok {
			continue
		}
		instances = append(instances, &PieceInstance{
			TemplateID:       tmpl.ID,
			OwnerID:          d.OwnerID,
			CurrentHP:        tmpl.MaxHP,
			Position:         entry.StartPosition,
			AbilityCooldowns: make(map[string]int),
			IsAlive:          true,
		})
	}
	return instances
}
