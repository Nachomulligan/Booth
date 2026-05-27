// ============================================================
// CustomerManager.cs
// Manages the customer queue: spawning, presenting, departing.
// Owns the CustomerFactory. Listens to decision events to
// know when to advance the queue.
// ============================================================

using System.Collections;
using UnityEngine;
using Booth.Core;
using Booth.Customers;
using Booth.Decisions;

namespace Booth.Customers
{
    public class CustomerManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Config")]
        [SerializeField] private CustomerConfig _config;
        [SerializeField] private MoneyConfig    _moneyConfig;

        [Header("Scene References")]
        [Tooltip("The presenter handles the visual display of the customer.")]
        [SerializeField] private CustomerPresenter _presenter;

        // ── State ─────────────────────────────────────────────
        private CustomerFactory  _factory;
        private CustomerData     _currentCustomer;
        private int              _currentDifficultyTier = 0;
        private bool             _awaitingDecision      = false;
        private bool             _askedForMoreAlready   = false;

        // ── Unity lifecycle ───────────────────────────────────
        private void Awake()
        {
            var composer = new MoneyComposer(_moneyConfig);
            _factory = new CustomerFactory(_config, composer);
        }

        private void OnEnable()
        {
            GameEvents.OnDecisionResolved  += HandleDecisionResolved;
            GameEvents.OnDifficultyChanged += HandleDifficultyChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnDecisionResolved  -= HandleDecisionResolved;
            GameEvents.OnDifficultyChanged -= HandleDifficultyChanged;
        }

        // ── Public API ────────────────────────────────────────

        /// <summary>Spawns the next customer after a short arrival delay.</summary>
        public void SpawnNextCustomer()
        {
            float delay = _config.SpawnDelayByTier[_currentDifficultyTier];
            StartCoroutine(SpawnAfterDelay(delay));
        }

        /// <summary>Clears the current customer without firing a decision event.</summary>
        public void ClearCurrentCustomer()
        {
            StopAllCoroutines();
            _currentCustomer   = null;
            _awaitingDecision  = false;
            _presenter.HideCustomer();
        }

        /// <summary>Returns current customer data (read by DecisionSystem).</summary>
        public CustomerData CurrentCustomer => _currentCustomer;

        /// <summary>True if Ask For More has already been used on this customer.</summary>
        public bool AskedForMoreAlready => _askedForMoreAlready;

        // ── Private ───────────────────────────────────────────

        private IEnumerator SpawnAfterDelay(float delay)
        {
            _awaitingDecision = false;
            _presenter.ShowArrivalAnimation();

            yield return new WaitForSeconds(delay);

            _currentCustomer      = _factory.Generate(_currentDifficultyTier);
            _askedForMoreAlready  = false;
            _awaitingDecision     = true;

            _presenter.PresentCustomer(_currentCustomer);
            GameEvents.TriggerCustomerArrived(_currentCustomer);

            Debug.Log($"[CustomerManager] Arrived: {_currentCustomer}");
        }

        private void HandleDecisionResolved(DecisionResult result)
        {
            if (result.Decision == DecisionType.AskForMore)
            {
                // Don't advance queue — same customer, now with extra money shown
                _askedForMoreAlready = true;
                _presenter.ShowExtraMoney(_currentCustomer);
                return;
            }

            // Customer leaves
            _awaitingDecision = false;
            _presenter.DepartCustomer(result.Decision);
            GameEvents.TriggerCustomerDeparted(_currentCustomer, result);

            // Spawn the next one
            SpawnNextCustomer();
        }

        private void HandleDifficultyChanged(int tier)
        {
            _currentDifficultyTier = tier;
        }
    }
}
