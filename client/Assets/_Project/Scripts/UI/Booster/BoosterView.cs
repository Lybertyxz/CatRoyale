using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatRoyale.Core;
using CatRoyale.UI.Collection;
using CatRoyale.Network;

namespace CatRoyale.UI.Booster
{
    public class BoosterView : MonoBehaviour
    {
        [Header("Booster List")]
        [SerializeField] private Transform _boosterContainer;
        [SerializeField] private GameObject _boosterCardPrefab;

        [Header("Opening Result")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private Transform _resultContainer;
        [SerializeField] private GameObject _pieceCardPrefab;
        [SerializeField] private Button _closeResultButton;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        private void Awake()
        {
            _backButton?.onClick.AddListener(OnBackClicked);
            _closeResultButton?.onClick.AddListener(OnCloseResult);
            if (_resultPanel) _resultPanel.SetActive(false);
        }

        private void OnEnable()
        {
            LoadBoosters();
        }

        private async void LoadBoosters()
        {
            foreach (Transform child in _boosterContainer)
                Destroy(child.gameObject);

            var api = ServiceLocator.Get<ApiService>();
            var boosters = await api.GetBoosters();

            if (boosters == null || boosters.Count == 0)
            {
                Debug.LogWarning("[BoosterView] No boosters received, using placeholders.");
                boosters = GetPlaceholderBoosters().ConvertAll(b => new BoosterResponse
                {
                    ID = b.ID,
                    Name = b.Name,
                    Description = b.Description,
                    PriceCoins = b.PriceCoins,
                    PriceGems = b.PriceGems,
                    PiecesCount = b.PiecesCount
                });
            }

            foreach (var booster in boosters)
            {
                var card = Instantiate(_boosterCardPrefab, _boosterContainer);
                var boosterCopy = booster;
                card.GetComponent<BoosterCardUI>()?.Setup(new BoosterData
                {
                    ID = boosterCopy.ID,
                    Name = boosterCopy.Name,
                    Description = boosterCopy.Description,
                    PriceCoins = boosterCopy.PriceCoins,
                    PriceGems = boosterCopy.PriceGems,
                    PiecesCount = boosterCopy.PiecesCount
                }, () => OnOpenBooster(new BoosterData
                {
                    ID = boosterCopy.ID,
                    Name = boosterCopy.Name,
                    PiecesCount = boosterCopy.PiecesCount
                }));
            }
        }

        private async void OnOpenBooster(BoosterData booster)
        {
            Debug.Log($"[BoosterView] Opening booster: {booster.Name}");

            var api = ServiceLocator.Get<ApiService>();
            var result = await api.OpenBooster(booster.ID);

            if (result?.Pieces == null || result.Pieces.Count == 0)
            {
                Debug.LogWarning("[BoosterView] No pieces received, using simulation.");
                ShowResult(SimulateBoosterOpening(booster));
                return;
            }

            var pieces = result.Pieces.ConvertAll(p => new PieceCardData
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

            ShowResult(pieces);
        }

        private void ShowResult(List<PieceCardData> pieces)
        {
            if (_resultPanel) _resultPanel.SetActive(true);

            foreach (Transform child in _resultContainer)
                Destroy(child.gameObject);

            foreach (var piece in pieces)
            {
                var card = Instantiate(_pieceCardPrefab, _resultContainer);
                card.GetComponent<PieceCardUI>()?.Setup(piece);
            }
        }

        private void OnCloseResult()
        {
            if (_resultPanel) _resultPanel.SetActive(false);
        }

        private void OnBackClicked()
        {
            ServiceLocator.Get<UIManager>().ShowView(ViewNames.Menu);
        }

        private List<BoosterData> GetPlaceholderBoosters()
        {
            return new List<BoosterData>
            {
                new() { ID = "booster_starter",  Name = "Starter Pack",       Description = "Parfait pour débuter",             PriceCoins = 0,   PriceGems = 0,   PiecesCount = 3 },
                new() { ID = "booster_standard", Name = "Booster Standard",   Description = "3 pièces aléatoires",              PriceCoins = 100, PriceGems = 0,   PiecesCount = 3 },
                new() { ID = "booster_premium",  Name = "Booster Premium",    Description = "5 pièces, meilleures chances",     PriceCoins = 0,   PriceGems = 50,  PiecesCount = 5 },
                new() { ID = "booster_legendary",Name = "Booster Légendaire", Description = "Garanti Épique ou Légendaire",     PriceCoins = 0,   PriceGems = 150, PiecesCount = 5 },
            };
        }

        private List<PieceCardData> SimulateBoosterOpening(BoosterData booster)
        {
            var allPieces = new List<PieceCardData>
            {
                new() { ID = "biscuit_001",  Name = "Biscuit",  Role = "pawn",   Rarity = "common",    SlotCost = 1, IsOwned = true },
                new() { ID = "granite_001",  Name = "Granite",  Role = "rook",   Rarity = "rare",      SlotCost = 2, IsOwned = true },
                new() { ID = "whisker_001",  Name = "Whisker",  Role = "knight", Rarity = "rare",      SlotCost = 2, IsOwned = true },
                new() { ID = "luna_001",     Name = "Luna",     Role = "bishop", Rarity = "epic",      SlotCost = 3, IsOwned = true },
                new() { ID = "tempete_001",  Name = "Tempête",  Role = "queen",  Rarity = "epic",      SlotCost = 4, IsOwned = true },
                new() { ID = "pharaon_001",  Name = "Pharaon",  Role = "king",   Rarity = "legendary", SlotCost = 5, IsOwned = true },
            };

            var result = new List<PieceCardData>();
            for (int i = 0; i < booster.PiecesCount; i++)
                result.Add(allPieces[Random.Range(0, allPieces.Count)]);

            return result;
        }
    }
}