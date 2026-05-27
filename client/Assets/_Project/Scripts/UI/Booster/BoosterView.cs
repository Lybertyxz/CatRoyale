using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CatRoyale.Core;
using CatRoyale.UI.Collection;
using CatRoyale.Network;
using CatRoyale.Data;

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
            var result = await api.GetBoosters();

            if (!result.Success)
            {
                Debug.LogWarning($"[BoosterView] {result.Error}");
                return;
            }

            var boosters = result.Data.ConvertAll(b => new BoosterData
            {
                ID = b.ID,
                Name = b.Name,
                Description = b.Description,
                PriceCoins = b.PriceCoins,
                PriceGems = b.PriceGems,
                PiecesCount = b.PiecesCount
            });

            foreach (var booster in boosters)
            {
                var card = Instantiate(_boosterCardPrefab, _boosterContainer);
                var boosterCopy = booster;
                card.GetComponent<BoosterCardUI>()?.Setup(boosterCopy, () => OnOpenBooster(boosterCopy));
            }
        }

        private async void OnOpenBooster(BoosterData booster)
        {
            var api = ServiceLocator.Get<ApiService>();
            var repo = ServiceLocator.Get<PieceRepository>();
            var result = await api.OpenBooster(booster.ID);

            if (!result.Success)
            {
                Debug.LogWarning($"[BoosterView] {result.Error}");
                return;
            }

            var pieces = result.Data.Pieces.ConvertAll(p =>
            {
                var model = repo.Get(p.ID);
                return new PieceCardData
                {
                    ID = p.ID,
                    Name = p.Name,
                    Role = p.Role,
                    Rarity = p.Rarity,
                    SlotCost = p.SlotCost,
                    MaxHP = p.MaxHP,
                    Attack = p.Attack,
                    Armor = p.Armor,
                    IsOwned = true,
                    Icon = model?.Icon
                };
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
    }
}