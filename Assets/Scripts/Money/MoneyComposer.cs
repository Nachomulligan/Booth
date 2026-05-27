// ============================================================
// MoneyComposer.cs
// Convierte un monto (float) en una lista de billetes/monedas
// físicos (DenominationInstance[]).
//
// Es una clase pura — sin MonoBehaviour, sin estado.
// CustomerFactory la instancia y la llama.
//
// Algoritmo:
//   1. Decide si el total incluye centavos (según CoinChanceByTier)
//   2. Descompone el monto usando denominaciones de mayor a menor
//      (greedy), garantizando representación exacta en múltiplos de $0.05
//   3. Decide si algún billete es falso (HasFakeBills del CustomerData)
//      y si es así, elige uno al azar para reemplazarlo con fake
//   4. Respeta MaxDisplayPieces — si hay más piezas, colapsa las
//      menores en stacks visuales
// ============================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Booth.Customers
{
    public class MoneyComposer
    {
        private readonly MoneyConfig _config;

        public MoneyComposer(MoneyConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Genera la lista de piezas físicas para un cliente.
        /// </summary>
        /// <param name="amount">Total en dólares a representar físicamente.</param>
        /// <param name="hasFakeBills">Si true, una pieza elegible será falsa.</param>
        /// <param name="tier">Tier de dificultad (0-3) — afecta probabilidad de monedas.</param>
        public List<DenominationInstance> Compose(float amount, bool hasFakeBills, int tier)
        {
            tier = Mathf.Clamp(tier, 0, _config.CoinChanceByTier.Length - 1);

            // ── Paso 1: decidir si el monto incluye centavos ──
            bool includeCoins = Random.value < _config.CoinChanceByTier[tier];

            // Redondear según si hay monedas o no
            float rounded = includeCoins
                ? RoundToNearestCoin(amount)   // múltiplo de $0.10
                : Mathf.Round(amount);          // dólar entero

            // Asegurarse de que sea positivo y representable
            rounded = Mathf.Max(rounded, 0.10f);

            // ── Paso 2: descomposición greedy ─────────────────
            List<DenominationInstance> pieces = Decompose(rounded, includeCoins);

            // ── Paso 3: introducir billete falso si aplica ────
            if (hasFakeBills)
                InjectFakeBill(pieces);

            // ── Paso 4: respetar MaxDisplayPieces ────────────
            pieces = CapPieces(pieces);

            return pieces;
        }

        // ── Paso 2: descomposición greedy ────────────────────

        private List<DenominationInstance> Decompose(float amount, bool allowCoins)
        {
            var pieces = new List<DenominationInstance>();

            // Ordenar denominaciones de mayor a menor
            var defs = _config.Denominations
                .Where(d => allowCoins || d.Kind == DenominationKind.Bill)
                .OrderByDescending(d => d.Value)
                .ToArray();

            // Usar enteros de centavos para evitar floating point errors
            int remaining = Mathf.RoundToInt(amount * 100f);

            foreach (var def in defs)
            {
                int denomCents = Mathf.RoundToInt(def.Value * 100f);
                if (denomCents <= 0) continue;

                while (remaining >= denomCents)
                {
                    pieces.Add(DenominationInstance.Create(def, isFake: false));
                    remaining -= denomCents;
                }
            }

            // Si queda residuo (no representable con denominaciones disponibles),
            // ignorarlo — el float original sigue siendo el valor real en CustomerData.
            return pieces;
        }

        // ── Paso 3: inyección de falso ────────────────────────

        private void InjectFakeBill(List<DenominationInstance> pieces)
        {
            // Solo los billetes que CanBeFake son candidatos
            var candidates = pieces
                .Select((p, i) => new { piece = p, index = i })
                .Where(x => x.piece.Kind == DenominationKind.Bill)
                .ToList();

            if (candidates.Count == 0) return;

            // Elegir uno al azar para hacer falso
            var target = candidates[Random.Range(0, candidates.Count)];

            // Buscar la definición original para obtener FakeSpriteKey
            var def = _config.Denominations
                .FirstOrDefault(d => d.SpriteKey == target.piece.SpriteKey && d.CanBeFake);

            if (def == null) return;

            // Reemplazar la pieza con la versión falsa
            pieces[target.index] = DenominationInstance.Create(def, isFake: true);
        }

        // ── Paso 4: cap de piezas ─────────────────────────────

        private List<DenominationInstance> CapPieces(List<DenominationInstance> pieces)
        {
            if (pieces.Count <= _config.MaxDisplayPieces)
                return pieces;

            // Si hay más piezas que el límite, quedarse con las de mayor valor
            // Las monedas pequeñas se eliminan del visual (pero el valor total en
            // CustomerData sigue siendo correcto — solo afecta la presentación)
            return pieces
                .OrderByDescending(p => p.Value)
                .Take(_config.MaxDisplayPieces)
                .ToList();
        }

        // ── Helpers ───────────────────────────────────────────

        /// <summary>Redondea a múltiplo de $0.10 — la moneda más pequeña del juego.</summary>
        private float RoundToNearestCoin(float amount)
        {
            return Mathf.Round(amount * 10f) / 10f;
        }
    }
}
