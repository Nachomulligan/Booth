// ============================================================
// CustomerConfig.cs
// ScriptableObject holding all tunable customer generation data.
// Edit in Inspector — no recompile needed to tweak balance.
// Create via: Assets > Create > Booth > Customer Config
// ============================================================

using UnityEngine;

namespace Booth.Customers
{
    [CreateAssetMenu(fileName = "CustomerConfig", menuName = "Booth/Customer Config")]
    public class CustomerConfig : ScriptableObject
    {
        [Header("Ticket Price")]
        [Tooltip("Base price of one ticket in dollars.")]
        public float TicketPrice = 4.00f;

        [Header("Money Ranges by Difficulty Tier")]
        [Tooltip("Money ranges per difficulty tier (0=easy, 3=hard). " +
                 "Each tier can produce customers with more ambiguous amounts.")]
        public MoneyRange[] MoneyRangesByTier = new MoneyRange[]
        {
            new MoneyRange { Min = 4.00f,  Max = 8.00f  }, // Tier 0 — always enough
            new MoneyRange { Min = 2.00f,  Max = 6.00f  }, // Tier 1 — sometimes short
            new MoneyRange { Min = 1.00f,  Max = 5.50f  }, // Tier 2 — often ambiguous
            new MoneyRange { Min = 0.50f,  Max = 5.00f  }, // Tier 3 — frequently short
        };

        [Header("Minor Probability by Difficulty Tier")]
        [Tooltip("Chance (0-1) of spawning a minor per tier.")]
        public float[] MinorChanceByTier = { 0.10f, 0.15f, 0.20f, 0.25f };

        [Header("Fake Bill Probability by Difficulty Tier")]
        [Tooltip("Chance (0-1) of fake bills per tier.")]
        public float[] FakeBillChanceByTier = { 0.05f, 0.12f, 0.20f, 0.28f };

        [Header("Ask For More")]
        [Tooltip("Chance that a short-money customer actually has more.")]
        public float CanPayMoreChance = 0.50f;

        [Tooltip("How much extra money a 'can pay more' customer has.")]
        public MoneyRange ExtraMoneyRange = new MoneyRange { Min = 0.50f, Max = 3.00f };

        [Header("Spawn Timing by Difficulty Tier")]
        [Tooltip("Seconds between customers arriving, per tier.")]
        public float[] SpawnDelayByTier = { 0.8f, 0.6f, 0.4f, 0.25f };

        [Header("Sprite Keys")]
        [Tooltip("All available customer sprite keys (must match names in CustomerVisualConfig).")]
        public string[] CustomerSpriteKeys = {
            "customer_adult_a", "customer_adult_b", "customer_adult_c",
            "customer_adult_d", "customer_minor_a", "customer_minor_b"
        };

        [Tooltip("Sprite keys that are always minors (used to guarantee minor detection).")]
        public string[] MinorSpriteKeys = { "customer_minor_a", "customer_minor_b" };
    }

    [System.Serializable]
    public struct MoneyRange
    {
        public float Min;
        public float Max;

        public float Random() => UnityEngine.Random.Range(Min, Max);
    }
}
