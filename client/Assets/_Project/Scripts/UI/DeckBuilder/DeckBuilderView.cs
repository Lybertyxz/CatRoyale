using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;
using CatRoyale.UI.Collection;
using CatRoyale.Network;

namespace CatRoyale.UI.DeckBuilder
{
    public class DeckBuilderView : MonoBehaviour
    {
        [Header("Deck Info")]
        [SerializeField] private TextMeshProUGUI _deckNameText;
        [SerializeField] private TextMeshProUGUI _slotsUsedText;

        [Header("Board Preview")]
        [SerializeField] private Transform _boardContainer;
        [SerializeField] private GameObject _boardSlotPrefab;

        [Header("Piece Collection")]
        [SerializeField] private Transform _collectionContainer;
        [SerializeField] private GameObject _pieceCardPrefab;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _saveButton;

        private const int MaxSlots = 20;
        private int _usedSlots = 0;

        // Board = grille 8x2 (zone joueur)
        private DeckSlotUI[,] _boardSlots = new DeckSlotUI[2, 8];
        private List<PieceCardData> _collection = new();
        private DeckSlotUI _selectedSlot;

        private void Awake()
        {
            _backButton?.onClick.AddListener(OnBackClicked);
            _saveButton?.onClick.AddListener(OnSaveClicked);
        }

        private void OnEnable()
        {
            InitBoard();
            LoadCollection();
        }

        // Crée la grille 8x2 du placement
        private void InitBoard()
        {
            foreach (Transform child in _boardContainer)
                Destroy(child.gameObject);

            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    var slotObj = Instantiate(_boardSlotPrefab, _boardContainer);
                    var slot = slotObj.GetComponent<DeckSlotUI>();
                    int capturedX = x, capturedY = y;
                    slot.Setup(null, (s) => OnBoardSlotClicked(s, capturedX, capturedY));
                    _boardSlots[y, x] = slot;
                }
            }
        }

        private async void LoadCollection()
        {
            foreach (Transform child in _collectionContainer)
                Destroy(child.gameObject);

            var api = ServiceLocator.Get<ApiService>();
            var pieces = await api.GetPieces();

            if (pieces == null || pieces.Count == 0)
            {
                Debug.LogWarning("[DeckBuilderView] No pieces received, using placeholders.");
                _collection = GetPlaceholderPieces();
            }
            else
            {
                _collection = pieces.ConvertAll(p => new PieceCardData
                {
                    ID = p.ID,
                    Name = p.Name,
                    Role = p.Role,
                    Rarity = p.Rarity,
                    SlotCost = p.SlotCost,
                    MaxHP = p.MaxHP,
                    Attack = p.Attack,
                    Armor = p.Armor,
                    IsOwned = true
                });
            }

            foreach (var piece in _collection)
            {
                var card = Instantiate(_pieceCardPrefab, _collectionContainer);
                card.GetComponent<PieceCardUI>()?.Setup(piece, OnCollectionCardClicked);
            }
        }

        private void OnCollectionCardClicked(PieceCardData piece)
        {
            if (_selectedSlot == null) return;

            // Vérifie les slots disponibles
            int cost = piece.SlotCost;
            if (_usedSlots + cost > MaxSlots)
            {
                Debug.LogWarning("[DeckBuilder] Not enough slots.");
                return;
            }

            // Si le slot avait déjà une pièce, libère les slots
            if (!_selectedSlot.IsEmpty)
                _usedSlots -= _selectedSlot.Piece.SlotCost;

            _selectedSlot.Setup(piece, (s) => OnBoardSlotClicked(s, 0, 0));
            _usedSlots += cost;
            UpdateSlotsUI();
            _selectedSlot = null;
        }

        private void OnBoardSlotClicked(DeckSlotUI slot, int x, int y)
        {
            // Si slot occupé → retire la pièce
            if (!slot.IsEmpty)
            {
                _usedSlots -= slot.Piece.SlotCost;
                slot.Clear();
                UpdateSlotsUI();
                return;
            }

            // Sélectionne le slot pour y placer une pièce
            _selectedSlot = slot;
            Debug.Log($"[DeckBuilder] Slot selected: ({x},{y})");
        }

        private void UpdateSlotsUI()
        {
            if (_slotsUsedText)
                _slotsUsedText.text = $"{_usedSlots}/{MaxSlots} slots";
        }

        private void OnSaveClicked()
        {
            Debug.Log("[DeckBuilder] Saving deck...");
            // TODO: appel HTTP PUT /api/v1/decks/:id
        }

        private void OnBackClicked()
        {
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.Menu);
        }

        private List<PieceCardData> GetPlaceholderPieces()
        {
            return new List<PieceCardData>
            {
                new() { ID = "biscuit_001", Name = "Biscuit", Role = "pawn", Rarity = "common", MaxHP = 80, Attack = 15, Armor = 5, SlotCost = 1 },
                new() { ID = "granite_001", Name = "Granite", Role = "rook", Rarity = "rare", MaxHP = 160, Attack = 20, Armor = 20, SlotCost = 2 },
                new() { ID = "whisker_001", Name = "Whisker", Role = "knight", Rarity = "rare", MaxHP = 100, Attack = 30, Armor = 5, SlotCost = 2 },
                new() { ID = "luna_001", Name = "Luna", Role = "bishop", Rarity = "epic", MaxHP = 90, Attack = 35, Armor = 0, SlotCost = 3 },
                new() { ID = "tempete_001", Name = "Tempête", Role = "queen", Rarity = "epic", MaxHP = 120, Attack = 40, Armor = 10, SlotCost = 4 },
                new() { ID = "pharaon_001", Name = "Pharaon", Role = "king", Rarity = "legendary", MaxHP = 220, Attack = 20, Armor = 25, SlotCost = 5 },
            };
        }
    }
}