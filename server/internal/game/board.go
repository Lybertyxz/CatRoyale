package game

const (
	BoardSize      = 8
	PlayerZoneRows = 2 // lignes de placement pour chaque joueur
)

// Board représente l'état complet de l'échiquier
type Board struct {
	Cells [BoardSize][BoardSize]*PieceInstance `json:"cells"`
}

// NewBoard crée un échiquier vide
func NewBoard() *Board {
	return &Board{}
}

// PlacePiece place une pièce sur une case
func (b *Board) PlacePiece(piece *PieceInstance, pos Position) bool {
	if !b.IsValidPosition(pos) {
		return false
	}
	if b.Cells[pos.Y][pos.X] != nil {
		return false
	}
	piece.Position = pos
	b.Cells[pos.Y][pos.X] = piece
	return true
}

// MovePiece déplace une pièce d'une case à une autre
func (b *Board) MovePiece(from, to Position) bool {
	if !b.IsValidPosition(from) || !b.IsValidPosition(to) {
		return false
	}
	piece := b.Cells[from.Y][from.X]
	if piece == nil {
		return false
	}
	if b.Cells[to.Y][to.X] != nil {
		return false
	}
	b.Cells[to.Y][to.X] = piece
	b.Cells[from.Y][from.X] = nil
	piece.Position = to
	return true
}

// GetPiece retourne la pièce à une position donnée
func (b *Board) GetPiece(pos Position) *PieceInstance {
	if !b.IsValidPosition(pos) {
		return nil
	}
	return b.Cells[pos.Y][pos.X]
}

// RemovePiece retire une pièce de l'échiquier
func (b *Board) RemovePiece(pos Position) {
	if b.IsValidPosition(pos) {
		b.Cells[pos.Y][pos.X] = nil
	}
}

// IsValidPosition vérifie qu'une position est dans les limites
func (b *Board) IsValidPosition(pos Position) bool {
	return pos.X >= 0 && pos.X < BoardSize &&
		pos.Y >= 0 && pos.Y < BoardSize
}

// IsPlayerZone vérifie qu'une position est dans la zone de placement d'un joueur
func (b *Board) IsPlayerZone(pos Position, playerIndex int) bool {
	if playerIndex == 0 {
		return pos.Y < PlayerZoneRows
	}
	return pos.Y >= BoardSize-PlayerZoneRows
}
