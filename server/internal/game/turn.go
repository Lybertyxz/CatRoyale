package game

import "fmt"

type TurnProcessor struct {
	match *Match
}

func NewTurnProcessor(match *Match) *TurnProcessor {
	return &TurnProcessor{match: match}
}

func (t *TurnProcessor) ProcessAction(action PlayerAction) error {
	if t.match.Status != MatchStatusInProgress {
		return fmt.Errorf("match is not in progress")
	}
	if action.PlayerID != t.match.CurrentPlayerID() {
		return fmt.Errorf("not your turn")
	}
	if t.match.IsTimeExpired() {
		t.endTurn()
		return nil
	}

	switch action.Type {
	case ActionMove:
		return t.processMove(action)
	case ActionAbility:
		return t.processAbility(action)
	case ActionSkip:
		t.endTurn()
		return nil
	default:
		return fmt.Errorf("unknown action type: %s", action.Type)
	}
}

func (t *TurnProcessor) processMove(action PlayerAction) error {
	ts := t.match.Turn

	if ts.RemainingPM <= 0 {
		return fmt.Errorf("no PM remaining")
	}

	piece := t.match.Board.GetPiece(action.PiecePos)
	if piece == nil {
		return fmt.Errorf("no piece at position")
	}
	if piece.OwnerID != action.PlayerID {
		return fmt.Errorf("not your piece")
	}
	if !piece.IsAlive {
		return fmt.Errorf("piece is dead")
	}
	if hasState(piece, StateFreeze) || hasState(piece, StateStun) {
		return fmt.Errorf("piece is immobilized")
	}

	pieceKey := piece.TemplateID + piece.OwnerID
	if ts.HasMoved(pieceKey) {
		return fmt.Errorf("piece already moved this turn")
	}

	tmpl, ok := t.match.Templates[piece.TemplateID]
	if !ok {
		return fmt.Errorf("template not found")
	}
	if !t.isValidMove(piece, action.TargetPos, tmpl) {
		return fmt.Errorf("invalid move")
	}

	t.match.Board.MovePiece(action.PiecePos, action.TargetPos)
	ts.RegisterMove(pieceKey)

	// Auto end turn si PA et PM épuisés
	if ts.RemainingPA <= 0 && ts.RemainingPM <= 0 {
		t.endTurn()
	}
	return nil
}

func (t *TurnProcessor) processAbility(action PlayerAction) error {
	ts := t.match.Turn

	if ts.RemainingPA <= 0 {
		return fmt.Errorf("no PA remaining")
	}

	caster := t.match.Board.GetPiece(action.PiecePos)
	if caster == nil {
		return fmt.Errorf("piece not found")
	}
	if caster.OwnerID != action.PlayerID {
		return fmt.Errorf("not your piece")
	}
	if !caster.IsAlive {
		return fmt.Errorf("piece is dead")
	}
	if hasState(caster, StateStun) {
		return fmt.Errorf("piece is stunned")
	}

	cd, onCooldown := caster.AbilityCooldowns[action.AbilityID]
	if onCooldown && cd > 0 {
		return fmt.Errorf("ability on cooldown: %d turns remaining", cd)
	}

	tmpl, ok := t.match.Templates[caster.TemplateID]
	if !ok {
		return fmt.Errorf("template not found")
	}

	var ability *Ability
	for i := range tmpl.Abilities {
		if tmpl.Abilities[i].ID == action.AbilityID {
			ability = &tmpl.Abilities[i]
			break
		}
	}
	if ability == nil {
		return fmt.Errorf("ability not found")
	}
	if ability.Type == AbilityPassive {
		return fmt.Errorf("cannot manually trigger passive ability")
	}

	pieceKey := caster.TemplateID + caster.OwnerID
	if ts.HasUsedAbility(pieceKey, action.AbilityID) {
		return fmt.Errorf("ability already used this turn")
	}

	target := t.match.Board.GetPiece(action.TargetPos)
	t.applyAbilityEffects(caster, target, ability)

	if ability.Cooldown > 0 {
		caster.AbilityCooldowns[action.AbilityID] = ability.Cooldown
	}
	ts.RegisterAbility(pieceKey, action.AbilityID)

	// Auto end turn si PA et PM épuisés
	if ts.RemainingPA <= 0 && ts.RemainingPM <= 0 {
		t.endTurn()
	}
	return nil
}

