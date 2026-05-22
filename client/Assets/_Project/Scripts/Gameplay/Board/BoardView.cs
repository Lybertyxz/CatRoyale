using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatRoyale.Gameplay
{
    public class BoardView : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private Transform _boardContainer;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _piecePrefab;

        [Header("Settings")]
        [SerializeField] private int _boardSize = 8;
        [SerializeField] private float _cellSize = 100f;

        private CellView[,] _cells;
        private Dictionary<string, PieceView> _pieces = new();
        private CellView _selectedCell;
        private string _localPlayerID;

        public System.Action<int, int> OnCellSelected;

        public void Initialize(string localPlayerID)
        {
            _localPlayerID = localPlayerID;
            CreateBoard();
        }

        private void CreateBoard()
        {
            _cells = new CellView[_boardSize, _boardSize];

            for (int y = 0; y < _boardSize; y++)
            {
                for (int x = 0; x < _boardSize; x++)
                {
                    var cellObj = Instantiate(_cellPrefab, _boardContainer);
                    var cell = cellObj.GetComponent<CellView>();

                    var rect = cellObj.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(_cellSize, _cellSize);
                    rect.anchoredPosition = new Vector2(
                        x * _cellSize - (_boardSize * _cellSize / 2f) + _cellSize / 2f,
                        y * _cellSize - (_boardSize * _cellSize / 2f) + _cellSize / 2f
                    );

                    int capturedX = x, capturedY = y;
                    cell.Setup(x, y, (c) => OnCellClicked(c));
                    _cells[y, x] = cell;
                }
            }
        }

        private void OnCellClicked(CellView cell)
        {
            if (_selectedCell == null)
            {
                // Sélectionne la pièce
                var piece = GetPieceAt(cell.X, cell.Y);
                if (piece != null && piece.OwnerID == _localPlayerID)
                {
                    _selectedCell = cell;
                    cell.SetHighlight(CellHighlight.Selected);
                    HighlightAdjacentCells(cell.X, cell.Y);
                }
            }
            else
            {
                // Action sur la case cible
                OnCellSelected?.Invoke(_selectedCell.X, _selectedCell.Y);
                ClearHighlights();
                _selectedCell = null;
            }
        }

        private void HighlightAdjacentCells(int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || nx >= _boardSize || ny < 0 || ny >= _boardSize) continue;

                    var piece = GetPieceAt(nx, ny);
                    if (piece == null)
                        _cells[ny, nx].SetHighlight(CellHighlight.Move);
                    else if (piece.OwnerID != _localPlayerID)
                        _cells[ny, nx].SetHighlight(CellHighlight.Attack);
                }
            }
        }

        public void ClearHighlights()
        {
            for (int y = 0; y < _boardSize; y++)
                for (int x = 0; x < _boardSize; x++)
                    _cells[y, x].SetHighlight(CellHighlight.None);
        }

        public void UpdateBoard(List<PieceStateData> pieces)
        {
            // Retire les pièces mortes
            var toRemove = new List<string>();
            foreach (var kv in _pieces)
            {
                bool found = false;
                foreach (var p in pieces)
                    if (p.TemplateID + p.OwnerID == kv.Key && p.IsAlive) { found = true; break; }
                if (!found) toRemove.Add(kv.Key);
            }
            foreach (var key in toRemove)
            {
                _pieces[key].PlayDeathAnimation();
                _pieces.Remove(key);
            }

            // Met à jour ou crée les pièces
            foreach (var data in pieces)
            {
                if (!data.IsAlive) continue;
                string key = data.TemplateID + data.OwnerID;

                if (_pieces.TryGetValue(key, out var existing))
                {
                    existing.UpdateHP(data.CurrentHP);
                    var targetPos = GetCellPosition(data.X, data.Y);
                    existing.MoveTo(targetPos);
                }
                else
                {
                    var pieceObj = Instantiate(_piecePrefab, _boardContainer);
                    var piece = pieceObj.GetComponent<PieceView>();
                    piece.Setup(data, data.OwnerID == _localPlayerID);
                    pieceObj.GetComponent<RectTransform>().anchoredPosition = GetCellPosition(data.X, data.Y);
                    _pieces[key] = piece;
                }
            }
        }

        private Vector2 GetCellPosition(int x, int y)
        {
            return new Vector2(
                x * _cellSize - (_boardSize * _cellSize / 2f) + _cellSize / 2f,
                y * _cellSize - (_boardSize * _cellSize / 2f) + _cellSize / 2f
            );
        }

        private PieceView GetPieceAt(int x, int y)
        {
            foreach (var piece in _pieces.Values)
                if (piece.X == x && piece.Y == y) return piece;
            return null;
        }
    }
}