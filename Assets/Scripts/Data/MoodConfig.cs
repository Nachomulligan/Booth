// ============================================================
// MoodConfig.cs
// ScriptableObject: all tunable values for the mood system.
// Tweak during playtesting without recompiling.
// Create via: Assets > Create > Booth > Mood Config
// ============================================================

using UnityEngine;

namespace Booth.Mood
{
    [CreateAssetMenu(fileName = "MoodConfig", menuName = "Booth/Mood Config")]
    public class MoodConfig : ScriptableObject
    {
        [Header("Mood Range")]
        [Tooltip("Maximum mood value (full bar).")]
        public float MaxMood = 100f;

        [Header("Idle Drain")]
        [Tooltip("Seconds before idle drain begins. GDD specifies 5 seconds.")]
        public float IdleGracePeriod = 5f;

        [Tooltip("Mood drained per second during idle. Gradual — less severe than wrong decisions.")]
        public float IdleDrainPerSecond = 3f;

        [Header("Wrong Decision Penalties")]
        [Tooltip("Mood lost when a minor is accepted. Severe.")]
        public float PenaltyAcceptMinor = 25f;

        [Tooltip("Mood lost when fake bills are accepted. Severe.")]
        public float PenaltyAcceptFake = 20f;

        [Tooltip("Mood lost when someone without enough money is accepted.")]
        public float PenaltyAcceptInsufficient = 15f;

        [Tooltip("Mood lost when a valid customer is rejected. Severe — boos from the crowd.")]
        public float PenaltyRejectValid = 20f;

        [Tooltip("Mood lost when Ask For More is used unnecessarily.")]
        public float PenaltyAskUnnecessary = 8f;

        [Tooltip("Default penalty for edge cases.")]
        public float PenaltyDefault = 10f;

        // ── Validation ────────────────────────────────────────
        private void OnValidate()
        {
            IdleGracePeriod    = Mathf.Max(1f,  IdleGracePeriod);
            IdleDrainPerSecond = Mathf.Max(0f,  IdleDrainPerSecond);
            MaxMood            = Mathf.Max(10f, MaxMood);
        }
    }
}
