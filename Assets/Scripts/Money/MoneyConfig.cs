// ============================================================
// MoneyConfig.cs
// ScriptableObject con toda la configuración del dinero físico.
//
// Contiene:
//   - Las denominaciones disponibles (billetes y monedas)
//   - Probabilidad de usar monedas por tier (dificultad)
//   - Probabilidad de que un billete específico sea falso
//
// Create via: Assets > Create > Booth > Money Config
// ============================================================

using UnityEngine;

namespace Booth.Customers
{
    [CreateAssetMenu(fileName = "MoneyConfig", menuName = "Booth/Money Config")]
    public class MoneyConfig : ScriptableObject
    {
        [Header("Denominaciones disponibles")]
        [Tooltip("Definí todos los billetes y monedas del juego acá. " +
                 "El MoneyComposer los usa para armar el cambio físico.")]
        public DenominationDef[] Denominations = new DenominationDef[]
        {
            // Billetes
            new DenominationDef { Value = 5.00f, Kind = DenominationKind.Bill,
                SpriteKey = "bill_5",      FakeSpriteKey = "bill_5_fake",    CanBeFake = true  },
            new DenominationDef { Value = 1.00f, Kind = DenominationKind.Bill,
                SpriteKey = "bill_1",      FakeSpriteKey = "bill_1_fake",    CanBeFake = true  },
            // Monedas
            new DenominationDef { Value = 0.50f, Kind = DenominationKind.Coin,
                SpriteKey = "coin_50",     FakeSpriteKey = "",               CanBeFake = false },
            new DenominationDef { Value = 0.25f, Kind = DenominationKind.Coin,
                SpriteKey = "coin_25",     FakeSpriteKey = "",               CanBeFake = false },
            new DenominationDef { Value = 0.10f, Kind = DenominationKind.Coin,
                SpriteKey = "coin_10",     FakeSpriteKey = "",               CanBeFake = false },
        };

        [Header("Probabilidad de incluir monedas por tier")]
        [Tooltip("Chance (0-1) de que el total incluya centavos (monedas) según el tier. " +
                 "Tier 0 = sin monedas, Tier 3 = casi siempre monedas. " +
                 "Debe tener exactamente 4 valores.")]
        public float[] CoinChanceByTier = { 0.00f, 0.20f, 0.50f, 0.80f };

        [Header("Falsificación")]
        [Tooltip("Chance (0-1) de que un billete falso contamine la transacción. " +
                 "Cuando hay billetes falsos, UNO SOLO es falso — el resto son auténticos. " +
                 "Esto lo decide MoneyComposer, no CustomerFactory.")]
        public float FakeBillChance = 0.30f;

        [Tooltip("Si hay múltiples billetes del mismo tipo, ¿chance de que el falso " +
                 "esté mezclado con auténticos del mismo valor? Hace la detección más difícil.")]
        public bool AllowFakeMixedWithReal = true;

        [Header("Límite de piezas en pantalla")]
        [Tooltip("Máximo de sprites individuales a mostrar en mesa. " +
                 "Si el cambio requiere más piezas, se agrupan visualmente. " +
                 "Recomendado: 7-9 para que quepa en UI.")]
        public int MaxDisplayPieces = 8;
    }
}
