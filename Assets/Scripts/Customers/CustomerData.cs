// ============================================================
// CustomerData.cs
// Pure data model representing one customer's attributes.
// Ahora incluye PhysicalMoney: lista de billetes/monedas reales
// que el jugador debe contar visualmente.
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace Booth.Customers
{
    [System.Serializable]
    public class CustomerData
    {
        // ── Identity ─────────────────────────────────────────
        public int    Id        { get; private set; }
        public string SpriteKey { get; private set; }

        // ── Evaluation conditions ─────────────────────────────
        /// <summary>Total numérico — fuente de verdad para DecisionSystem.
        /// Las piezas físicas son presentación; este valor es la lógica.</summary>
        public float MoneyPresented      { get; private set; }
        public bool  IsMinor             { get; private set; }
        public bool  HasFakeBills        { get; private set; }
        public bool  CanPayMore          { get; private set; }
        public float ExtraMoneyAvailable { get; private set; }

        // ── Dinero físico ─────────────────────────────────────
        /// <summary>
        /// Billetes y monedas individuales que el cliente pone en la mesa.
        /// El jugador los cuenta para determinar el total.
        /// Generados por MoneyComposer en CustomerFactory.
        /// </summary>
        public List<DenominationInstance> PhysicalMoney      { get; private set; }

        /// <summary>Piezas extra que se revelan al pedir más. Null hasta que se pide.</summary>
        public List<DenominationInstance> ExtraPhysicalMoney { get; private set; }

        // ── Derived ───────────────────────────────────────────
        public bool IsValid(float ticketPrice)
        {
            if (IsMinor)      return false;
            if (HasFakeBills) return false;
            return TotalPossibleMoney >= ticketPrice;
        }

        public float TotalPossibleMoney =>
            MoneyPresented + (CanPayMore ? ExtraMoneyAvailable : 0f);

        // ── Factory constructor ───────────────────────────────
        private CustomerData() { }

        public static CustomerData Create(
            int    id,
            string spriteKey,
            float  moneyPresented,
            bool   isMinor,
            bool   hasFakeBills,
            bool   canPayMore,
            float  extraMoney,
            List<DenominationInstance> physicalMoney,
            List<DenominationInstance> extraPhysicalMoney)
        {
            return new CustomerData
            {
                Id                   = id,
                SpriteKey            = spriteKey,
                MoneyPresented       = moneyPresented,
                IsMinor              = isMinor,
                HasFakeBills         = hasFakeBills,
                CanPayMore           = canPayMore,
                ExtraMoneyAvailable  = extraMoney,
                PhysicalMoney        = physicalMoney      ?? new List<DenominationInstance>(),
                ExtraPhysicalMoney   = extraPhysicalMoney ?? new List<DenominationInstance>()
            };
        }

        public override string ToString() =>
            $"Customer[{Id}] sprite:{SpriteKey} money:{MoneyPresented} " +
            $"pieces:{PhysicalMoney?.Count ?? 0} minor:{IsMinor} fake:{HasFakeBills} canMore:{CanPayMore}";
    }

    // ============================================================
    // DecisionResult
    // ============================================================
    [System.Serializable]
    public class DecisionResult
    {
        public CustomerData Customer   { get; set; }
        public DecisionType Decision   { get; set; }
        public bool         IsCorrect  { get; set; }
        public string       ReasonCode { get; set; }

        public override string ToString() =>
            $"Decision:{Decision} Correct:{IsCorrect} Reason:{ReasonCode}";
    }

    public enum DecisionType
    {
        Accept,
        Reject,
        AskForMore
    }
}
