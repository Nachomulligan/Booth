// ============================================================
// MoodSystem.cs
// The emotional core of the game.
//
// Rules:
//   - Mood starts at 1.0 (full)
//   - Mood is STABLE while player decides
//   - After 5 seconds without a decision → gradual drain starts
//   - Any decision stops the drain timer
//   - WRONG decision → instant heavy penalty
//   - Correct AskForMore → small drain (slight delay penalty)
//   - Mood hits 0 → game over event fired (state machine handles it)
//
// This system does NOT care about victory/defeat — it just
// manages the value and fires events. GameStateMachine reacts.
// ============================================================

using UnityEngine;
using Booth.Core;
using Booth.Customers;

namespace Booth.Mood
{
    public class MoodSystem : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Mood Config")]
        [SerializeField] private MoodConfig _config;

        // ── State ─────────────────────────────────────────────
        private float _currentMood;          // 0 to _config.MaxMood
        private float _idleTimer;            // seconds since last decision
        private bool  _isDraining;           // is the idle drain active?
        private bool  _gameActive;           // only drain/react when playing

        // ── Public ────────────────────────────────────────────
        public float NormalizedMood => _currentMood / _config.MaxMood;
        public float CurrentMood    => _currentMood;

        // ── Unity lifecycle ───────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnGameStarted      += HandleGameStarted;
            GameEvents.OnGameOver         += HandleGameOver;
            GameEvents.OnCustomerArrived  += HandleCustomerArrived;
            GameEvents.OnDecisionResolved += HandleDecisionResolved;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted      -= HandleGameStarted;
            GameEvents.OnGameOver         -= HandleGameOver;
            GameEvents.OnCustomerArrived  -= HandleCustomerArrived;
            GameEvents.OnDecisionResolved -= HandleDecisionResolved;
        }

        private void Update()
        {
            if (!_gameActive) return;

            TickIdleTimer();
        }

        // ── Public API ────────────────────────────────────────

        public void Initialize()
        {
            _currentMood = _config.MaxMood;
            _idleTimer   = 0f;
            _isDraining  = false;
            _gameActive  = false;

            BroadcastMood();
        }

        // ── Event handlers ────────────────────────────────────

        private void HandleGameStarted()
        {
            _gameActive = true;
        }

        private void HandleGameOver(string _)
        {
            _gameActive = false;
            _isDraining = false;
        }

        private void HandleCustomerArrived(CustomerData _)
        {
            // Reset idle timer — new customer, clock starts
            ResetIdleTimer();
        }

        private void HandleDecisionResolved(DecisionResult result)
        {
            // Stop idle drain — player acted
            ResetIdleTimer();

            if (result.Decision == Booth.Customers.DecisionType.AskForMore)
            {
                // AskForMore is not a final decision — don't apply penalty/bonus yet
                // But it does reset the idle timer (player did something)
                return;
            }

            if (!result.IsCorrect)
            {
                ApplyPenalty(result.ReasonCode);
            }
            // Correct decisions don't restore mood — it's about not losing, not gaining
        }

        // ── Idle drain logic ──────────────────────────────────

        private void TickIdleTimer()
        {
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= _config.IdleGracePeriod)
            {
                if (!_isDraining)
                {
                    _isDraining = true;
                    GameEvents.TriggerMoodDrainStarted();
                    Debug.Log("[MoodSystem] Idle drain started.");
                }

                // Drain per second
                float drain = _config.IdleDrainPerSecond * Time.deltaTime;
                ModifyMood(-drain);
            }
        }

        private void ResetIdleTimer()
        {
            if (_isDraining)
            {
                _isDraining = false;
                GameEvents.TriggerMoodDrainStopped();
            }
            _idleTimer = 0f;
        }

        // ── Penalty logic ─────────────────────────────────────

        private void ApplyPenalty(string reasonCode)
        {
            float penalty = reasonCode switch
            {
                "accepted_minor"           => _config.PenaltyAcceptMinor,
                "accepted_fake_bills"      => _config.PenaltyAcceptFake,
                "accepted_insufficient_money" => _config.PenaltyAcceptInsufficient,
                "rejected_valid_customer"  => _config.PenaltyRejectValid,
                "asked_more_unnecessary"   => _config.PenaltyAskUnnecessary,
                _                          => _config.PenaltyDefault
            };

            Debug.Log($"[MoodSystem] Penalty applied: {reasonCode} → -{penalty}");
            ModifyMood(-penalty);
            GameEvents.TriggerAudioCue("crowd_groan");
        }

        // ── Mood mutation ─────────────────────────────────────

        private void ModifyMood(float delta)
        {
            _currentMood = Mathf.Clamp(_currentMood + delta, 0f, _config.MaxMood);
            BroadcastMood();

            if (_currentMood <= 0f)
            {
                _gameActive = false;
                // GameStateMachine listens to OnMoodChanged and handles defeat
            }
        }

        private void BroadcastMood() => GameEvents.TriggerMoodChanged(NormalizedMood);
    }
}
