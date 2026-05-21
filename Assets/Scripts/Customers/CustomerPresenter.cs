// ============================================================
// CustomerPresenter.cs
// Handles all visual representation of the current customer:
// sprite display, money display, arrival/departure animations.
// Pure presentation — no game logic.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Booth.Customers;
using Booth.Decisions;

namespace Booth.Customers
{
    public class CustomerPresenter : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Customer Visual")]
        [SerializeField] private Image    _customerSprite;
        [SerializeField] private Animator _customerAnimator;  // optional

        [Header("Money Display")]
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private GameObject      _billsContainer;
        [SerializeField] private Image           _fakeBillIndicator; // subtle visual cue

        [Header("Sprite Config")]
        [SerializeField] private CustomerVisualConfig _visualConfig;

        [Header("Animation Settings")]
        [SerializeField] private float _slideDuration  = 0.3f;
        [SerializeField] private float _departDuration = 0.25f;

        // ── Cached positions ──────────────────────────────────
        private Vector3 _presentedPosition;
        private Vector3 _offscreenRight;
        private Vector3 _offscreenLeft;

        // ── Unity lifecycle ───────────────────────────────────
        private void Awake()
        {
            _presentedPosition = _customerSprite.transform.localPosition;
            _offscreenRight    = _presentedPosition + Vector3.right  * 800f;
            _offscreenLeft     = _presentedPosition + Vector3.left   * 800f;

            HideCustomer();
        }

        // ── Public API ────────────────────────────────────────

        public void PresentCustomer(CustomerData data)
        {
            // Set sprite
            Sprite sp = _visualConfig.GetSprite(data.SpriteKey);
            if (sp != null) _customerSprite.sprite = sp;

            // Set money text
            _moneyText.text = FormatMoney(data.MoneyPresented);

            // Show/hide fake bill indicator
            // Subtle — visible but requires attention to notice
            _fakeBillIndicator.gameObject.SetActive(data.HasFakeBills);

            // Slide in from right
            _customerSprite.transform.localPosition = _offscreenRight;
            _customerSprite.gameObject.SetActive(true);
            _billsContainer.SetActive(true);

            StartCoroutine(SlideToPosition(_customerSprite.transform, _presentedPosition, _slideDuration));
        }

        public void ShowExtraMoney(CustomerData data)
        {
            // Animate the money amount updating
            float total = data.MoneyPresented + data.ExtraMoneyAvailable;
            StartCoroutine(AnimateMoneyUpdate(data.MoneyPresented, total));
        }

        public void ShowArrivalAnimation()
        {
            // Optional: animate the queue shifting
        }

        public void DepartCustomer(DecisionType decision)
        {
            // Slide direction depends on decision
            Vector3 target = decision == DecisionType.Accept ? _offscreenLeft : _offscreenRight;
            StartCoroutine(SlideAndHide(_customerSprite.transform, target, _departDuration));
        }

        public void HideCustomer()
        {
            _customerSprite.gameObject.SetActive(false);
            _billsContainer.SetActive(false);
        }

        // ── Private helpers ───────────────────────────────────

        private string FormatMoney(float amount) => $"${amount:F2}";

        private IEnumerator SlideToPosition(Transform t, Vector3 target, float duration)
        {
            Vector3 start   = t.localPosition;
            float   elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed          += Time.deltaTime;
                float normalized  = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                t.localPosition   = Vector3.Lerp(start, target, normalized);
                yield return null;
            }

            t.localPosition = target;
        }

        private IEnumerator SlideAndHide(Transform t, Vector3 target, float duration)
        {
            yield return SlideToPosition(t, target, duration);
            HideCustomer();
        }

        private IEnumerator AnimateMoneyUpdate(float from, float to)
        {
            float elapsed  = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float current = Mathf.Lerp(from, to, elapsed / duration);
                _moneyText.text = FormatMoney(current);
                yield return null;
            }

            _moneyText.text = FormatMoney(to);
        }
    }
}
