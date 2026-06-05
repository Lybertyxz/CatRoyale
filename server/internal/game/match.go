package game

import "time"

type MatchStatus string

const (
	MatchStatusWaiting    MatchStatus = "waiting"
	MatchStatusPlacement  MatchStatus = "placement"
	MatchStatusInProgress MatchStatus = "in_progress"
	MatchStatusFinished   MatchStatus = "finished"
)

type TurnAction string

const (
	ActionMove    TurnAction = "move"
	ActionAbility TurnAction = "ability"
	ActionSkip    TurnAction = "skip"
)

// PlayerAction représente l'action d'un joueur pendant son tour
type PlayerAction struct {
	PlayerID  string     `json:"player_id"`
	Type      TurnAction `json:"type"`
	PiecePos  Position   `json:"piece_pos"`
	TargetPos Position   `json:"target_pos"`
	AbilityID string     `json:"ability_id"`
}

// TurnState tracks PA/PM usage for the current turn
type TurnState struct {
	RemainingPM  int               `json:"remaining_pm"`
	RemainingPA  int               `json:"remaining_pa"`
	MovedPieces  map[string]bool   `json:"moved_pieces"`  // templateID+ownerID → moved
	UsedAbilities map[string][]string `json:"used_abilities"` // templateID+ownerID → []abilityID
}

func NewTurnState() *TurnState {
	return &TurnState{
		RemainingPM:   1,
		RemainingPA:   1,
		MovedPieces:   make(map[string]bool),
		UsedAbilities: make(map[string][]string),
	}
}

func (t *TurnState) HasMoved(pieceKey string) bool {
	return t.MovedPieces[pieceKey]
}

func (t *TurnState) HasUsedAbility(pieceKey, abilityID string) bool {
	for _, id := range t.UsedAbilities[pieceKey] {
		if id == abilityID {
			return true
		}
	}
	return false
}

func (t *TurnState) RegisterMove(pieceKey string) {
	t.MovedPieces[pieceKey] = true
	t.RemainingPM--
}

func (t *TurnState) RegisterAbility(pieceKey, abilityID string) {
	t.UsedAbilities[pieceKey] = append(t.UsedAbilities[pieceKey], abilityID)
	t.RemainingPA--
}

// Match représente une partie en cours
type Match struct {
	ID            string                    `json:"id"`
	Status        MatchStatus               `json:"status"`
	PlayerIDs     [2]string                 `json:"player_ids"`
	Board         *Board                    `json:"board"`
	Templates     map[string]*PieceTemplate `json:"-"`
	CurrentTurn   int                       `json:"current_turn"`
	TurnNumber    int                       `json:"turn_number"`
	TurnStartedAt time.Time                 `json:"turn_started_at"`
	TimeBanks     [2]time.Duration          `json:"time_banks"` // Fischer clock
	WinnerID      string                    `json:"winner_id"`
	CreatedAt     time.Time                 `json:"created_at"`
	UpdatedAt     time.Time                 `json:"updated_at"`
	Turn          *TurnState                `json:"turn_state"`
}

const DefaultTimeBank = 5 * 60 * time.Second // 5 minutes par joueur

func NewMatch(id string, player1ID, player2ID string) *Match {
	return &Match{
		ID:        id,
		Status:    MatchStatusWaiting,
		PlayerIDs: [2]string{player1ID, player2ID},
		Board:     NewBoard(),
		Templates: make(map[string]*PieceTemplate),
		TurnNumber: 1,
		TimeBanks:  [2]time.Duration{DefaultTimeBank, DefaultTimeBank},
		TurnStartedAt: time.Now(),
		CreatedAt:  time.Now(),
		UpdatedAt:  time.Now(),
		Turn:       NewTurnState(),
	}
}

func (m *Match) CurrentPlayerID() string {
	return m.PlayerIDs[m.CurrentTurn]
}

// TimeRemaining retourne le temps restant dans la banque du joueur actif
func (m *Match) TimeRemaining() time.Duration {
	elapsed := time.Since(m.TurnStartedAt)
	remaining := m.TimeBanks[m.CurrentTurn] - elapsed
	if remaining < 0 {
		return 0
	}
	return remaining
}

func (m *Match) IsTimeExpired() bool {
	return m.TimeRemaining() <= 0
}

// SwitchTurn passe au joueur suivant et reset le TurnState
func (m *Match) SwitchTurn() {
	// Déduit le temps écoulé de la banque du joueur actif
	elapsed := time.Since(m.TurnStartedAt)
	m.TimeBanks[m.CurrentTurn] -= elapsed
	if m.TimeBanks[m.CurrentTurn] < 0 {
		m.TimeBanks[m.CurrentTurn] = 0
	}

	m.CurrentTurn = (m.CurrentTurn + 1) % 2
	if m.CurrentTurn == 0 {
		m.TurnNumber++
	}

	m.TurnStartedAt = time.Now()
	m.Turn = NewTurnState()
	m.UpdatedAt = time.Now()
}

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

func (m *Match) CheckWinner() string {
	for _, playerID := range m.PlayerIDs {
		king := m.GetKing(playerID)
		if king == nil || !king.IsAlive || king.CurrentHP <= 0 {
			for _, id := range m.PlayerIDs {
				if id != playerID {
					return id
				}
			}
		}
	}
	// Vérifie time banks
	for i, bank := range m.TimeBanks {
		if bank <= 0 {
			return m.PlayerIDs[(i+1)%2]
		}
	}
	return ""
}