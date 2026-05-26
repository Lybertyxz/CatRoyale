using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Network;
using System;

namespace CatRoyale.UI.DeckBuilder
{
    public class DeckItemUI : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _slotsText;
        [SerializeField] private Image _activeIndicator;

        [Header("Buttons")]
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _deleteButton;

        public void Setup(DeckResponse deck, Action onEdit, Action onDelete)
        {
            if (_nameText) _nameText.text = deck.Name;
            if (_slotsText) _slotsText.text = $"{deck.TotalSlots}/20 slots";
            if (_activeIndicator) _activeIndicator.gameObject.SetActive(deck.IsActive);

            _editButton?.onClick.AddListener(() => onEdit?.Invoke());
            _deleteButton?.onClick.AddListener(() => onDelete?.Invoke());
        }
    }
}