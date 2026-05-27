using UnityEngine;
using System.Collections.Generic;

namespace CatRoyale.Gameplay
{
    [CreateAssetMenu(fileName = "PieceRegistry", menuName = "CatRoyale/Piece Registry")]
    public class PieceRegistry : ScriptableObject
    {
        public List<PieceData> Pieces;

        private Dictionary<string, PieceData> _lookup;

        public Sprite GetIcon(string pieceID)
        {
            if (_lookup == null)
            {
                _lookup = new();
                foreach (var p in Pieces)
                    _lookup[p.PieceID] = p;
            }
            return _lookup.TryGetValue(pieceID, out var def) ? def.Icon : null;
        }
    }
}