using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;
using CatRoyale.Network;
using Newtonsoft.Json;

namespace CatRoyale.UI.Collection
{
    public class CollectionView : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private GameObject _pieceCardPrefab;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Filter")]
        [SerializeField] private TMP_Dropdown _rarityFilter;
        [SerializeField] private TMP_Dropdown _roleFilter;

        private List<PieceCardData> _allPieces = new();

        private void Awake()
        {
            _backButton?.onClick.AddListener(OnBackClicked);
            _rarityFilter?.onValueChanged.AddListener(OnFilterChanged);
            _roleFilter?.onValueChanged.AddListener(OnRoleFilterChanged);
        }

        private void OnEnable()
        {
            LoadPieces();
        }

        private async void LoadPieces()
        {
            var api = ServiceLocator.Get<ApiService>();
            var pieces = await api.GetPieces();

            if (pieces == null || pieces.Count == 0)
            {
                Debug.LogWarning("[CollectionView] No pieces received, using placeholders.");
                _allPieces = GetPlaceholderPieces();
            }
            else
            {
                _allPieces = pieces.ConvertAll(p => new PieceCardData
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

            DisplayPieces(_allPieces);
        }

        private void DisplayPieces(List<PieceCardData> pieces)
        {
            foreach (Transform child in _gridContainer)
                Destroy(child.gameObject);
            foreach (var piece in pieces)
            {
                var card = Instantiate(_pieceCardPrefab, _gridContainer);
                card.GetComponent<PieceCardUI>()?.Setup(piece, OnCardClicked);
            }
        }

        private void OnCardClicked(PieceCardData data)
        {
            Debug.Log($"[CollectionView] Card clicked: {data.Name}");
            // TODO: ouvrir popup détail
        }

        private void OnRoleFilterChanged(int index)
        {
            string[] roles = { "all", "pawn", "rook", "knight", "bishop", "queen", "king" };
            string selectedRole = roles[index];
            string selectedRarity = new string[] { "all", "common", "rare", "epic", "legendary" }[_rarityFilter.value];

            var filtered = _allPieces.FindAll(p =>
                (selectedRarity == "all" || p.Rarity == selectedRarity) &&
                (selectedRole == "all" || p.Role.ToLower() == selectedRole)
            );

            DisplayPieces(filtered);
        }

        private void OnFilterChanged(int index)
        {
            string[] rarities = { "all", "common", "rare", "epic", "legendary" };
            string selectedRarity = rarities[index];
            string[] roles = { "all", "pawn", "rook", "knight", "bishop", "queen", "king" };
            string selectedRole = roles[_roleFilter.value];

            var filtered = _allPieces.FindAll(p =>
                (selectedRarity == "all" || p.Rarity == selectedRarity) &&
                (selectedRole == "all" || p.Role.ToLower() == selectedRole)
            );

            DisplayPieces(filtered);
        }

        private void OnBackClicked()
        {
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.Menu);
        }

        private List<PieceCardData> GetPlaceholderPieces()
        {
            return new List<PieceCardData>
            {
                new() { ID = "biscuit_001", Name = "Biscuit", Role = "Pawn", Rarity = "common", MaxHP = 80, Attack = 15, Armor = 5, SlotCost = 1 },
                new() { ID = "granite_001", Name = "Granite", Role = "Rook", Rarity = "rare", MaxHP = 160, Attack = 20, Armor = 20, SlotCost = 2 },
                new() { ID = "whisker_001", Name = "Whisker", Role = "Knight", Rarity = "rare", MaxHP = 100, Attack = 30, Armor = 5, SlotCost = 2 },
                new() { ID = "luna_001", Name = "Luna", Role = "Bishop", Rarity = "epic", MaxHP = 90, Attack = 35, Armor = 0, SlotCost = 3 },
                new() { ID = "tempete_001", Name = "Tempête", Role = "Queen", Rarity = "epic", MaxHP = 120, Attack = 40, Armor = 10, SlotCost = 4 },
                new() { ID = "pharaon_001", Name = "Pharaon", Role = "King", Rarity = "legendary", MaxHP = 220, Attack = 20, Armor = 25, SlotCost = 5 },
            };
        }
    }
}