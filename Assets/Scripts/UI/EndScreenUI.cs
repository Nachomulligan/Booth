// ============================================================
// EndScreenUI.cs
// Displays end-game screen (victory or defeat).
// Listens to OnCustomerDeparted to count stats.
// Shows final statistics per GDD "deseable" features.
// ============================================================

using UnityEngine;
using TMPro;
using Booth.Core;
using Booth.Customers;

namespace Booth.UI
{
    public class EndScreenUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI _headlineText;
        [SerializeField] private TextMeshProUGUI _customersServedText;
        [SerializeField] private TextMeshProUGUI _errorsMadeText;
        [SerializeField] private TextMeshProUGUI _flavorText;

        [Header("Victory Flavor Texts")]
        [TextArea]
        [SerializeField] private string[] _victoryFlavors = {
            "The theater erupts. The screen lights up.",
            "The event begins. You were there — and no one will remember that.",
            "May 25, 1977. The doors open."
        };

        [Header("Defeat Flavor Texts")]
        [TextArea]
        [SerializeField] private string[] _defeatFlavors = {
            "The crowd can't wait any longer. They flood the doors.",
            "The event you were guarding swallowed you whole.",
            "The patience of history has limits."
        };

        // ── Statistics ────────────────────────────────────────
        private int _customersServed = 0;
        private int _errorsMade      = 0;
        private bool _isVictory      = false;

        // ── Unity lifecycle ───────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnCustomerDeparted += HandleCustomerDeparted;
            GameEvents.OnGameVictory      += HandleVictory;
            GameEvents.OnGameDefeat       += HandleDefeat;
        }

        private void OnDisable()
        {
            GameEvents.OnCustomerDeparted -= HandleCustomerDeparted;
            GameEvents.OnGameVictory      -= HandleVictory;
            GameEvents.OnGameDefeat       -= HandleDefeat;
        }

        // ── Public API ────────────────────────────────────────

        public void Show()
        {
            string flavor = _isVictory
                ? _victoryFlavors[UnityEngine.Random.Range(0, _victoryFlavors.Length)]
                : _defeatFlavors[UnityEngine.Random.Range(0, _defeatFlavors.Length)];

            if (_customersServedText) _customersServedText.text = $"Customers Served: {_customersServed}";
            if (_errorsMadeText)      _errorsMadeText.text      = $"Errors Made: {_errorsMade}";
            if (_flavorText)          _flavorText.text          = flavor;
        }

        // ── Event handlers ────────────────────────────────────

        private void HandleCustomerDeparted(CustomerData _, DecisionResult result)
        {
            if (result.Decision != Customers.DecisionType.AskForMore)
            {
                _customersServed++;
                if (!result.IsCorrect) _errorsMade++;
            }
        }

        private void HandleVictory() => _isVictory = true;
        private void HandleDefeat()  => _isVictory = false;
    }
}