func (t *TurnProcessor) applyDamage(attacker, target *PieceInstance, rawDamage int, dmgType DamageType) {
	if target == nil {
		return
	}
	damage := rawDamage
	switch dmgType {
	case DamageTypeNormal:
		targetTmpl := t.match.Templates[target.TemplateID]
		if targetTmpl != nil {
			damage -= targetTmpl.Armor
		}
	}
	if damage < 0 {
		damage = 0
	}
	target.CurrentHP -= damage
}

func (t *TurnProcessor) applyAbilityEffects(caster, target *PieceInstance, ability *Ability) {
	for _, effect := range ability.Effects {
		t.applyEffect(caster, target, effect)
	}
}

func (t *TurnProcessor) applyEffect(caster, target *PieceInstance, effect AbilityEffect) {
	if target == nil {
		return
	}
	switch effect.Type {
	case EffectDamage:
		t.applyDamage(caster, target, effect.Value, DamageTypeNormal)
	case EffectPiercingDamage:
		t.applyDamage(caster, target, effect.Value, DamageTypePiercing)
	case EffectTrueDamage:
		t.applyDamage(caster, target, effect.Value, DamageTypeTrue)
	case EffectHeal:
		tmpl := t.match.Templates[target.TemplateID]
		if tmpl != nil {
			target.CurrentHP = min(target.CurrentHP+effect.Value, tmpl.MaxHP)
		}
	default:
		addState(target, StatusEffect{
			Type:     effect.Type,
			Duration: effect.Duration,
			Value:    effect.Value,
		})
	}
	t.checkDeath(target)
}

func (t *TurnProcessor) checkDeath(piece *PieceInstance) {
	if piece == nil || piece.CurrentHP > 0 {
		return
	}
	piece.IsAlive = false
	piece.CurrentHP = 0
	t.match.Board.RemovePiece(piece.Position)
}

func (t *TurnProcessor) endTurn() {
	t.tickAllCooldowns()
	t.tickAllStates()
	t.match.SwitchTurn()
}

func (t *TurnProcessor) tickAllCooldowns() {
	for y := 0; y < BoardSize; y++ {
		for x := 0; x < BoardSize; x++ {
			piece := t.match.Board.Cells[y][x]
			if piece == nil {
				continue
			}
			for id, cd := range piece.AbilityCooldowns {
				if cd > 0 {
					piece.AbilityCooldowns[id] = cd - 1
				}
			}
		}
	}
}

func (t *TurnProcessor) tickAllStates() {
	for y := 0; y < BoardSize; y++ {
		for x := 0; x < BoardSize; x++ {
			piece := t.match.Board.Cells[y][x]
			if piece == nil {
				continue
			}
			t.tickPieceStates(piece)
		}
	}
}

func (t *TurnProcessor) tickPieceStates(piece *PieceInstance) {
	remaining := piece.ActiveStates[:0]
	for _, state := range piece.ActiveStates {
		switch state.Type {
		case EffectPoison, EffectBurn:
			t.applyDamage(nil, piece, state.Value, DamageTypeTrue)
			t.checkDeath(piece)
		case EffectRegeneration:
			tmpl := t.match.Templates[piece.TemplateID]
			if tmpl != nil {
				piece.CurrentHP = min(piece.CurrentHP+state.Value, tmpl.MaxHP)
			}
		}
		state.Duration--
		if state.Duration > 0 {
			remaining = append(remaining, state)
		}
	}
	piece.ActiveStates = remaining
}

func (t *TurnProcessor) isValidMove(piece *PieceInstance, target Position, tmpl *PieceTemplate) bool {
	if !t.match.Board.IsValidPosition(target) {
		return false
	}
	if t.match.Board.GetPiece(target) != nil {
		return false
	}
	dx := abs(target.X - piece.Position.X)
	dy := abs(target.Y - piece.Position.Y)

	switch tmpl.MovementPattern.Type {
	case MovementLinear:
		return (dx == 0 || dy == 0) && max(dx, dy) <= tmpl.MovementPattern.Range
	case MovementDiagonal:
		return dx == dy && dx <= tmpl.MovementPattern.Range
	case MovementOmnidirectional:
		return max(dx, dy) <= tmpl.MovementPattern.Range
	case MovementCustom:
		rel := Position{X: target.X - piece.Position.X, Y: target.Y - piece.Position.Y}
		for _, c := range tmpl.MovementPattern.Custom {
			if c.X == rel.X && c.Y == rel.Y {
				return true
			}
		}
		return false
	}
	return false
}

func abs(x int) int {
	if x < 0 {
		return -x
	}
	return x
}

func min(a, b int) int {
	if a < b {
		return a
	}
	return b
}

func max(a, b int) int {
	if a > b {
		return a
	}
	return b
}