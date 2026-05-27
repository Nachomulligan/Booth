// ============================================================
// MoneyVisualConfig.cs
// ScriptableObject que mapea sprite keys de dinero a Sprites.
// Funciona igual que CustomerVisualConfig pero para billetes
// y monedas (reales y falsas).
//
// Create via: Assets > Create > Booth > Money Visual Config
//
// Keys que necesitás definir (deben coincidir con MoneyConfig):
//   Billetes reales:  bill_5, bill_1
//   Billetes falsos:  bill_5_fake, bill_1_fake
//   Monedas:          coin_50, coin_25, coin_10
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace Booth.Customers
{
    [CreateAssetMenu(fileName = "MoneyVisualConfig", menuName = "Booth/Money Visual Config")]
    public class MoneyVisualConfig : ScriptableObject
    {
        [System.Serializable]
        public struct SpriteEntry
        {
            public string Key;
            public Sprite Sprite;
        }

        [Tooltip("Mapeo de key → Sprite para cada denominación.\n\n" +
                 "Keys requeridos:\n" +
                 "  bill_5       → billete de $5 auténtico\n" +
                 "  bill_5_fake  → billete de $5 falso\n" +
                 "  bill_1       → billete de $1 auténtico\n" +
                 "  bill_1_fake  → billete de $1 falso\n" +
                 "  coin_50      → moneda de $0.50\n" +
                 "  coin_25      → moneda de $0.25\n" +
                 "  coin_10      → moneda de $0.10")]
        public SpriteEntry[] Entries;

        private Dictionary<string, Sprite> _lookup;

        private void OnEnable() => BuildLookup();

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, Sprite>(Entries?.Length ?? 0);
            if (Entries == null) return;
            foreach (var e in Entries)
                if (!string.IsNullOrEmpty(e.Key) && e.Sprite != null)
                    _lookup[e.Key] = e.Sprite;
        }

        public Sprite GetSprite(string key)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(key, out Sprite sp);
            return sp;
        }
    }
}
