using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatRoyale.Gameplay
{
    [CreateAssetMenu(fileName = "NewPiece", menuName = "CatRoyale/Piece Data")]
    public class PieceData : ScriptableObject
    {
        [Header("Identity")]
        public string PieceID;
        public string PieceName;
        public PieceRole Role;
        public Rarity Rarity;
        public Sprite Icon;

        [Header("Stats")]
        public int MaxHP;
        public int Attack;
        public int Armor;
        public int AttackRange;
        public int SlotCost;

        [Header("Movement")]
        public MovementType MovementType;
        public int MoveRange;
        public bool CanJump;

        [Header("Abilities")]
        public List<AbilityData> Abilities;
    }

[Serializable]
public class AbilityData
{
    [Header("Identity")]
    public string AbilityID;
    public string AbilityName;
    public AbilityType Type;
    public int Cooldown;
    [TextArea] public string Description;
    public Sprite Icon;

    [Header("Attack Pattern")]
    public AttackPattern AttackPattern;

    [Header("Effects")]
    public List<AbilityEffect> Effects;
}

    public enum PieceRole { Pawn, Rook, Knight, Bishop, Queen, King }
    public enum Rarity { Common, Rare, Epic, Legendary }
    public enum MovementType { Linear, Diagonal, Omnidirectional, Custom }
    public enum AbilityType { Passive, Active }
}