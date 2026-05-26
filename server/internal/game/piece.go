package game

// Rarity définit la rareté d'une pièce
type Rarity string

const (
	RarityCommon    Rarity = "common"
	RarityRare      Rarity = "rare"
	RarityEpic      Rarity = "epic"
	RarityLegendary Rarity = "legendary"
)

// PieceRole représente le rôle échecs de la pièce
type PieceRole string

const (
	RolePawn   PieceRole = "pawn"
	RoleRook   PieceRole = "rook"
	RoleKnight PieceRole = "knight"
	RoleBishop PieceRole = "bishop"
	RoleQueen  PieceRole = "queen"
	RoleKing   PieceRole = "king"
)

// PieceTemplate est la définition statique d'une pièce (carte)
type PieceTemplate struct {
	ID              string          `json:"id"`
	Name            string          `json:"name"`
	Role            PieceRole       `json:"role"`
	Rarity          Rarity          `json:"rarity"`
	SlotCost        int             `json:"slot_cost"`
	MaxHP           int             `json:"max_hp"`
	Attack          int             `json:"attack"`
	Armor           int             `json:"armor"`
	AttackRange     int             `json:"attack_range"`
	MovementPattern MovementPattern `json:"movement_pattern"`
	Abilities       []Ability       `json:"abilities"`
}

// MovementPattern définit comment une pièce se déplace
type MovementPattern struct {
	Type       MovementType `json:"type"`
	Range      int          `json:"range"`      // nombre de cases max, -1 = illimité
	CanJump    bool         `json:"can_jump"`   // peut sauter par dessus d'autres pièces
	Directions []Direction  `json:"directions"` // directions autorisées
	Custom     []Position   `json:"custom"`     // cases relatives custom (ex: cavalier)
}

type MovementType string

const (
	MovementLinear          MovementType = "linear"   // lignes droites
	MovementDiagonal        MovementType = "diagonal" // diagonales
	MovementOmnidirectional MovementType = "omni"     // toutes directions
	MovementCustom          MovementType = "custom"   // pattern custom
)

type Direction string

const (
	DirForward  Direction = "forward"
	DirBackward Direction = "backward"
	DirLeft     Direction = "left"
	DirRight    Direction = "right"
	DirDiagAll  Direction = "diag_all"
)

// Ability représente une capacité active ou passive
type Ability struct {
	ID          string          `json:"id"`
	Name        string          `json:"name"`
	Type        AbilityType     `json:"type"`
	Cooldown    int             `json:"cooldown"`
	Description string          `json:"description"`
	Effects     []AbilityEffect `json:"effects"`
}

type AbilityType string

const (
	AbilityPassive AbilityType = "passive"
	AbilityActive  AbilityType = "active"
)

// PieceInstance est une pièce en jeu sur l'échiquier
type PieceInstance struct {
	TemplateID       string         `json:"template_id"`
	OwnerID          string         `json:"owner_id"`
	CurrentHP        int            `json:"current_hp"`
	Position         Position       `json:"position"`
	AbilityCooldowns map[string]int `json:"ability_cooldowns"`
	ActiveStates     []StatusEffect `json:"active_states"`
	IsAlive          bool           `json:"is_alive"`
}

// Position représente une case sur l'échiquier
type Position struct {
	X int `json:"x"` // 0-7
	Y int `json:"y"` // 0-7
}

// hasState vérifie si une pièce a un état actif
func hasState(piece *PieceInstance, stateType StateType) bool {
	for _, s := range piece.ActiveStates {
		if s.Type == stateType && s.Duration > 0 {
			return true
		}
	}
	return false
}

// addState ajoute ou rafraîchit un état sur une pièce
func addState(piece *PieceInstance, effect StatusEffect) {
	for i, s := range piece.ActiveStates {
		if s.Type == effect.Type {
			piece.ActiveStates[i].Duration = effect.Duration
			piece.ActiveStates[i].Value = effect.Value
			return
		}
	}
	piece.ActiveStates = append(piece.ActiveStates, effect)
}
