using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CatRoyale.UI.Collection;

namespace CatRoyale.UI.DeckBuilder
{
    public class DeckSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Visual")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _characterIcon;
        [SerializeField] private Image _roleIcon;
        [SerializeField] private Image _rarityBorder;
        [SerializeField] private GameObject _emptyState;   // affiché si slot vide

        private PieceCardData _piece;
        private System.Action<DeckSlotUI> _onClickCallback;

        public PieceCardData Piece => _piece;
        public bool IsEmpty => _piece == null;

        public void Setup(PieceCardData piece, System.Action<DeckSlotUI> onClick)
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
            if (_roleIcon) _roleIcon.gameObject.SetActive(!isEmpty);
            if (_rarityBorder) _rarityBorder.gameObject.SetActive(!isEmpty);

            if (!isEmpty)
            {
                if (_characterIcon && _piece.Icon) _characterIcon.sprite = _piece.Icon;
                if (_roleIcon && _piece.RoleIcon) _roleIcon.sprite = _piece.RoleIcon;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClickCallback?.Invoke(this);
        }
    }
}