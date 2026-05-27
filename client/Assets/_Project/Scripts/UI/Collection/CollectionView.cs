using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;
using CatRoyale.Data;

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

        private void LoadPieces()
        {
            var repo = ServiceLocator.Get<PieceRepository>();
            _allPieces = repo.GetAll().ConvertAll(m => new PieceCardData
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
    }
}