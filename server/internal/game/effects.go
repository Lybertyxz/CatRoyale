package game

// DamageType définit le type de dégâts
type DamageType int

const (
	DamageTypeNormal   DamageType = iota
	DamageTypePiercing            // ignore l'armure
	DamageTypeTrue                // ignore tout
)

// EffectType définit le type d'effet d'une capacité
type EffectType string

const (
	// Dégâts
	EffectDamage         EffectType = "damage"
	EffectPiercingDamage EffectType = "piercing_damage"
	EffectTrueDamage     EffectType = "true_damage"

	// Soins
	EffectHeal   EffectType = "heal"
	EffectShield EffectType = "shield"

	// États négatifs
	EffectPoison EffectType = "poison"
	EffectBurn   EffectType = "burn"
	EffectFreeze EffectType = "freeze"
	EffectStun   EffectType = "stun"
	EffectSlow   EffectType = "slow"
	EffectWeaken EffectType = "weaken"

	// États positifs
	EffectHaste        EffectType = "haste"
	EffectStrengthen   EffectType = "strengthen"
	EffectRegeneration EffectType = "regeneration"
	EffectInvisibility EffectType = "invisibility"

	// Déplacement forcé
	EffectKnockback EffectType = "knockback"
	EffectPull      EffectType = "pull"
	EffectTeleport  EffectType = "teleport"
)

// StatusState définit un état négatif ou positif
type StateType = EffectType

const (
	StateFreeze = EffectFreeze
	StateStun   = EffectStun
)

// StatusEffect représente un état actif sur une pièce
type StatusEffect struct {
	Type     EffectType `json:"type"`
	Duration int        `json:"duration"` // tours restants
	Value    int        `json:"value"`    // valeur (dégâts/tour, bonus etc.)
}

// AbilityEffect représente l'effet d'une capacité
type AbilityEffect struct {
	Type     EffectType `json:"type"`
	Value    int        `json:"value"`
	Duration int        `json:"duration"` // 0 = instantané
}
