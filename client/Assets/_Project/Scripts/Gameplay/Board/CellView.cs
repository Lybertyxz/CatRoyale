using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace CatRoyale.Gameplay
{
    public class CellView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Visual")]
        [SerializeField] private Image _background;
        [SerializeField] private Color _lightColor = new Color(0.9f, 0.85f, 0.7f);
        [SerializeField] private Color _darkColor = new Color(0.4f, 0.3f, 0.2f);
        [SerializeField] private Color _highlightColor = new Color(0.3f, 0.8f, 0.3f, 0.6f);
        [SerializeField] private Color _selectedColor = new Color(0.8f, 0.8f, 0.2f, 0.6f);
        [SerializeField] private Color _enemyColor = new Color(0.8f, 0.2f, 0.2f, 0.6f);

        public int X { get; private set; }
        public int Y { get; private set; }

        private Color _baseColor;
        private Action<CellView> _onClickCallback;

        public void Setup(int x, int y, Action<CellView> onClick)
        {
            X = x;
            Y = y;
            _onClickCallback = onClick;
            _baseColor = (x + y) % 2 == 0 ? _lightColor : _darkColor;
            SetColor(_baseColor);
        }

        public void SetHighlight(CellHighlight highlight)
        {
            switch (highlight)
            {
                case CellHighlight.None: SetColor(_baseColor); break;
                case CellHighlight.Selected: SetColor(_selectedColor); break;
                case CellHighlight.Move: SetColor(_highlightColor); break;
                case CellHighlight.Attack: SetColor(_enemyColor); break;
            }
        }

        private void SetColor(Color color)
        {
            if (_background) _background.color = color;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClickCallback?.Invoke(this);
        }
    }

    public enum CellHighlight { None, Selected, Move, Attack }
}