using UnityEngine;
using System.Collections.Generic;

namespace CatRoyale.Data
{
    [System.Serializable]
    public class PieceModel
    {
        // Identity
        public string ID;
        public string Name;
        public string Role;
        public string Rarity;

        // Stats
        public int SlotCost;
        public int MaxHP;
        public int Attack;
        public int Armor;
        public int AttackRange;
        public int MoveRange;
        public bool CanJump;
        public string MovementType;

        // Visual (client only)
        public Sprite Icon;

        // Player state
        public bool IsOwned;
        public int Level;
    }
}