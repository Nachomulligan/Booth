// ============================================================
// CustomerFactory.cs
// Genera CustomerData completos, ahora incluyendo dinero físico
// mediante MoneyComposer.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using Booth.Customers;

namespace Booth.Customers
{
    public class CustomerFactory
    {
        private readonly CustomerConfig _config;
        private readonly MoneyComposer _composer;
        private int _nextId = 0;

        public CustomerFactory(CustomerConfig config, MoneyComposer composer)
        {
            _config = config;
            _composer = composer;
        }

        public CustomerData Generate(int difficultyTier)
        {
            int tier = Mathf.Clamp(difficultyTier, 0, 3);

            // ── Identidad ─────────────────────────────────────
            bool isMinor = Random.value < _config.MinorChanceByTier[tier];
            string spriteKey = ChooseSpriteKey(isMinor);

            // ── Billetes falsos ───────────────────────────────
            bool hasFakeBills = Random.value < _config.FakeBillChanceByTier[tier];

            // ── Monto total ───────────────────────────────────
            float money = _config.MoneyRangesByTier[tier].Random();
            // No redondeamos acá — MoneyComposer lo hace según si hay monedas

            // ── Puede dar más ─────────────────────────────────
            // Menores y de billetes falsos TAMBIÉN pueden tener más dinero.
            // El jugador tiene que detectar el problema por su cuenta —
            // el sistema no lo protege cortando CanPayMore.
            // Si le pedís más a un menor y da: igual se rechaza.
            // Si le pedís más a alguien con fake: el extra también puede ser fake.
            bool canPayMore = false;
            float extraMoney = 0f;
            bool isShort = money < _config.TicketPrice;

            if (isShort)
            {
                canPayMore = Random.value < _config.CanPayMoreChance;
                if (canPayMore)
                {
                    extraMoney = _config.ExtraMoneyRange.Random();
                    extraMoney = Mathf.Round(extraMoney * 4f) / 4f;
                }
            }

            // ── Dinero físico ─────────────────────────────────
            // MoneyComposer convierte el float en billetes/monedas concretos.
            // HasFakeBills le dice si debe inyectar un billete falso.
            List<DenominationInstance> physical = _composer.Compose(
                amount: money,
                hasFakeBills: hasFakeBills,
                tier: tier);

            // El monto real es la suma de las piezas generadas
            // (puede diferir levemente del float original por redondeo de monedas)
            float actualMoney = SumPieces(physical);

            // Extra money también como piezas físicas
            List<DenominationInstance> extraPhysical = null;
            if (canPayMore && extraMoney > 0f)
            {
                extraPhysical = _composer.Compose(
                    amount: extraMoney,
                    hasFakeBills: false,   // el extra nunca es falso
                    tier: tier);
                extraMoney = SumPieces(extraPhysical);
            }

            return CustomerData.Create(
                id: _nextId++,
                spriteKey: spriteKey,
                moneyPresented: actualMoney,
                isMinor: isMinor,
                hasFakeBills: hasFakeBills,
                canPayMore: canPayMore,
                extraMoney: extraMoney,
                physicalMoney: physical,
                extraPhysicalMoney: extraPhysical);
        }

        // ── Helpers ───────────────────────────────────────────

        private float SumPieces(List<DenominationInstance> pieces)
        {
            if (pieces == null) return 0f;
            float total = 0f;
            foreach (var p in pieces) total += p.Value;
            // Redondear para evitar floating point drift
            return Mathf.Round(total * 100f) / 100f;
        }

        private string ChooseSpriteKey(bool isMinor)
        {
            if (isMinor && _config.MinorSpriteKeys.Length > 0)
                return _config.MinorSpriteKeys[Random.Range(0, _config.MinorSpriteKeys.Length)];

            var adultKeys = System.Array.FindAll(
                _config.CustomerSpriteKeys,
                k => System.Array.IndexOf(_config.MinorSpriteKeys, k) < 0);

            if (adultKeys.Length == 0) return _config.CustomerSpriteKeys[0];
            return adultKeys[Random.Range(0, adultKeys.Length)];
        }
    }
}