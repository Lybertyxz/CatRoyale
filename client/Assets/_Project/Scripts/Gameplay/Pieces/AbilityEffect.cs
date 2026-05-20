using System;
using UnityEngine;

namespace CatRoyale.Gameplay
{
    [Serializable]
    public class AbilityEffect
    {
        public EffectType Type;
        public int Value;           // dégâts, soin, etc.
        public int Duration;        // en tours, 0 = instantané
        public bool Stackable;      // peut se cumuler

        [TextArea]
        public string Description;
    }

    public enum EffectType
    {
        // Dégâts
        Damage,
        PiercingDamage,     // ignore l'armure
        TrueDamage,         // ignore tous les effets défensifs

        // Soins
        Heal,
        Shield,             // absorbe les dégâts

        // États négatifs
        Poison,             // dégâts par tour
        Burn,               // dégâts par tour + réduit armure
        Freeze,             // immobilise
        Stun,               // passe le tour
        Slow,               // réduit le mouvement
        Weaken,             // réduit l'attaque

        // États positifs
        Haste,              // augmente le mouvement
        Strengthen,         // augmente l'attaque
        Regeneration,       // soin par tour
        Invisibility,       // ne peut pas être ciblé

        // Déplacement forcé
        Knockback,          // pousse la cible
        Pull,               // attire la cible
        Teleport            // téléporte la pièce
    }
}