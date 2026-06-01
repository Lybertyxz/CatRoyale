using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CatRoyale.Gameplay
{
    public class PieceView : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Image _characterIcon;
        [SerializeField] private Image _healthBar;
        [SerializeField] private Image _rarityBorder;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _hpText;

        private int _maxHP;
        private string _ownerID;

        public string OwnerID => _ownerID;
        public int X { get; private set; }
        public int Y { get; private set; }

        // ─── Setup ────────────────────────────────────────────

        /// <summary>
        /// Initialise la pièce. L'icône est passée depuis BoardView qui a déjà accès au PieceRepository.
        /// </summary>
        public void Setup(PieceStateData data, bool isLocalPlayer, Sprite icon = null)
        {
            _ownerID = data.OwnerID;
            _maxHP = data.MaxHP > 0 ? data.MaxHP : 1; // guard division par zéro
            X = data.X;
            Y = data.Y;

            if (_characterIcon && icon != null)
                _characterIcon.sprite = icon;

            UpdateHP(data.CurrentHP);
        }

        // ─── State Updates ────────────────────────────────────

        public void UpdatePosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void UpdateHP(int currentHP)
        {
            if (_hpText) _hpText.text = currentHP.ToString();
            if (_healthBar)
            {
                float ratio = (float)currentHP / _maxHP;
                _healthBar.fillAmount = ratio;
                _healthBar.color = ratio > 0.5f ? Color.green :
                                   ratio > 0.25f ? Color.yellow : Color.red;
            }
        }

        // ─── Animations ───────────────────────────────────────

        public void MoveTo(Vector2 targetPosition)
        {
            StopCoroutine(nameof(MoveCoroutine));
            StartCoroutine(MoveCoroutine(targetPosition));
        }

        private IEnumerator MoveCoroutine(Vector2 target)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector2 start = ((RectTransform)transform).anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ((RectTransform)transform).anchoredPosition = Vector2.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            ((RectTransform)transform).anchoredPosition = target;
        }

        public void PlayAttackAnimation()
        {
            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            Vector2 original = ((RectTransform)transform).anchoredPosition;
            Vector2 forward = original + new Vector2(0, 20f);
            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ((RectTransform)transform).anchoredPosition = Vector2.Lerp(original, forward, elapsed / duration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                ((RectTransform)transform).anchoredPosition = Vector2.Lerp(forward, original, elapsed / duration);
                yield return null;
            }

            ((RectTransform)transform).anchoredPosition = original;
        }

        public void PlayDeathAnimation()
        {
            StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Color original = _characterIcon ? _characterIcon.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                if (_characterIcon)
                    _characterIcon.color = new Color(original.r, original.g, original.b, alpha);
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsed / duration);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}