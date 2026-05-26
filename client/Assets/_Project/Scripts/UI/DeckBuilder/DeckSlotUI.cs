using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using CatRoyale.UI.Collection;

namespace CatRoyale.UI.DeckBuilder
{
    public class DeckSlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        [Header("Visual")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _characterIcon;
        [SerializeField] private GameObject _emptyState;

        private PieceCardData _piece;
        private Action<DeckSlotUI> _onClickCallback;
        public Action<DeckSlotUI, PieceCardData> OnPieceDropped;

        public PieceCardData Piece => _piece;
        public bool IsEmpty => _piece == null;

        public void Setup(PieceCardData piece, Action<DeckSlotUI> onClick)
        {
            _piece = piece;
            _onClickCallback = onClick;
            Refresh();
        }

        public void Clear()
        {
            _piece = null;
            Refresh();
        }

        private void Refresh()
        {
            bool isEmpty = _piece == null;
            if (_emptyState) _emptyState.SetActive(isEmpty);
            if (_characterIcon) _characterIcon.gameObject.SetActive(!isEmpty);
            if (!isEmpty && _characterIcon && _piece.Icon)
                _characterIcon.sprite = _piece.Icon;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClickCallback?.Invoke(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var card = PieceCardUI.Dragging;
            if (card == null) return;
            OnPieceDropped?.Invoke(this, card.Data);
        }
    }
}