using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;
using CatRoyale.Network;
using CatRoyale.UI.Collection;
using CatRoyale.Data;

namespace CatRoyale.UI.DeckBuilder
{
    public class DeckBuilderView : MonoBehaviour
    {
        [Header("Deck List Panel")]
        [SerializeField] private Transform _deckListContainer;
        [SerializeField] private GameObject _deckItemPrefab;
        [SerializeField] private Button _createDeckButton;

        [Header("Deck Editor Panel")]
        [SerializeField] private TextMeshProUGUI _deckNameText;
        [SerializeField] private TextMeshProUGUI _slotsUsedText;
        [SerializeField] private Transform _boardContainer;
        [SerializeField] private GameObject _boardSlotPrefab;
        [SerializeField] private Transform _collectionContainer;
        [SerializeField] private GameObject _pieceCardPrefab;
        [SerializeField] private Button _saveButton;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        private const int MaxSlots = 20;
        private int _usedSlots = 0;
        private string _currentDeckID;
        private DeckSlotUI[,] _boardSlots = new DeckSlotUI[2, 8];
        private List<PieceCardData> _collection = new();
        private DeckSlotUI _selectedSlot;
        private List<DeckResponse> _decks = new();

        private void Awake()
        {
            _createDeckButton?.onClick.AddListener(OnCreateDeckClicked);
            _saveButton?.onClick.AddListener(OnSaveClicked);
            _backButton?.onClick.AddListener(OnBackClicked);
        }

        private void OnEnable()
        {
            InitBoard();
            LoadCollection();
            LoadDecks();
        }

        // ─── Deck List ────────────────────────────────────────
        private async void LoadDecks()
        {
            var api = ServiceLocator.Get<ApiService>();
            var result = await api.GetDecks();

            Debug.Log($"[DeckBuilder] LoadDecks success: {result.Success} count: {result.Data?.Count}");
            foreach (Transform child in _deckListContainer)
                Destroy(child.gameObject);

            if (!result.Success || result.Data == null)
            {
                Debug.LogWarning($"[DeckBuilder] {result.Error}");
                return;
            }

            _decks = result.Data;

            foreach (var deck in _decks)
            {
                var item = Instantiate(_deckItemPrefab, _deckListContainer);
                var deckCopy = deck;
                item.GetComponent<DeckItemUI>()?.Setup(deck,
                    () => OpenEditor(deckCopy),
                    () => OnDeleteDeck(deckCopy)
                );
            }
        }

        private async void OnCreateDeckClicked()
        {
            var api = ServiceLocator.Get<ApiService>();
            var result = await api.CreateDeck("Nouveau Deck");

            if (!result.Success)
            {
                Debug.LogError($"[DeckBuilder] Failed to create deck: {result.Error}");
                return;
            }

            LoadDecks();
        }

        private async void OnDeleteDeck(DeckResponse deck)
        {
            var api = ServiceLocator.Get<ApiService>();
            var result = await api.DeleteDeck(deck.ID);

            if (!result.Success)
            {
                Debug.LogError($"[DeckBuilder] Delete failed: {result.Error}");
                return;
            }

            Debug.Log($"[DeckBuilder] Deleted deck: {deck.ID}");
            LoadDecks();
        }

        // ─── Deck Editor ──────────────────────────────────────
        private void OpenEditor(DeckResponse deck)
        {
            _currentDeckID = deck.ID;
            if (_deckNameText) _deckNameText.text = deck.Name;
            _usedSlots = 0;
            InitBoard();
            LoadDeckEntries(deck.ID);
        }

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
                    slot.OnPieceDropped = (s, piece) => OnPieceDroppedOnSlot(s, piece);
                    _boardSlots[y, x] = slot;
                }
            }
        }

        private void OnPieceDroppedOnSlot(DeckSlotUI slot, PieceCardData piece)
        {
            if (!slot.IsEmpty)
                _usedSlots -= slot.Piece.SlotCost;

            if (_usedSlots + piece.SlotCost > MaxSlots)
            {
                Debug.LogWarning("[DeckBuilder] Not enough slots.");
                return;
            }

            slot.Setup(piece, (s) => OnBoardSlotClicked(s, 0, 0));
            _usedSlots += piece.SlotCost;
            UpdateSlotsUI();
        }

        private void LoadCollection()
        {
            foreach (Transform child in _collectionContainer)
                Destroy(child.gameObject);

            var repo = ServiceLocator.Get<PieceRepository>();
            _collection = repo.GetAll().ConvertAll(m => new PieceCardData
            {
                ID = m.ID,
                Name = m.Name,
                Role = m.Role,
                Rarity = m.Rarity,
                SlotCost = m.SlotCost,
                MaxHP = m.MaxHP,
                Attack = m.Attack,
                Armor = m.Armor,
                IsOwned = m.IsOwned,
                Icon = m.Icon
            });

            foreach (var piece in _collection)
            {
                var card = Instantiate(_pieceCardPrefab, _collectionContainer);
                card.GetComponent<PieceCardUI>()?.Setup(piece, OnCollectionCardClicked);
            }
        }

        private void OnCollectionCardClicked(PieceCardData piece)
        {
            if (_selectedSlot == null) return;

            if (!_selectedSlot.IsEmpty)
                _usedSlots -= _selectedSlot.Piece.SlotCost;

            if (_usedSlots + piece.SlotCost > MaxSlots)
            {
                Debug.LogWarning("[DeckBuilder] Not enough slots.");
                return;
            }

            _selectedSlot.Setup(piece, (s) => OnBoardSlotClicked(s, 0, 0));
            _usedSlots += piece.SlotCost;
            UpdateSlotsUI();
            _selectedSlot = null;
        }

        private void OnBoardSlotClicked(DeckSlotUI slot, int x, int y)
        {
            if (!slot.IsEmpty)
            {
                _usedSlots -= slot.Piece.SlotCost;
                slot.Clear();
                UpdateSlotsUI();
                return;
            }
            _selectedSlot = slot;
        }

        private async void LoadDeckEntries(string deckID)
        {
            var api = ServiceLocator.Get<ApiService>();
            var result = await api.GetDeckDetail(deckID);

            if (!result.Success || result.Data?.Entries == null) return;

            foreach (var entry in result.Data.Entries)
            {
                var piece = _collection.Find(p => p.ID == entry.TemplateID);
                if (piece == null) continue;

                var slot = _boardSlots[entry.StartY, entry.StartX];
                if (slot == null) continue;

                slot.Setup(piece, (s) => OnBoardSlotClicked(s, entry.StartX, entry.StartY));
                _usedSlots += piece.SlotCost;
            }

            UpdateSlotsUI();
        }

        private void UpdateSlotsUI()
        {
            if (_slotsUsedText) _slotsUsedText.text = $"{_usedSlots}/{MaxSlots} slots";
        }

        private async void OnSaveClicked()
        {
            if (string.IsNullOrEmpty(_currentDeckID)) return;

            var api = ServiceLocator.Get<ApiService>();
            var entries = new List<DeckEntryRequest>();

            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 8; x++)
                {
                    var slot = _boardSlots[y, x];
                    if (!slot.IsEmpty)
                        entries.Add(new DeckEntryRequest
                        {
                            TemplateID = slot.Piece.ID,
                            StartX = x,
                            StartY = y
                        });
                }

            var result = await api.SaveDeck(_currentDeckID, entries);
            Debug.Log(result.Success
                ? $"[DeckBuilder] Saved {entries.Count} pieces."
                : $"[DeckBuilder] Save failed: {result.Error}");
        }

        private void OnBackClicked()
        {
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.Menu);
        }
    }
}