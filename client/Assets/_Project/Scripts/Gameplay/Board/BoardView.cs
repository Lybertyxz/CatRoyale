using System.Collections.Generic;
using UnityEngine;
using CatRoyale.Core;
using CatRoyale.Data;

namespace CatRoyale.Gameplay
{
    public class BoardView : MonoBehaviour
    {
        [Header("Prefabs")]
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
        private PieceRepository _repo;

        // Callback vers GameSceneController : (pieceX, pieceY, targetX, targetY)
        public System.Action<int, int, int, int> OnActionRequested;

        // ─── Init ─────────────────────────────────────────────

        public void Initialize(string localPlayerID)
        {
            _localPlayerID = localPlayerID;
            _repo = ServiceLocator.Get<PieceRepository>();
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
                    rect.anchoredPosition = GetCellPosition(x, y);

                    int cx = x, cy = y;
                    cell.Setup(cx, cy, OnCellClicked);
                    _cells[y, x] = cell;
                }
            }
        }

        // ─── Input ────────────────────────────────────────────

        private void OnCellClicked(CellView cell)
        {
            if (_selectedCell == null)
            {
                // Sélectionne une pièce locale
                var piece = GetPieceAt(cell.X, cell.Y);
                if (piece != null && piece.OwnerID == _localPlayerID)
                {
                    _selectedCell = cell;
                    cell.SetHighlight(CellHighlight.Selected);
                    HighlightValidCells(cell.X, cell.Y);
                }
            }
            else
            {
                // Envoie l'action : pièce sélectionnée → case cible
                int fromX = _selectedCell.X, fromY = _selectedCell.Y;
                ClearHighlights();
                _selectedCell = null;

                if (cell.X != fromX || cell.Y != fromY)
                    OnActionRequested?.Invoke(fromX, fromY, cell.X, cell.Y);
            }
        }

        private void HighlightValidCells(int x, int y)
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

        // ─── Board Update ─────────────────────────────────────

        public void UpdateBoard(List<PieceStateData> pieces)
        {
            Debug.Log($"[BoardView] UpdateBoard called with {pieces?.Count} pieces");
            if (pieces == null) return;
            // Retire les pièces mortes ou absentes
            var toRemove = new List<string>();
            foreach (var kv in _pieces)
            {
                bool alive = false;
                foreach (var p in pieces)
                    if (p.TemplateID + p.OwnerID == kv.Key && p.IsAlive) { alive = true; break; }
                if (!alive) toRemove.Add(kv.Key);
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
                var icon = _repo?.Get(data.TemplateID)?.Icon;

                if (_pieces.TryGetValue(key, out var existing))
                {
                    existing.UpdateHP(data.CurrentHP);
                    existing.MoveTo(GetCellPosition(data.X, data.Y));
                }
                else
                {
                    var pieceObj = Instantiate(_piecePrefab, _boardContainer);
                    var piece = pieceObj.GetComponent<PieceView>();
                    piece.Setup(data, data.OwnerID == _localPlayerID, icon);
                    pieceObj.GetComponent<RectTransform>().anchoredPosition = GetCellPosition(data.X, data.Y);
                    _pieces[key] = piece;
                }
            }
        }

        // ─── Helpers ──────────────────────────────────────────

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