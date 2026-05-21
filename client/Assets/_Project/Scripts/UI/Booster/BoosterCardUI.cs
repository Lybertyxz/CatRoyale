using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CatRoyale.UI.Booster
{
    public class BoosterCardUI : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _rarityBorder;
        [SerializeField] private Image _icon;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _priceCoinsText;
        [SerializeField] private TextMeshProUGUI _priceGemsText;

        [Header("Button")]
        [SerializeField] private Button _openButton;

        private System.Action _onOpenCallback;

        [Header("Rarity Colors")]
        [SerializeField] private Color _commonColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color _rareColor = new Color(0.2f, 0.5f, 1f);
        [SerializeField] private Color _epicColor = new Color(0.6f, 0.2f, 0.8f);
        [SerializeField] private Color _legendaryColor = new Color(1f, 0.7f, 0f);

        private void Awake()
        {
            _openButton?.onClick.AddListener(() => _onOpenCallback?.Invoke());
        }

        public void Setup(BoosterData data, System.Action onOpen)
        {
            _onOpenCallback = onOpen;

            if (_nameText) _nameText.text = data.Name;
            if (_rarityText) _rarityText.text = data.Description;
            if (_priceCoinsText) _priceCoinsText.text = data.PriceCoins > 0 ? $"{data.PriceCoins} 🪙" : "Free";
            if (_priceGemsText) _priceGemsText.text = data.PriceGems > 0 ? $"{data.PriceGems} 💎" : "";
        }
    }

    public class BoosterData
    {
        public string ID;
        public string Name;
        public string Description;
        public int PriceCoins;
        public int PriceGems;
        public int PiecesCount;
    }
}