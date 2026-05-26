using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace CatRoyale.UI.Collection
{
    public class PieceCardUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Background")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _rarityBorder;

        [Header("Content")]
        [SerializeField] private Image _characterIcon;
        [SerializeField] private Image _roleIcon;
        [SerializeField] private GameObject _unknownOverlay;

        private PieceCardData _data;
        private Action<PieceCardData> _onClickCallback;

        // Drag
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Transform _originalParent;
        private Vector2 _originalPosition;
        private static PieceCardUI _dragging;

        public PieceCardData Data => _data;
        public static PieceCardUI Dragging => _dragging;

        [Header("Rarity Colors")]
        [SerializeField] private Color _commonColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _rareColor = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private Color _epicColor = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.7f, 0f);

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void Setup(PieceCardData data, Action<PieceCardData> onClick = null)
        {
            _data = data;
            _onClickCallback = onClick;

            var rarityColor = GetRarityColor(data.Rarity);
            if (_background) _background.color = new Color(rarityColor.r * 0.3f, rarityColor.g * 0.3f, rarityColor.b * 0.3f, 1f);
            if (_rarityBorder) _rarityBorder.color = rarityColor;
            if (_characterIcon && data.Icon) _characterIcon.sprite = data.Icon;
            if (_roleIcon && data.RoleIcon) _roleIcon.sprite = data.RoleIcon;
            if (_unknownOverlay) _unknownOverlay.SetActive(!data.IsOwned);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_dragging != null) return;
            _onClickCallback?.Invoke(_data);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = this;
            _originalParent = transform.parent;
            _originalPosition = _rectTransform.anchoredPosition;

            transform.SetParent(_canvas.transform, true);
            transform.SetAsLastSibling();
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.8f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragging = null;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            // Si pas droppé sur un slot valide — retourne à la position originale
            transform.SetParent(_originalParent, true);
            _rectTransform.anchoredPosition = _originalPosition;
        }

        private Color GetRarityColor(string rarity) => rarity switch
        {
            "common" => _commonColor,
            "rare" => _rareColor,
            "epic" => _epicColor,
            "legendary" => _legendaryColor,
            _ => _commonColor
        };
    }

    public class PieceCardData
    {
        public string ID;
        public string Name;
        public string Role;
        public string Rarity;
        public int MaxHP;
        public int Attack;
        public int Armor;
        public int SlotCost;
        public int AttackRange;
        public bool IsOwned;
        public Sprite Icon;
        public Sprite RoleIcon;
    }
}