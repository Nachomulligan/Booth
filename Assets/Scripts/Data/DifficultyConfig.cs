// ============================================================
// DifficultyConfig.cs
// ScriptableObject for difficulty progression settings.
// Create via: Assets > Create > Booth > Difficulty Config
// ============================================================

using UnityEngine;

namespace Booth.Difficulty
{
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "Booth/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Session")]
        [Tooltip("Must match TimerConfig.SessionDuration.")]
        public float SessionDuration = 300f;

        [Header("Tier Thresholds (seconds elapsed)")]
        [Tooltip("Time elapsed in seconds to enter each tier. Index = tier number.\n" +
                 "Tier 0: 0s (start)\n" +
                 "Tier 1: 60s (1 min)\n" +
                 "Tier 2: 180s (3 min)\n" +
                 "Tier 3: 240s (4 min)")]
        public float[] TierThresholds = { 0f, 60f, 180f, 240f };
    }
}
