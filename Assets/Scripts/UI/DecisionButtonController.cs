// ============================================================
// DecisionButtonController.cs
// Handles the three player action buttons: ACCEPT, REJECT,
// ASK FOR MORE. Translates UI events → GameEvents.
//
// Also manages button state:
//   - Disabled when no customer is present
//   - AskForMore disabled after it's been used once
//   - Visual feedback on press
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Booth.Core;
using Booth.Customers;

namespace Booth.UI
{
    public class DecisionButtonController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Buttons")]
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _rejectButton;
        [SerializeField] private Button _askMoreButton;

        [Header("Visual Feedback")]
        [SerializeField] private Color _activeColor   = Color.white;
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        // ── State ─────────────────────────────────────────────
        private bool _buttonsEnabled = false;

        // ── Unity lifecycle ───────────────────────────────────
        private void Awake()
        {
            // Wire button callbacks
            _acceptButton.onClick.AddListener(OnAcceptPressed);
            _rejectButton.onClick.AddListener(OnRejectPressed);
            _askMoreButton.onClick.AddListener(OnAskMorePressed);

            SetButtonsEnabled(false);
        }

        private void OnEnable()
        {
            GameEvents.OnCustomerArrived  += HandleCustomerArrived;
            GameEvents.OnDecisionResolved += HandleDecisionResolved;
            GameEvents.OnGameOver         += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnCustomerArrived  -= HandleCustomerArrived;
            GameEvents.OnDecisionResolved -= HandleDecisionResolved;
            GameEvents.OnGameOver         -= HandleGameOver;
        }

        // ── Button callbacks ──────────────────────────────────

        private void OnAcceptPressed()
        {
            if (!_buttonsEnabled) return;
            SetButtonsEnabled(false); // Prevent double-tapping
            GameEvents.TriggerDecisionAccept();
        }

        private void OnRejectPressed()
        {
            if (!_buttonsEnabled) return;
            SetButtonsEnabled(false);
            GameEvents.TriggerDecisionReject();
        }

        private void OnAskMorePressed()
        {
            if (!_buttonsEnabled) return;
            // AskForMore doesn't disable all buttons — customer stays
            _askMoreButton.interactable = false; // Can only ask once
            GameEvents.TriggerDecisionAskMore();
        }

        // ── Event handlers ────────────────────────────────────

        private void HandleCustomerArrived(CustomerData _)
        {
            SetButtonsEnabled(true);
            _askMoreButton.interactable = true; // Reset for new customer
        }

        private void HandleDecisionResolved(DecisionResult result)
        {
            if (result.Decision != Customers.DecisionType.AskForMore)
            {
                // Final decision — disable until next customer
                SetButtonsEnabled(false);
            }
            // AskForMore: keep accept/reject enabled, askMore is already disabled
        }

        private void HandleGameOver(string _)
        {
            SetButtonsEnabled(false);
        }

        // ── Helpers ───────────────────────────────────────────

        private void SetButtonsEnabled(bool enabled)
        {
            _buttonsEnabled = enabled;

            _acceptButton.interactable = enabled;
            _rejectButton.interactable = enabled;
            if (enabled) _askMoreButton.interactable = true;

            // Visual tint
            SetButtonTint(_acceptButton,  enabled);
            SetButtonTint(_rejectButton,  enabled);
            SetButtonTint(_askMoreButton, enabled);
        }

        private void SetButtonTint(Button btn, bool active)
        {
            Image img = btn.GetComponent<Image>();
            if (img != null)
                img.color = active ? _activeColor : _disabledColor;
        }
    }
}
