// ============================================================
// TimerUI.cs
// Displays the session countdown timer. Changes visual urgency
// as time runs low to reinforce the emotional arc.
// ============================================================

using UnityEngine;
using TMPro;
using Booth.Core;

namespace Booth.UI
{
    public class TimerUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Text Reference")]
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Urgency Thresholds")]
        [SerializeField] private float _urgencyThreshold = 60f;  // last 60 seconds
        [SerializeField] private float _criticalThreshold = 30f; // last 30 seconds

        [Header("Colors")]
        [SerializeField] private Color _normalColor   = Color.white;
        [SerializeField] private Color _urgencyColor  = new Color(1f, 0.75f, 0f);
        [SerializeField] private Color _criticalColor = new Color(1f, 0.2f, 0.2f);

        // ── Unity lifecycle ───────────────────────────────────
        private void OnEnable()  => GameEvents.OnTimerTick += HandleTick;
        private void OnDisable() => GameEvents.OnTimerTick -= HandleTick;

        // ── Event handlers ────────────────────────────────────

        private void HandleTick(float remaining)
        {
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);

            _timerText.text = $"{minutes:D1}:{seconds:D2}";

            // Color urgency
            if (remaining <= _criticalThreshold)
                _timerText.color = _criticalColor;
            else if (remaining <= _urgencyThreshold)
                _timerText.color = _urgencyColor;
            else
                _timerText.color = _normalColor;
        }
    }
}
