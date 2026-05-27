// ============================================================
// MoneyDenomination.cs
// Tipos de datos puros para el sistema de dinero físico.
// Sin MonoBehaviour, sin Unity deps — solo definiciones.
//
// Separado del resto para que MoneyConfig, CustomerData y
// MoneyComposer puedan importarlo sin dependencias cruzadas.
// ============================================================

using System.Collections.Generic;

namespace Booth.Customers
{
    // ── Qué tipo de denominación es ──────────────────────────
    public enum DenominationKind
    {
        Bill,   // billete
        Coin    // moneda
    }

    // ── Una denominación posible (dato estático de diseño) ───
    /// <summary>
    /// Define un tipo de billete o moneda: cuánto vale, qué sprite
    /// le corresponde, y si puede aparecer en versión falsa.
    /// Configurado en MoneyConfig (ScriptableObject).
    /// </summary>
    [System.Serializable]
    public class DenominationDef
    {
        /// <summary>Valor en dólares. Ej: 1.00, 5.00, 0.25</summary>
        public float Value;

        /// <summary>Bill o Coin — afecta layout visual.</summary>
        public DenominationKind Kind;

        /// <summary>
        /// Key del sprite real (auténtico).
        /// Debe existir en MoneyVisualConfig.
        /// </summary>
        public string SpriteKey;

        /// <summary>
        /// Key del sprite falso.
        /// Solo relevante si CanBeFake = true.
        /// </summary>
        public string FakeSpriteKey;

        /// <summary>
        /// ¿Puede aparecer en versión falsa?
        /// Solo los billetes ($1, $5) pueden ser falsos en el diseño del juego.
        /// </summary>
        public bool CanBeFake;
    }

    // ── Una instancia concreta puesta en la mesa ─────────────
    /// <summary>
    /// Un billete o moneda específico que el cliente pone en la mesa.
    /// Tiene valor, si es falso, y el sprite key a mostrar.
    /// Inmutable — creado por MoneyComposer.
    /// </summary>
    public class DenominationInstance
    {
        public float  Value      { get; private set; }
        public bool   IsFake     { get; private set; }
        public string SpriteKey  { get; private set; }  // auténtico o falso según IsFake
        public DenominationKind Kind { get; private set; }

        public static DenominationInstance Create(DenominationDef def, bool isFake)
        {
            return new DenominationInstance
            {
                Value     = def.Value,
                IsFake    = isFake,
                SpriteKey = isFake ? def.FakeSpriteKey : def.SpriteKey,
                Kind      = def.Kind
            };
        }
    }
}
