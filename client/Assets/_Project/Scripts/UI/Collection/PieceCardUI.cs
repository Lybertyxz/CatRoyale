using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CatRoyale.UI.Collection
{
    public class PieceCardUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("Background")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _rarityBorder;

        [Header("Content")]
        [SerializeField] private Image _characterIcon;
        [SerializeField] private Image _roleIcon;
        [SerializeField] private GameObject _unknownOverlay; // "?" si non possédé

        private PieceCardData _data;
        private System.Action<PieceCardData> _onClickCallback;

        [Header("Rarity Colors")]
        [SerializeField] private Color _commonColor    = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _rareColor      = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private Color _epicColor      = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.7f, 0f);

        public void Setup(PieceCardData data, System.Action<PieceCardData> onClick = null)
        {
            _data = data;
            _onClickCallback = onClick;

            var rarityColor = GetRarityColor(data.Rarity);

            if (_background) _background.color = new Color(
                rarityColor.r * 0.3f,
                rarityColor.g * 0.3f,
                rarityColor.b * 0.3f, 1f
            );
            if (_rarityBorder) _rarityBorder.color = rarityColor;
            if (_characterIcon && data.Icon) _characterIcon.sprite = data.Icon;
            if (_roleIcon && data.RoleIcon) _roleIcon.sprite = data.RoleIcon;
            if (_unknownOverlay) _unknownOverlay.SetActive(!data.IsOwned);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClickCallback?.Invoke(_data);
        }

        private Color GetRarityColor(string rarity) => rarity switch
        {
            "common"    => _commonColor,
            "rare"      => _rareColor,
            "epic"      => _epicColor,
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