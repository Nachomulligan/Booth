// ============================================================
// DifficultyManager.cs
// Monitors session time elapsed and upgrades difficulty tiers.
// Fires OnDifficultyChanged so CustomerManager adjusts spawn rates
// and CustomerFactory adjusts generation probabilities.
//
// Emotional curve from GDD:
//   0-1 min  → calm orientation     (Tier 0)
//   1-3 min  → growing urgency      (Tier 1)
//   3-4 min  → maximum stress       (Tier 2)
//   4-5 min  → collapse or glory    (Tier 3)
// ============================================================

using UnityEngine;
using Booth.Core;

namespace Booth.Difficulty
{
    public class DifficultyManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Config")]
        [SerializeField] private DifficultyConfig _config;

        // ── State ─────────────────────────────────────────────
        private int   _currentTier;
        private float _sessionDuration; // cached from TimerConfig

        // ── Unity lifecycle ───────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnTimerTick += HandleTimerTick;
        }

        private void OnDisable()
        {
            GameEvents.OnTimerTick -= HandleTimerTick;
        }

        // ── Public API ────────────────────────────────────────

        public void Initialize()
        {
            _currentTier    = 0;
            _sessionDuration = _config.SessionDuration;
            GameEvents.TriggerDifficultyChanged(_currentTier);
        }

        // ── Private ───────────────────────────────────────────

        private void HandleTimerTick(float remaining)
        {
            float elapsed      = _sessionDuration - remaining;
            int   expectedTier = GetTierForElapsed(elapsed);

            if (expectedTier != _currentTier)
            {
                _currentTier = expectedTier;
                Debug.Log($"[DifficultyManager] Tier upgraded to {_currentTier} at {elapsed:F0}s elapsed.");
                GameEvents.TriggerDifficultyChanged(_currentTier);
                GameEvents.TriggerAudioCue($"difficulty_tier_{_currentTier}");
            }
        }

        private int GetTierForElapsed(float elapsed)
        {
            // Walk the tier thresholds in reverse to find the highest applicable
            for (int i = _config.TierThresholds.Length - 1; i >= 0; i--)
            {
                if (elapsed >= _config.TierThresholds[i])
                    return i;
            }
            return 0;
        }
    }
}
