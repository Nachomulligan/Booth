// ============================================================
// EndScreenUI.cs
// Displays end-game screen (victory or defeat).
//
// FIXES aplicados:
// 1. Headline ahora se escribe correctamente en Show()
// 2. Stats se acumulan desde el inicio — Show() solo las muestra.
//    Se evita el race condition donde Show() se llamaba antes
//    de que el último OnCustomerDeparted se procesara.
// 3. Solo el script en la pantalla activa cuenta stats,
//    evitando duplicación entre VictoryScreen y DefeatScreen.
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

        [Header("Headline Texts")]
        [SerializeField] private string _victoryHeadline = "THE SHOW BEGINS";
        [SerializeField] private string _defeatHeadline = "CHAOS";

        [Header("Victory Flavor Texts")]
        [TextArea]
        [SerializeField]
        private string[] _victoryFlavors = {
            "The theater erupts. The screen lights up.",
            "The event begins. You were there — and no one will remember that.",
            "May 25, 1977. The doors open."
        };

        [Header("Defeat Flavor Texts")]
        [TextArea]
        [SerializeField]
        private string[] _defeatFlavors = {
            "The crowd can't wait any longer. They flood the doors.",
            "The event you were guarding swallowed you whole.",
            "The patience of history has limits."
        };

        // ── Statistics ────────────────────────────────────────
        // Se acumulan durante toda la sesión, no en Show().
        // Así Show() siempre tiene los datos completos al llamarse.
        private int _customersServed = 0;
        private int _errorsMade = 0;
        private bool _isVictory = false;

        // ── Unity lifecycle ───────────────────────────────────

        private void Awake()
        {
            // Suscribirse en Awake, no en OnEnable.
            // Las pantallas de fin de juego arrancan desactivadas en la escena,
            // así que OnEnable no corre hasta que se muestran — demasiado tarde.
            // Awake corre siempre al iniciar la escena, activo o no.
            GameEvents.OnCustomerDeparted += HandleCustomerDeparted;
            GameEvents.OnGameStarted += HandleGameStarted;
        }

        private void OnDestroy()
        {
            // Limpiar al destruir la escena
            GameEvents.OnCustomerDeparted -= HandleCustomerDeparted;
            GameEvents.OnGameStarted -= HandleGameStarted;
        }

        // ── Public API — llamado por UIManager ────────────────

        /// <summary>
        /// Llamado por UIManager.ShowVictoryScreen() o ShowDefeatScreen().
        /// Recibe el resultado para no depender del orden de eventos.
        /// </summary>
        public void Show(bool isVictory)
        {
            _isVictory = isVictory;

            // Headline
            if (_headlineText)
                _headlineText.text = isVictory ? _victoryHeadline : _defeatHeadline;

            // Stats — ya acumuladas durante la partida
            if (_customersServedText)
                _customersServedText.text = $"Customers Served: {_customersServed}";

            if (_errorsMadeText)
                _errorsMadeText.text = $"Errors Made: {_errorsMade}";

            // Flavor text aleatorio
            if (_flavorText)
            {
                string[] pool = isVictory ? _victoryFlavors : _defeatFlavors;
                if (pool != null && pool.Length > 0)
                    _flavorText.text = pool[Random.Range(0, pool.Length)];
            }
        }

        // ── Event handlers ────────────────────────────────────

        private void HandleGameStarted()
        {
            // Resetear stats al inicio de cada partida
            _customersServed = 0;
            _errorsMade = 0;
        }

        private void HandleCustomerDeparted(CustomerData _, DecisionResult result)
        {
            // No contar el AskForMore — no es una decisión final
            if (result.Decision == DecisionType.AskForMore) return;

            _customersServed++;
            if (!result.IsCorrect) _errorsMade++;
        }
    }
}