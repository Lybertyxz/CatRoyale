# Cat Royale — Asset Generation Prompts

## Style Reference
Toujours joindre `CatRoyaleExemple.jpeg` comme image de référence quand le service le permet (Midjourney `/imagine` avec `--iw 1.5`, Adobe Firefly "Reference Image", Leonardo.ai "Image Reference").

## Style Guide (à inclure dans tous les prompts personnages)
```
chibi cat character, fantasy RPG equipment, bold black outlines,
flat cel-shading, vibrant saturated colors, white background,
full body front-facing, mobile game art style, 2D sprite,
big expressive eyes, cute kawaii aesthetic, high contrast colors
```

---

## 1. PERSONNAGES

### 1.1 Prompt Base Personnage
> Utiliser pour générer un nouveau personnage. Remplacer les variables [MAJUSCULES].

**Avec image de référence (recommandé) :**
```
[DESCRIPTION_PERSONNAGE], chibi cat character, fantasy RPG equipment,
bold black outlines, flat cel-shading, vibrant saturated colors,
white background, full body front-facing, mobile game art style,
2D sprite, big expressive eyes, cute kawaii aesthetic,
high contrast colors, --ar 1:1 --style raw --iw 1.5
```

**Variables à remplacer :**
- `[DESCRIPTION_PERSONNAGE]` — description du personnage et de son équipement

**Exemples par rôle :**
| Rôle | Description |
|------|-------------|
| Pawn | small cat warrior holding a wooden shield and short sword, leather armor, blue outfit |
| Rook | sturdy cat soldier holding a large tower shield, heavy iron armor, grey outfit |
| Knight | agile cat with a lance and cape, light armor, green outfit, riding pose |
| Bishop | cat mage with a wooden staff and spell book, purple robe and hood |
| Queen | elegant cat queen with a magic wand and crown, royal dress, gold and red |
| King | majestic cat king with a golden crown and royal mantle, sword, white and gold fur coat |

---

### 1.2 Prompt Sprite Sheet (Poses d'animation)
> Générer toutes les poses d'un personnage en une seule image pour découpage.
> Joindre le sprite idle déjà validé du personnage comme référence.

```
sprite sheet of [NOM_PERSONNAGE] chibi cat character,
6 animation poses on white background, labeled poses:
[1-IDLE standing calm] [2-WALK stepping forward]
[3-ATTACK striking with weapon] [4-HIT recoiling backward]
[5-CAST raising weapon/hands with glow] [6-DEATH falling down],
same art style, bold black outlines, flat cel-shading,
vibrant colors, consistent character design, 2D mobile game sprite,
arranged in 2 rows of 3, white background, --ar 3:2 --style raw
```

---

### 1.3 Prompt Animation Frame par Frame
> Pour générer chaque animation séparément avec plus de détail.

**IDLE (loop) :**
```
[NOM_PERSONNAGE] chibi cat character idle animation frame [N/4],
slight breathing movement, calm standing pose,
same art style as reference, white background,
bold outlines, flat colors, --ar 1:1 --style raw
```

**MOVE (déplacement) :**
```
[NOM_PERSONNAGE] chibi cat character walking frame [N/4],
stepping forward with weapon ready,
same art style as reference, white background,
bold outlines, flat colors, --ar 1:1 --style raw
```

**ATTACK (attaque basique) :**
```
[NOM_PERSONNAGE] chibi cat character attack animation frame [N/4],
[DESCRIPTION_ATTAQUE], aggressive action pose,
motion lines, impact effect,
same art style as reference, white background,
bold outlines, flat colors, --ar 1:1 --style raw
```

**HIT (recevoir des dégâts) :**
```
[NOM_PERSONNAGE] chibi cat character taking damage frame [N/3],
recoiling backward, surprised expression, pain reaction,
small impact stars around body,
same art style as reference, white background,
bold outlines, flat colors, --ar 1:1 --style raw
```

