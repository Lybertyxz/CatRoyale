using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CatRoyale.UI.Collection
{
    public class PieceCardUI : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Image _icon;
        [SerializeField] private Image _rarityBorder;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _roleText;
        [SerializeField] private TextMeshProUGUI _rarityText;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _attackText;
        [SerializeField] private TextMeshProUGUI _armorText;
        [SerializeField] private TextMeshProUGUI _slotCostText;

        [Header("Rarity Colors")]
        [SerializeField] private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color _rareColor = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private Color _epicColor = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.7f, 0f);

        public void Setup(PieceCardData data)
        {
            if (_nameText) _nameText.text = data.Name;
            if (_roleText) _roleText.text = data.Role;
            if (_rarityText) _rarityText.text = data.Rarity;
            if (_hpText) _hpText.text = $"HP {data.MaxHP}";
            if (_attackText) _attackText.text = $"ATK {data.Attack}";
            if (_armorText) _armorText.text = $"ARM {data.Armor}";
            if (_slotCostText) _slotCostText.text = $"Slots {data.SlotCost}";
            if (_icon && data.Icon) _icon.sprite = data.Icon;

            if (_rarityBorder)
                _rarityBorder.color = GetRarityColor(data.Rarity);
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
        public Sprite Icon;
    }
}