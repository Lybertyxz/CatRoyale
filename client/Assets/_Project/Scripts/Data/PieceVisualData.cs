using UnityEngine;

namespace CatRoyale.Data
{
    [CreateAssetMenu(fileName = "PieceVisualData", menuName = "CatRoyale/Piece Visual Data")]
    public class PieceVisualData : ScriptableObject
    {
        public string PieceID;
        public Sprite Icon;
        // Plus tard : AnimatorController, AudioClip, VFX prefab...
    }
}