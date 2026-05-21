// ============================================================
// CustomerVisualConfig.cs
// ScriptableObject that maps sprite key strings to actual
// Unity Sprites. Decouples code from direct asset references.
// Create via: Assets > Create > Booth > Customer Visual Config
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace Booth.Customers
{
    [CreateAssetMenu(fileName = "CustomerVisualConfig", menuName = "Booth/Customer Visual Config")]
    public class CustomerVisualConfig : ScriptableObject
    {
        [System.Serializable]
        public struct SpriteEntry
        {
            public string Key;
            public Sprite Sprite;
        }

        [Tooltip("Map sprite key strings to Unity Sprites. " +
                 "Keys must match those in CustomerConfig.CustomerSpriteKeys.")]
        public SpriteEntry[] Entries;

        // Cached dictionary for O(1) lookup
        private Dictionary<string, Sprite> _lookup;

        private void OnEnable() => BuildLookup();

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, Sprite>(Entries?.Length ?? 0);
            if (Entries == null) return;

            foreach (var entry in Entries)
            {
                if (!string.IsNullOrEmpty(entry.Key) && entry.Sprite != null)
                    _lookup[entry.Key] = entry.Sprite;
            }
        }

        /// <summary>Returns the sprite for a given key, or null if not found.</summary>
        public Sprite GetSprite(string key)
        {
            if (_lookup == null) BuildLookup();

            _lookup.TryGetValue(key, out Sprite sp);
            return sp;
        }
    }
}
