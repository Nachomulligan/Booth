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

namespace Booth.UI
{
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
            [Tooltip("Alpha del botón cuando está activo (1 = color original intacto).")]
            [Range(0f, 1f)]
            [SerializeField] private float _activeAlpha = 1f;
            [Tooltip("Alpha del botón cuando está deshabilitado. No toca el color original.")]
            [Range(0f, 1f)]
            [SerializeField] private float _disabledAlpha = 0.35f;

            // ── Colores originales (guardados en Awake) ───────────
            // Así nunca pisamos los colores que el artista puso en el sprite.
            private Color _acceptOriginalColor;
            private Color _rejectOriginalColor;
            private Color _askMoreOriginalColor;

            // ── State ─────────────────────────────────────────────
            private bool _buttonsEnabled = false;

            // ── Unity lifecycle ───────────────────────────────────
            private void Awake()
            {
                // Guardar colores originales ANTES de tocar nada
                _acceptOriginalColor = GetButtonImage(_acceptButton).color;
                _rejectOriginalColor = GetButtonImage(_rejectButton).color;
                _askMoreOriginalColor = GetButtonImage(_askMoreButton).color;

                // Wire button callbacks
                _acceptButton.onClick.AddListener(OnAcceptPressed);
                _rejectButton.onClick.AddListener(OnRejectPressed);
                _askMoreButton.onClick.AddListener(OnAskMorePressed);

                SetButtonsEnabled(false);
            }

            private void OnEnable()
            {
                GameEvents.OnCustomerArrived += HandleCustomerArrived;
                GameEvents.OnDecisionResolved += HandleDecisionResolved;
                GameEvents.OnGameOver += HandleGameOver;
            }

            private void OnDisable()
            {
                GameEvents.OnCustomerArrived -= HandleCustomerArrived;
                GameEvents.OnDecisionResolved -= HandleDecisionResolved;
                GameEvents.OnGameOver -= HandleGameOver;
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

                // Solo modificamos el alpha — el color original del sprite queda intacto
                SetButtonAlpha(_acceptButton, _acceptOriginalColor, enabled ? _activeAlpha : _disabledAlpha);
                SetButtonAlpha(_rejectButton, _rejectOriginalColor, enabled ? _activeAlpha : _disabledAlpha);
                SetButtonAlpha(_askMoreButton, _askMoreOriginalColor, enabled ? _activeAlpha : _disabledAlpha);
            }

            /// <summary>
            /// Aplica un alpha sobre el color original del botón.
            /// No toca RGB — solo transparencia.
            /// </summary>
            private void SetButtonAlpha(Button btn, Color originalColor, float alpha)
            {
                Image img = GetButtonImage(btn);
                if (img == null) return;

                Color c = originalColor;
                c.a = alpha;
                img.color = c;
            }

            /// <summary>Obtiene la Image del botón, con fallback seguro.</summary>
            private Image GetButtonImage(Button btn)
            {
                if (btn == null) return null;
                return btn.GetComponent<Image>();
            }
        }
    }


}
