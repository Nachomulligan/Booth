// ============================================================
// CustomerData.cs
// Pure data model representing one customer's attributes.
// No MonoBehaviour, no Unity deps — just a clean data class.
// Instantiated by CustomerFactory, read by DecisionSystem.
// ============================================================

using UnityEngine;

namespace Booth.Customers
{
    /// <summary>
    /// All information the player must evaluate for one customer.
    /// Immutable after creation — created by CustomerFactory.
    /// </summary>
    [System.Serializable]
    public class CustomerData
    {
        // ── Identity ─────────────────────────────────────────
        /// <summary>Unique ID for this customer instance (for logging/stats).</summary>
        public int Id { get; private set; }

        /// <summary>Visual sprite key — maps to a sprite in CustomerVisualConfig.</summary>
        public string SpriteKey { get; private set; }

        // ── Evaluation conditions ─────────────────────────────
        /// <summary>Amount of money the customer presents (in dollars).</summary>
        public float MoneyPresented { get; private set; }

        /// <summary>True if this customer is a minor. Minors must always be rejected.</summary>
        public bool IsMinor { get; private set; }

        /// <summary>True if the bills presented are counterfeit. Fakes must always be rejected.</summary>
        public bool HasFakeBills { get; private set; }

        /// <summary>True if the customer has extra money to give when asked.</summary>
        public bool CanPayMore { get; private set; }

        /// <summary>Extra money available if asked (only matters when CanPayMore is true).</summary>
        public float ExtraMoneyAvailable { get; private set; }

        // ── Derived ───────────────────────────────────────────
        /// <summary>
        /// True if this customer is completely valid and should be ACCEPTED.
        /// A valid customer: not a minor, no fake bills, sufficient money.
        /// </summary>
        public bool IsValid(float ticketPrice)
        {
            if (IsMinor)       return false;
            if (HasFakeBills)  return false;
            return TotalPossibleMoney >= ticketPrice;
        }

        /// <summary>Max money this customer can ever provide.</summary>
        public float TotalPossibleMoney => MoneyPresented + (CanPayMore ? ExtraMoneyAvailable : 0f);

        // ── Factory constructor (use CustomerFactory, not this directly) ──
        private CustomerData() { }

        public static CustomerData Create(
            int    id,
            string spriteKey,
            float  moneyPresented,
            bool   isMinor,
            bool   hasFakeBills,
            bool   canPayMore,
            float  extraMoney)
        {
            return new CustomerData
            {
                Id                   = id,
                SpriteKey            = spriteKey,
                MoneyPresented       = moneyPresented,
                IsMinor              = isMinor,
                HasFakeBills         = hasFakeBills,
                CanPayMore           = canPayMore,
                ExtraMoneyAvailable  = extraMoney
            };
        }

        public override string ToString() =>
            $"Customer[{Id}] sprite:{SpriteKey} money:{MoneyPresented} " +
            $"minor:{IsMinor} fake:{HasFakeBills} canMore:{CanPayMore}";
    }

    // ============================================================
    // DecisionResult — result of evaluating a decision
    // ============================================================
    /// <summary>
    /// Outcome after the player makes a decision on a customer.
    /// Passed through events so multiple systems can react.
    /// </summary>
    [System.Serializable]
    public class DecisionResult
    {
        public CustomerData  Customer    { get; set; }
        public DecisionType  Decision    { get; set; }
        public bool          IsCorrect   { get; set; }
        public string        ReasonCode  { get; set; } // e.g. "accepted_minor", "correct_reject"

        public override string ToString() =>
            $"Decision:{Decision} Correct:{IsCorrect} Reason:{ReasonCode}";
    }

    /// <summary>The three actions the player can take.</summary>
    public enum DecisionType
    {
        Accept,
        Reject,
        AskForMore
    }
}
