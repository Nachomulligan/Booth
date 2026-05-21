// ============================================================
// IdleDrainIndicator.cs
// Shows a small countdown indicator that appears when a customer
// is at the window. After 5 seconds of inactivity it turns red
// to warn the player mood drain has started.
//
// This is pure UI feedback — no game logic.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Booth.Core;
using Booth.Customers;

namespace Booth.UI
{
    public class IdleDrainIndicator : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("References")]
        [SerializeField] private GameObject      _container;
        [SerializeField] private Image           _timerFill;    // circular fill image
        [SerializeField] private TextMeshProUGUI _countdownText;

        [Header("Settings")]
        [SerializeField] private float _gracePeriod  = 5f;       // must match MoodConfig
        [SerializeField] private Color _safeColor    = Color.yellow;
        [SerializeField] private Color _drainingColor = Color.red;

        // ── State ─────────────────────────────────────────────
        private float _elapsed;
        private bool  _customerPresent;
        private Coroutine _tickCoroutine;

        // ── Unity lifecycle ───────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnCustomerArrived  += HandleCustomerArrived;
            GameEvents.OnDecisionResolved += HandleDecisionResolved;
            GameEvents.OnMoodDrainStarted += HandleDrainStarted;
            GameEvents.OnGameOver         += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnCustomerArrived  -= HandleCustomerArrived;
            GameEvents.OnDecisionResolved -= HandleDecisionResolved;
            GameEvents.OnMoodDrainStarted -= HandleDrainStarted;
            GameEvents.OnGameOver         -= HandleGameOver;
        }

        // ── Event handlers ────────────────────────────────────

        private void HandleCustomerArrived(CustomerData _)
        {
            _customerPresent = true;
            _elapsed = 0f;
            _container.SetActive(true);
            _timerFill.color = _safeColor;

            if (_tickCoroutine != null) StopCoroutine(_tickCoroutine);
            _tickCoroutine = StartCoroutine(CountdownTick());
        }

        private void HandleDecisionResolved(DecisionResult result)
        {
            if (result.Decision == Customers.DecisionType.AskForMore) return;
            StopIndicator();
        }

        private void HandleDrainStarted()
        {
            _timerFill.color = _drainingColor;
        }

        private void HandleGameOver(string _)
        {
            StopIndicator();
        }

        // ── Private ───────────────────────────────────────────

        private IEnumerator CountdownTick()
        {
            while (_customerPresent)
            {
                _elapsed += Time.deltaTime;
                float fill = Mathf.Clamp01(_elapsed / _gracePeriod);
                _timerFill.fillAmount = fill;

                int secondsLeft = Mathf.CeilToInt(_gracePeriod - _elapsed);
                if (_countdownText)
                    _countdownText.text = _elapsed < _gracePeriod ? secondsLeft.ToString() : "!";

                yield return null;
            }
        }

        private void StopIndicator()
        {
            _customerPresent = false;
            _container.SetActive(false);
            if (_tickCoroutine != null) StopCoroutine(_tickCoroutine);
        }
    }
}
