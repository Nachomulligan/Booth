// ============================================================
// CustomerFactory.cs
// Generates CustomerData instances based on difficulty tier.
// Pure factory — no MonoBehaviour, no scene deps.
// CustomerManager owns and calls this.
// ============================================================

using UnityEngine;
using Booth.Customers;

namespace Booth.Customers
{
    /// <summary>
    /// Generates CustomerData instances. Separated from
    /// CustomerManager so generation logic can be tested/swapped.
    /// </summary>
    public class CustomerFactory
    {
        private readonly CustomerConfig _config;
        private int _nextId = 0;

        public CustomerFactory(CustomerConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Generates a new customer appropriate for the given difficulty tier.
        /// </summary>
        /// <param name="difficultyTier">0=easy ... 3=hard</param>
        public CustomerData Generate(int difficultyTier)
        {
            int tier = Mathf.Clamp(difficultyTier, 0, 3);

            // ── Determine if minor ────────────────────────────
            bool isMinor = Random.value < _config.MinorChanceByTier[tier];

            // ── Choose sprite ─────────────────────────────────
            string spriteKey = ChooseSpriteKey(isMinor);

            // ── Determine fake bills ──────────────────────────
            // Minors can also have fake bills, but it doesn't matter —
            // they get rejected anyway. Still track it for stats.
            bool hasFakeBills = Random.value < _config.FakeBillChanceByTier[tier];

            // ── Determine money ───────────────────────────────
            float money = _config.MoneyRangesByTier[tier].Random();
            // Round to nearest 25 cents for readability
            money = Mathf.Round(money * 4f) / 4f;

            // ── Determine if they can pay more ────────────────
            bool canPayMore    = false;
            float extraMoney   = 0f;
            bool isShortOnMoney = money < _config.TicketPrice;

            if (isShortOnMoney && !isMinor && !hasFakeBills)
            {
                canPayMore = Random.value < _config.CanPayMoreChance;
                if (canPayMore)
                {
                    extraMoney = _config.ExtraMoneyRange.Random();
                    extraMoney = Mathf.Round(extraMoney * 4f) / 4f;
                }
            }

            return CustomerData.Create(
                id:             _nextId++,
                spriteKey:      spriteKey,
                moneyPresented: money,
                isMinor:        isMinor,
                hasFakeBills:   hasFakeBills,
                canPayMore:     canPayMore,
                extraMoney:     extraMoney
            );
        }

        // ── Private helpers ───────────────────────────────────

        private string ChooseSpriteKey(bool isMinor)
        {
            if (isMinor && _config.MinorSpriteKeys.Length > 0)
            {
                // Pick a random minor sprite
                return _config.MinorSpriteKeys[
                    Random.Range(0, _config.MinorSpriteKeys.Length)];
            }

            // Pick a random adult sprite (exclude minor keys)
            // Collect adult keys
            var adultKeys = System.Array.FindAll(
                _config.CustomerSpriteKeys,
                k => System.Array.IndexOf(_config.MinorSpriteKeys, k) < 0);

            if (adultKeys.Length == 0)
                return _config.CustomerSpriteKeys[0]; // fallback

            return adultKeys[Random.Range(0, adultKeys.Length)];
        }
    }
}
