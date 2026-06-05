using System.Collections.Generic;
using UnityEngine;
using CatRoyale.Core;
using CatRoyale.Data;
using CatRoyale.Network;

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
        private int _playerIndex;
        private PieceRepository _repo;

        public System.Action<int, int, int, int> OnActionRequested;
        public bool IsInitialized { get; private set; }

        // ─── Init ─────────────────────────────────────────────

        public void Initialize(string localPlayerID, int playerIndex = 0)
        {
            _localPlayerID = localPlayerID;
            _playerIndex = playerIndex;
            _repo = ServiceLocator.Get<PieceRepository>();
            IsInitialized = true;
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

                    cell.Setup(x, y, OnCellClicked);
                    _cells[y, x] = cell;
                }
            }
        }

        // ─── Input ────────────────────────────────────────────

        private void OnCellClicked(CellView cell)
        {
            Debug.Log($"[BoardView] Cell clicked: {cell.X},{cell.Y} | piece at: {GetPieceAt(cell.X, cell.Y)?.TemplateID}");
            if (_selectedCell == null)
            {
                var piece = GetPieceAt(cell.X, cell.Y);
                if (piece != null && piece.OwnerID == _localPlayerID)
                {
                    _selectedCell = cell;
                    cell.SetHighlight(CellHighlight.Selected);
                    HighlightValidCells(cell.X, cell.Y, piece.TemplateID);
                }
            }
            else
            {
                int fromX = _selectedCell.X, fromY = _selectedCell.Y;
                ClearHighlights();
                _selectedCell = null;

                if (cell.X != fromX || cell.Y != fromY)
                    OnActionRequested?.Invoke(fromX, fromY, cell.X, cell.Y);
            }
        }

        private void HighlightValidCells(int x, int y, string templateID)
        {
            var model = _repo?.Get(templateID);
            int range = model?.MoveRange ?? 1;
            string movementType = model?.MovementType ?? "omni";
            Debug.Log($"[BoardView] model: {model?.ID} | movementType: {movementType} | customCount: {model?.MovementCustom?.Count}");
            for (int ny = 0; ny < _boardSize; ny++)
            {
                for (int nx = 0; nx < _boardSize; nx++)
                {
                    if (nx == x && ny == y) continue;

                    if (!IsValidMoveTarget(x, y, nx, ny, range, movementType, model?.MovementCustom)) continue;

                    var piece = GetPieceAt(nx, ny);
                    Debug.Log($"[BoardView] Highlighting {nx},{ny} for {templateID} — movementType: {movementType}");
                    if (piece == null)
                        _cells[ny, nx].SetHighlight(CellHighlight.Move);
                    else if (piece.OwnerID != _localPlayerID)
                        _cells[ny, nx].SetHighlight(CellHighlight.Attack);
                }
            }
        }

        private bool IsValidMoveTarget(int fromX, int fromY, int toX, int toY, int range, string movementType, List<CustomPosition> custom)
        {
            int dx = toX - fromX;
            int dy = toY - fromY;
            int adx = Mathf.Abs(dx), ady = Mathf.Abs(dy);

            return movementType switch
            {
                "linear" => (adx == 0 || ady == 0) && Mathf.Max(adx, ady) <= range,
                "diagonal" => adx == ady && adx <= range,
                "omni" => Mathf.Max(adx, ady) <= range,
                "custom" => custom != null && custom.Exists(c => c.X == dx && c.Y == dy),
                _ => Mathf.Max(adx, ady) <= range
            };
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
            if (pieces == null) return;

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

            foreach (var data in pieces)
            {
                if (!data.IsAlive) continue;

                string key = data.TemplateID + data.OwnerID;
                var icon = _repo?.Get(data.TemplateID)?.Icon;
                var pos = GetCellPosition(data.X, data.Y);

                if (_pieces.TryGetValue(key, out var existing))
                {
                    existing.UpdateHP(data.CurrentHP);
                    existing.UpdatePosition(data.X, data.Y);
                    existing.MoveTo(pos);
                }
                else
                {
                    var pieceObj = Instantiate(_piecePrefab, _boardContainer);
                    var piece = pieceObj.GetComponent<PieceView>();
                    piece.Setup(data, data.OwnerID == _localPlayerID, icon);
                    pieceObj.GetComponent<RectTransform>().anchoredPosition = pos;
                    _pieces[key] = piece;
                }
            }
        }

        // ─── Helpers ──────────────────────────────────────────

        private Vector2 GetCellPosition(int x, int y)
        {
            int displayY = _playerIndex == 1 ? (_boardSize - 1 - y) : y;
            return new Vector2(
                x * _cellSize - (_boardSize * _cellSize / 2f) + _cellSize / 2f,
                displayY * _cellSize - (_boardSize * _cellSize / 2f) + _cellSize / 2f
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