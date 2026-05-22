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
        [SerializeField] private Image _healthBarBackground;
        [SerializeField] private Image _rarityBorder;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _hpText;

        private int _maxHP;
        private string _ownerID;

        public string OwnerID => _ownerID;
        public int X { get; private set; }
        public int Y { get; private set; }

        public void Setup(PieceStateData data, bool isLocalPlayer)
        {
            _ownerID = data.OwnerID;
            _maxHP = data.MaxHP;
            X = data.X;
            Y = data.Y;

            UpdateHP(data.CurrentHP);

            // Flip la pièce si c'est l'adversaire
            if (!isLocalPlayer)
                transform.localScale = new Vector3(1, -1, 1);
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

        public void MoveTo(Vector3 targetPosition)
        {
            StartCoroutine(MoveCoroutine(targetPosition));
        }

        private System.Collections.IEnumerator MoveCoroutine(Vector3 target)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 start = transform.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            transform.localPosition = target;
        }

        public void PlayAttackAnimation()
        {
            StartCoroutine(AttackCoroutine());
        }

        private System.Collections.IEnumerator AttackCoroutine()
        {
            Vector3 original = transform.localPosition;
            Vector3 forward = original + new Vector3(0, 20f, 0);

            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(original, forward, elapsed / duration);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(forward, original, elapsed / duration);
                yield return null;
            }

            transform.localPosition = original;
        }

        public void PlayDeathAnimation()
        {
            StartCoroutine(DeathCoroutine());
        }

        private System.Collections.IEnumerator DeathCoroutine()
        {
            float duration = 0.5f;
            float elapsed = 0f;
            Color original = _characterIcon ? _characterIcon.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                if (_characterIcon) _characterIcon.color = new Color(original.r, original.g, original.b, alpha);
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsed / duration);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}