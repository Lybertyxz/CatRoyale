package game

import "time"

type MatchStatus string

const DefaultTurnTimeSeconds = 60 // temps par tour en secondes

const (
	MatchStatusWaiting    MatchStatus = "waiting"
	MatchStatusPlacement  MatchStatus = "placement" // joueurs placent leurs pièces
	MatchStatusInProgress MatchStatus = "in_progress"
	MatchStatusFinished   MatchStatus = "finished"
)

type TurnAction string

const (
	ActionMove    TurnAction = "move"
	ActionAttack  TurnAction = "attack"
	ActionAbility TurnAction = "ability"
	ActionSkip    TurnAction = "skip"
)

// PlayerAction représente l'action d'un joueur pendant son tour
type PlayerAction struct {
	PlayerID  string     `json:"player_id"`
	Type      TurnAction `json:"type"`
	PiecePos  Position   `json:"piece_pos"`  // position de la pièce à bouger/attaquer
	TargetPos Position   `json:"target_pos"` // position cible
	AbilityID string     `json:"ability_id"` // si action = ability
}

// Match représente une partie en cours
type Match struct {
	ID            string                    `json:"id"`
	Status        MatchStatus               `json:"status"`
	PlayerIDs     [2]string                 `json:"player_ids"`
	Decks         [2]*Deck                  `json:"decks"`
	Board         *Board                    `json:"board"`
	Templates     map[string]*PieceTemplate `json:"-"`
	CurrentTurn   int                       `json:"current_turn"`
	TurnNumber    int                       `json:"turn_number"`
	TurnStartedAt time.Time                 `json:"turn_started_at"`
	TurnDuration  time.Duration             `json:"turn_duration"`
	WinnerID      string                    `json:"winner_id"`
	CreatedAt     time.Time                 `json:"created_at"`
	UpdatedAt     time.Time                 `json:"updated_at"`
}

// NewMatch crée une nouvelle partie
func NewMatch(id string, player1ID, player2ID string) *Match {
	return &Match{
		ID:           id,
		Status:       MatchStatusWaiting,
		PlayerIDs:    [2]string{player1ID, player2ID},
		Board:        NewBoard(),
		Templates:    make(map[string]*PieceTemplate),
		TurnNumber:   1,
		TurnDuration: DefaultTurnTimeSeconds * time.Second,
		CreatedAt:    time.Now(),
		UpdatedAt:    time.Now(),
	}
}

// TurnTimeRemaining retourne le temps restant pour le tour en cours
func (m *Match) TurnTimeRemaining() time.Duration {
	elapsed := time.Since(m.TurnStartedAt)
	remaining := m.TurnDuration - elapsed
	if remaining < 0 {
		return 0
	}
	return remaining
}

// IsTurnExpired vérifie si le temps du tour est écoulé
func (m *Match) IsTurnExpired() bool {
	return time.Since(m.TurnStartedAt) >= m.TurnDuration
}

// CurrentPlayerID retourne l'ID du joueur dont c'est le tour
func (m *Match) CurrentPlayerID() string {
	return m.PlayerIDs[m.CurrentTurn]
}

// SwitchTurn passe au joueur suivant
func (m *Match) SwitchTurn() {
	m.CurrentTurn = (m.CurrentTurn + 1) % 2
	if m.CurrentTurn == 0 {
		m.TurnNumber++
	}
	m.TurnStartedAt = time.Now()
	m.UpdatedAt = time.Now()
}

// GetKing retourne le roi d'un joueur
func (m *Match) GetKing(playerID string) *PieceInstance {
	for y := 0; y < BoardSize; y++ {
		for x := 0; x < BoardSize; x++ {
			piece := m.Board.Cells[y][x]
			if piece == nil {
				continue
			}
			tmpl, ok := m.Templates[piece.TemplateID]
			if !ok {
				continue
			}
			if piece.OwnerID == playerID && tmpl.Role == RoleKing {
				return piece
			}
		}
	}
	return nil
}

// CheckWinner vérifie si un joueur a gagné
func (m *Match) CheckWinner() string {
	for _, playerID := range m.PlayerIDs {
		king := m.GetKing(playerID)
		if king == nil || !king.IsAlive || king.CurrentHP <= 0 {
			// L'adversaire gagne
			for _, id := range m.PlayerIDs {
				if id != playerID {
					return id
				}
			}
		}
	}
	return ""
}