**DEATH :**
```
[NOM_PERSONNAGE] chibi cat character death animation frame [N/4],
falling down, defeated expression, spiral eyes,
same art style as reference, white background,
bold outlines, flat colors, --ar 1:1 --style raw
```

**ABILITY / CAST :**
```
[NOM_PERSONNAGE] chibi cat character using special ability [NOM_ABILITY],
[DESCRIPTION_EFFET_VISUEL], dramatic pose, glowing effect,
same art style as reference, white background,
bold outlines, flat colors, --ar 1:1 --style raw
```

---

### 1.4 Descriptions d'attaque par personnage

| Personnage | Attaque basique | Ability |
|------------|----------------|---------|
| Biscuit | swinging short sword forward | charging forward with shield bash, speed lines |
| Granite | slamming shield into ground, shockwave | surrounded by stone armor glow, defensive stance |
| Whisker | leaping L-shape jump strike with lance | aerial spin attack with lance extended |
| Luna | shooting diagonal magic beam from staff | releasing purple poison cloud from staff tip |
| Tempête | slashing with magic sword in all directions | full body lightning explosion, AoE shockwave |
| Pharaon | royal sword strike with golden glow | raising staff, golden healing light radiating outward |

---

## 2. UI ELEMENTS

### 2.1 Prompt Base UI
> Pour tous les éléments d'interface. Joindre `CatRoyaleExample2.jpeg` comme référence.

```
mobile game UI [ELEMENT], cat royale fantasy theme,
dark navy blue background, gold accents, rounded corners,
bold clean design, flat style, 2D game asset,
transparent background PNG, --ar [RATIO] --style raw
```

---

### 2.2 Boutons
```
mobile game button "[TEXTE_BOUTON]", cat royale fantasy theme,
large rounded rectangle, gold border, gradient fill [COULEUR],
embossed 3D effect, bold text, shine highlight on top,
transparent background, game UI asset, --ar 3:1 --style raw
```

**Couleurs par type :**
| Type | Couleur |
|------|---------|
| Primary (Battle/Play) | orange to yellow gradient |
| Secondary (Deck/Collection) | blue to purple gradient |
| Danger (Delete) | red to dark red gradient |
| Neutral (Back/Cancel) | grey to dark grey gradient |

---

### 2.3 Cartes de personnage (Card Frame)
```
mobile game card frame for cat character card,
[RARITY] rarity, rounded rectangle card border,
[COULEUR_RARITY] glowing border, dark background,
portrait area at top, stats area at bottom,
fantasy RPG style, game UI asset, transparent background,
--ar 2:3 --style raw
```

**Couleurs par rareté :**
| Rareté | Couleur |
|--------|---------|
| Common | silver grey, subtle shine |
| Rare | royal blue, soft glow |
| Epic | deep purple, sparkle effect |
| Legendary | gold orange, radiant glow, particle effects |

---

### 2.4 Icônes de rôle (Chess pieces icons)
```
chess piece icon [PIECE_TYPE], cute fantasy cat game style,
small icon, bold outlines, flat colors, [COULEUR] color scheme,
white background, pixel-clean edges, mobile game icon,
--ar 1:1 --style raw
```

**Par pièce :**
| Pièce | Couleur |
|-------|---------|
| Pawn | silver white |
| Rook | stone grey |
| Knight | forest green |
| Bishop | arcane purple |
| Queen | royal gold |
| King | bright gold with red |

---

### 2.5 Booster Packs
```
game booster pack [TYPE], cat paw print logo on front,
[COULEUR] colored pack with shine effect, 3D perspective slightly tilted,
fantasy game style, glowing edges, transparent background,
mobile game asset, --ar 2:3 --style raw
```

**Par type :**
| Pack | Couleur |
|------|---------|
| Starter | grey blue, simple |
| Standard | blue, silver shine |
| Premium | gold, bright shine |
| Legendary | purple, radiant particles |

---

### 2.6 Logo
```
"Cat Royale" game logo, fantasy cat theme,
bold stylized letters with cat paw and crown motif,
gold and white colors, dark outline, game title treatment,
mobile game logo, transparent background, --ar 3:1 --style raw
```

