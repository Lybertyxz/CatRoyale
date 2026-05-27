using UnityEngine;
using System.Collections.Generic;

namespace CatRoyale.Data
{
    [CreateAssetMenu(fileName = "PieceVisualRegistry", menuName = "CatRoyale/Piece Visual Registry")]
    public class PieceVisualRegistry : ScriptableObject
    {
        public List<PieceVisualData> Visuals;

        private Dictionary<string, PieceVisualData> _lookup;

        public PieceVisualData Get(string pieceID)
        {
            if (_lookup == null)
            {
                _lookup = new();
                foreach (var v in Visuals)
                    if (v != null) _lookup[v.PieceID] = v;
            }
            return _lookup.TryGetValue(pieceID, out var result) ? result : null;
        }

        public Sprite GetIcon(string pieceID) => Get(pieceID)?.Icon;
    }
}