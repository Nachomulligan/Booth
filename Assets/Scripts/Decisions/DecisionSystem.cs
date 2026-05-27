// ============================================================
// DecisionSystem.cs
// The rule engine. Validates each player decision (Accept,
// Reject, AskForMore) against the current customer's data.
// Fires DecisionResolved with outcome — MoodSystem reacts.
// This is the bureaucratic heart of the game.
// ============================================================

using UnityEngine;
using Booth.Core;
using Booth.Customers;
using Booth.Decisions;

namespace Booth.Decisions
{
    public class DecisionSystem : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Dependencies")]
        [SerializeField] private CustomerManager _customerManager;
        [SerializeField] private CustomerConfig _config;

        // ── Unity lifecycle ───────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnDecisionAccept += HandleAccept;
            GameEvents.OnDecisionReject += HandleReject;
            GameEvents.OnDecisionAskMore += HandleAskMore;
        }

        private void OnDisable()
        {
            GameEvents.OnDecisionAccept -= HandleAccept;
            GameEvents.OnDecisionReject -= HandleReject;
            GameEvents.OnDecisionAskMore -= HandleAskMore;
        }

        // ── Decision handlers ─────────────────────────────────

        private void HandleAccept()
        {
            CustomerData c = _customerManager.CurrentCustomer;
            if (c == null) return;

            DecisionResult result = EvaluateAccept(c);
            Debug.Log($"[DecisionSystem] ACCEPT → {result}");
            GameEvents.TriggerDecisionResolved(result);

            // Audio cue
            string cue = result.IsCorrect ? "decision_accept_correct" : "decision_accept_wrong";
            GameEvents.TriggerAudioCue(cue);
        }

        private void HandleReject()
        {
            CustomerData c = _customerManager.CurrentCustomer;
            if (c == null) return;

            DecisionResult result = EvaluateReject(c);
            Debug.Log($"[DecisionSystem] REJECT → {result}");
            GameEvents.TriggerDecisionResolved(result);

            string cue = result.IsCorrect ? "decision_reject_correct" : "decision_reject_wrong";
            GameEvents.TriggerAudioCue(cue);
        }

        private void HandleAskMore()
        {
            CustomerData c = _customerManager.CurrentCustomer;
            if (c == null) return;

            // Cannot ask for more if already asked
            if (_customerManager.AskedForMoreAlready)
            {
                Debug.Log("[DecisionSystem] Already asked for more — ignoring.");
                return;
            }

            DecisionResult result = EvaluateAskMore(c);
            Debug.Log($"[DecisionSystem] ASK MORE → {result}");
            GameEvents.TriggerDecisionResolved(result);
            GameEvents.TriggerAudioCue("decision_ask_more");
        }

        // ── Rule evaluations ──────────────────────────────────

        /// <summary>
        /// ACCEPT rules:
        /// - Minor → WRONG (accepted_minor)
        /// - Fake bills → WRONG (accepted_fake)
        /// - Insufficient money (even after ask more) → WRONG (accepted_insufficient)
        /// - All conditions pass → CORRECT (correct_accept)
        /// </summary>
        private DecisionResult EvaluateAccept(CustomerData c)
        {
            float effectiveMoney = c.MoneyPresented;

            // If player already asked for more, count extra money
            if (_customerManager.AskedForMoreAlready && c.CanPayMore)
                effectiveMoney += c.ExtraMoneyAvailable;

            if (c.IsMinor)
                return Wrong(c, DecisionType.Accept, "accepted_minor");

            if (c.HasFakeBills)
                return Wrong(c, DecisionType.Accept, "accepted_fake_bills");

            if (effectiveMoney < _config.TicketPrice)
                return Wrong(c, DecisionType.Accept, "accepted_insufficient_money");

            return Correct(c, DecisionType.Accept, "correct_accept");
        }

        /// <summary>
        /// REJECT rules:
        /// - Minor → CORRECT (correct_reject_minor)
        /// - Fake bills → CORRECT (correct_reject_fake)
        /// - Insufficient money AND can't pay more → CORRECT (correct_reject_insufficient)
        /// - Valid customer (not minor, no fake, enough money) → WRONG (rejected_valid)
        /// </summary>
        private DecisionResult EvaluateReject(CustomerData c)
        {
            if (c.IsMinor)
                return Correct(c, DecisionType.Reject, "correct_reject_minor");

            if (c.HasFakeBills)
                return Correct(c, DecisionType.Reject, "correct_reject_fake");

            // Check if money was actually insufficient
            float effectiveMoney = c.MoneyPresented;
            if (_customerManager.AskedForMoreAlready && c.CanPayMore)
                effectiveMoney += c.ExtraMoneyAvailable;

            if (effectiveMoney < _config.TicketPrice)
                return Correct(c, DecisionType.Reject, "correct_reject_insufficient");

            // Rejecting a valid customer is WRONG
            return Wrong(c, DecisionType.Reject, "rejected_valid_customer");
        }

        /// <summary>
        /// ASK FOR MORE rules:
        /// - Minor or fake bills → this is weird but we allow it (neutral, no mood change)
        ///   In practice the UI should disable AskMore for obvious minors/fakes, but we handle it gracefully.
        /// - Not short on money → WRONG (asked_more_unnecessary)
        /// - Short on money → CORRECT (correct_ask_more)
        ///   Note: AskForMore doesn't end the customer interaction.
        /// </summary>
        private DecisionResult EvaluateAskMore(CustomerData c)
        {
            // Si es menor o tiene billetes falsos, pedir más es neutral — sin penalidad.
            // El jugador todavía no detectó el problema real, y castigarlo por pedir
            // más plata antes de rechazar sería injusto. El error real viene al Aceptar.
            if (c.IsMinor || c.HasFakeBills)
                return Correct(c, DecisionType.AskForMore, "ask_more_on_invalid_neutral");

            // Si ya tiene suficiente dinero y le pedís más → penalidad
            if (c.MoneyPresented >= _config.TicketPrice)
                return Wrong(c, DecisionType.AskForMore, "asked_more_unnecessary");

            // Correcto pedir más cuando la plata no alcanza
            return Correct(c, DecisionType.AskForMore, "correct_ask_more");
        }

        // ── Helpers ───────────────────────────────────────────

        private DecisionResult Correct(CustomerData c, DecisionType type, string reason) =>
            new DecisionResult { Customer = c, Decision = type, IsCorrect = true, ReasonCode = reason };

        private DecisionResult Wrong(CustomerData c, DecisionType type, string reason) =>
            new DecisionResult { Customer = c, Decision = type, IsCorrect = false, ReasonCode = reason };
    }
}

// Keep DecisionType in the Decisions namespace
namespace Booth.Decisions
{
    // (DecisionType is defined in CustomerData.cs in Booth.Customers,
    //  but we re-expose it here via using for convenience)
}