---

### 2.7 Backgrounds / Plateaux de jeu
```
chess board game background for cat royale mobile game,
[THEME] environment theme, top-down slight angle,
stylized 2D cartoon, vibrant colors, checkered pattern visible,
castle/forest/dungeon atmosphere, --ar 9:16 --style raw
```

**Thèmes disponibles :**
- Castle Courtyard — stone tiles, banners, blue sky
- Enchanted Forest — grass tiles, trees, magical lights
- Ancient Dungeon — dark stone, torches, shadows
- Snowy Kingdom — ice tiles, snow, northern lights
- Volcanic Arena — lava cracks, dark stone, fire glow

---

## 3. VFX / EFFETS

### 3.1 Effets de status
```
2D game status effect icon [EFFET], small circular icon,
bold outline, flat colors, transparent background,
mobile game UI, [COULEUR] color, --ar 1:1 --style raw
```

**Par effet :**
| Effet | Couleur | Description visuelle |
|-------|---------|---------------------|
| Poison | green | dripping liquid drops |
| Burn | orange red | small flame |
| Freeze | ice blue | snowflake crystal |
| Stun | yellow | spiral stars |
| Shield | blue white | shield bubble |
| Heal | green white | cross plus sign |
| Strengthen | orange | up arrow flame |
| Slow | grey blue | downward arrow |

---

### 3.2 Effets d'attaque (VFX sprites)
```
2D game attack effect [DESCRIPTION_EFFET], impact sprite,
transparent background, bold colors, cartoon style,
mobile game VFX, single frame, --ar 1:1 --style raw
```

---

## 4. WORKFLOW ÉTAPE PAR ÉTAPE

### Nouveau personnage
```
1. Générer sprite idle        → prompt 1.1 + image référence
2. Valider le design
3. Générer sprite sheet 6 poses → prompt 1.2 + sprite idle validé
4. Découper dans Photopea (gratuit) ou Photoshop
5. Importer dans Unity Assets/_Project/Art/Characters/[NOM]/Sprites/
6. Créer Animator Controller dans Unity
7. Créer clips d'animation dans Unity
8. Tester en jeu
```

### Nouveau asset UI
```
1. Générer l'asset         → prompt section 2 correspondante + CatRoyaleExample2.jpeg
2. Nettoyer le fond        → remove.bg (gratuit) si fond pas transparent
3. Optimiser la taille     → TinyPNG (gratuit)
4. Importer dans Unity     → Assets/_Project/Art/UI/[CATEGORIE]/
5. Configurer Sprite Mode  → Single ou Multiple selon le cas
6. Assigner dans la scène
```

---

## 5. OUTILS RECOMMANDÉS

| Outil | Usage | Prix |
|-------|-------|------|
| **Midjourney** | Personnages, backgrounds, UI | ~10$/mois |
| **Adobe Firefly** | UI elements, icons | Freemium |
| **Leonardo.ai** | Alternative Midjourney | Freemium |
| **Photopea** | Découpage, retouche | Gratuit |
| **remove.bg** | Suppression de fond | Freemium |
| **TinyPNG** | Compression PNG | Gratuit |
| **Spine 2D** | Animation 2D | 70$ one-time |
| **Rive** | Animations UI | Freemium |
| **Suno** | Musique | Freemium |
| **ElevenLabs** | Sons/voix | Freemium |

---

## 6. CONVENTIONS DE NOMMAGE

```
Personnages :   [nom]_[animation]_[frame].png
                ex: biscuit_idle_01.png
                ex: whisker_attack_03.png

UI :            ui_[categorie]_[element]_[variante].png
                ex: ui_button_play_normal.png
                ex: ui_card_border_legendary.png
                ex: ui_icon_role_knight.png

VFX :           vfx_[effet]_[frame].png
                ex: vfx_poison_01.png
                ex: vfx_attack_slash_03.png

Boards :        board_[theme].png
                ex: board_castle.png
```

