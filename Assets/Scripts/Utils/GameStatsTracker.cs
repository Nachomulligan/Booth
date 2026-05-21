// ============================================================
// GameStatsTracker.cs
// Tracks statistics throughout the session.
// Read by EndScreenUI at game over.
// Demonstrates the observer pattern for analytics.
// ============================================================

using UnityEngine;
using Booth.Core;
using Booth.Customers;

namespace Booth.Utils
{
    /// <summary>
    /// Passive stats accumulator. Listens to events, never
    /// writes to them. Other systems query this for display.
    /// </summary>
    public class GameStatsTracker : MonoBehaviour
    {
        // ── Stats ─────────────────────────────────────────────
        public int CustomersServed    { get; private set; }
        public int CorrectDecisions   { get; private set; }
        public int WrongDecisions     { get; private set; }
        public int MinorsRejected     { get; private set; }
        public int FakesRejected      { get; private set; }
        public int TimesAskedForMore  { get; private set; }

        public float Accuracy =>
            CustomersServed > 0 ? (float)CorrectDecisions / CustomersServed : 0f;

        // ── Unity lifecycle ───────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnGameStarted      += ResetStats;
            GameEvents.OnDecisionResolved += HandleDecision;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted      -= ResetStats;
            GameEvents.OnDecisionResolved -= HandleDecision;
        }

        // ── Handlers ─────────────────────────────────────────

        private void ResetStats()
        {
            CustomersServed   = 0;
            CorrectDecisions  = 0;
            WrongDecisions    = 0;
            MinorsRejected    = 0;
            FakesRejected     = 0;
            TimesAskedForMore = 0;
        }

        private void HandleDecision(DecisionResult result)
        {
            if (result.Decision == DecisionType.AskForMore)
            {
                TimesAskedForMore++;
                return;
            }

            CustomersServed++;

            if (result.IsCorrect)
            {
                CorrectDecisions++;

                if (result.ReasonCode == "correct_reject_minor") MinorsRejected++;
                if (result.ReasonCode == "correct_reject_fake")  FakesRejected++;
            }
            else
            {
                WrongDecisions++;
            }
        }
    }
}
