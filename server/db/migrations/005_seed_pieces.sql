-- +goose Up

INSERT INTO piece_templates (id, name, role, rarity, slot_cost, max_hp, attack, armor, attack_range, move_range, can_jump, movement_type, movement_custom, abilities) VALUES

-- PION : Biscuit — Common, simple mais utile
('biscuit_001', 'Biscuit', 'pawn', 'common', 1, 80, 15, 5, 1, 1, false, 'linear',
NULL,
'[
  {
    "id": "biscuit_passive_001",
    "name": "Tenace",
    "type": "passive",
    "cooldown": 0,
    "description": "Réduit les dégâts reçus de 5 quand HP < 40.",
    "effects": [{"type": "shield", "value": 5, "duration": 0}]
  }
]'),

-- TOUR : Granite — Rare, tank défensif
('granite_001', 'Granite', 'rook', 'rare', 2, 160, 20, 20, 1, 2, false, 'linear',
NULL,
'[
  {
    "id": "granite_active_001",
    "name": "Forteresse",
    "type": "active",
    "cooldown": 3,
    "description": "Gagne un bouclier de 40 HP pendant 2 tours.",
    "effects": [{"type": "shield", "value": 40, "duration": 2}]
  },
  {
    "id": "granite_passive_001",
    "name": "Peau de Pierre",
    "type": "passive",
    "cooldown": 0,
    "description": "Réduit les dégâts de type normal de 10%.",
    "effects": [{"type": "shield", "value": 10, "duration": 0}]
  }
]'),

-- CAVALIER : Whisker — Rare, mobile et imprévisible
('whisker_001', 'Whisker', 'knight', 'rare', 2, 100, 30, 5, 1, 2, true, 'custom',
'[{"x": 2, "y": 1}, {"x": 2, "y": -1}, {"x": -2, "y": 1}, {"x": -2, "y": -1}, {"x": 1, "y": 2}, {"x": 1, "y": -2}, {"x": -1, "y": 2}, {"x": -1, "y": -2}]',
'[
  {
    "id": "whisker_active_001",
    "name": "Charge Féline",
    "type": "active",
    "cooldown": 3,
    "description": "Bondit sur une case adjacente et inflige 45 dégâts perforants.",
    "effects": [{"type": "piercing_damage", "value": 45, "duration": 0}]
  }
]'),

-- FOU : Luna — Epic, empoisonneuse à distance
('luna_001', 'Luna', 'bishop', 'epic', 3, 90, 35, 0, 3, 3, false, 'diagonal',
NULL,
'[
  {
    "id": "luna_active_001",
    "name": "Brume Lunaire",
    "type": "active",
    "cooldown": 2,
    "description": "Empoisonne la cible pendant 3 tours (8 dégâts/tour).",
    "effects": [{"type": "poison", "value": 8, "duration": 3}]
  },
  {
    "id": "luna_passive_001",
    "name": "Ombre",
    "type": "passive",
    "cooldown": 0,
    "description": "Les attaques de Luna ignorent 5 points d armure.",
    "effects": [{"type": "piercing_damage", "value": 5, "duration": 0}]
  }
]'),

-- REINE : Tempête — Epic, dévastatrice de zone
('tempete_001', 'Tempête', 'queen', 'epic', 4, 120, 40, 10, 2, 3, false, 'omni',
NULL,
'[
  {
    "id": "tempete_active_001",
    "name": "Tempête Féline",
    "type": "active",
    "cooldown": 4,
    "description": "Inflige 30 dégâts vrais à toutes les pièces dans un rayon de 2 cases.",
    "effects": [{"type": "true_damage", "value": 30, "duration": 0}]
  },
  {
    "id": "tempete_passive_001",
    "name": "Frénésie",
    "type": "passive",
    "cooldown": 0,
    "description": "Gagne +5 ATK à chaque tour.",
    "effects": [{"type": "strengthen", "value": 5, "duration": 0}]
  }
]'),

-- ROI : Pharaon — Legendary, le roi à protéger
('pharaon_001', 'Pharaon', 'king', 'legendary', 5, 220, 20, 25, 1, 1, false, 'omni',
NULL,
'[
  {
    "id": "pharaon_active_001",
    "name": "Décret Royal",
    "type": "active",
    "cooldown": 5,
    "description": "Soigne toutes les pièces alliées adjacentes de 30 HP.",
    "effects": [{"type": "heal", "value": 30, "duration": 0}]
  },
  {
    "id": "pharaon_passive_001",
    "name": "Aura Royale",
    "type": "passive",
    "cooldown": 0,
    "description": "Les pièces alliées adjacentes gagnent +5 armure.",
    "effects": [{"type": "strengthen", "value": 5, "duration": 0}]
  }
]');

-- +goose Down
DELETE FROM piece_templates WHERE id IN (
    'biscuit_001', 'granite_001', 'whisker_001',
    'luna_001', 'tempete_001', 'pharaon_001'
);