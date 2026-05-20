using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatRoyale.Gameplay
{
    [Serializable]
    public class AttackPattern
    {
        [Header("Range")]
        public int MinRange;        // portée minimum (1 = case adjacente)
        public int MaxRange;        // portée maximum (-1 = illimité)
        public bool CanSkipCells;   // peut attaquer par dessus d'autres pièces

        [Header("Pattern")]
        public AttackPatternType Type;
        public List<Vector2Int> CustomCells; // cases relatives custom

        [Header("Area")]
        public bool IsAreaOfEffect;
        public int AoeRadius;       // rayon de l'AoE autour de la cible
    }

    public enum AttackPatternType
    {
        Melee,          // case adjacente uniquement
        Linear,         // ligne droite
        Diagonal,       // diagonale
        Omnidirectional,// toutes directions
        Custom          // pattern custom défini par CustomCells
    }
}