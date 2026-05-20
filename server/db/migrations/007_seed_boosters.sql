-- +goose Up
INSERT INTO booster_types (id, name, description, price_coins, price_gems, pieces_count, rarity_weights) VALUES

('booster_starter', 'Starter Pack', 'Parfait pour débuter — garanti au moins une pièce Rare.', 0, 0, 3,
'{"common": 60, "rare": 35, "epic": 5, "legendary": 0}'),

('booster_standard', 'Booster Standard', '3 pièces aléatoires.', 100, 0, 3,
'{"common": 55, "rare": 30, "epic": 12, "legendary": 3}'),

('booster_premium', 'Booster Premium', '5 pièces avec chances accrues de pièces rares.', 0, 50, 5,
'{"common": 30, "rare": 40, "epic": 22, "legendary": 8}'),

('booster_legendary', 'Booster Légendaire', 'Garantit au moins une pièce Épique ou Légendaire.', 0, 150, 5,
'{"common": 0, "rare": 30, "epic": 50, "legendary": 20}');

-- +goose Down
DELETE FROM booster_types WHERE id IN (
    'booster_starter', 'booster_standard', 'booster_premium', 'booster_legendary'
);