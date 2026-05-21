// ============================================================
// SessionTimer.cs
// Manages the 5-minute session countdown.
// Fires OnTimerTick every second and OnTimerExpired at zero.
// Difficulty progression is driven by time elapsed.
// GameStateMachine listens to OnTimerExpired for victory.
// ============================================================

using System.Collections;
using UnityEngine;
using Booth.Core;

namespace Booth.Timer
{
    public class SessionTimer : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Timer Config")]
        [SerializeField] private TimerConfig _config;

        // ── State ─────────────────────────────────────────────
        private float   _remainingSeconds;
        private bool    _isRunning;
        private Coroutine _tickCoroutine;

        // ── Public ────────────────────────────────────────────
        public float RemainingSeconds  => _remainingSeconds;
        public float ElapsedSeconds    => _config.SessionDuration - _remainingSeconds;
        public float NormalizedElapsed => ElapsedSeconds / _config.SessionDuration;

        // ── Public API ────────────────────────────────────────

        public void StartTimer()
        {
            _remainingSeconds = _config.SessionDuration;
            _isRunning        = true;

            if (_tickCoroutine != null) StopCoroutine(_tickCoroutine);
            _tickCoroutine = StartCoroutine(TickCoroutine());
        }

        public void StopTimer()
        {
            _isRunning = false;
            if (_tickCoroutine != null)
            {
                StopCoroutine(_tickCoroutine);
                _tickCoroutine = null;
            }
        }

        // ── Coroutine ─────────────────────────────────────────

        private IEnumerator TickCoroutine()
        {
            while (_isRunning && _remainingSeconds > 0f)
            {
                yield return new WaitForSeconds(1f);

                _remainingSeconds -= 1f;
                _remainingSeconds  = Mathf.Max(0f, _remainingSeconds);

                GameEvents.TriggerTimerTick(_remainingSeconds);

                if (_remainingSeconds <= 0f)
                {
                    _isRunning = false;
                    GameEvents.TriggerTimerExpired();
                    Debug.Log("[SessionTimer] Session expired → Victory condition.");
                }
            }
        }
    }
}
