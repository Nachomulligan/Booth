// ============================================================
// MoodBarUI.cs
// Displays the crowd mood bar. Animates smoothly.
// Reacts to drain state changes with visual feedback.
//
// The bar represents collective disappointment, not the
// player's health — this distinction should feel in the UI.
// ============================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Booth.Core;

namespace Booth.UI
{
    public class MoodBarUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Bar References")]
        [SerializeField] private Slider          _moodSlider;
        [SerializeField] private Image           _fillImage;
        [SerializeField] private TextMeshProUGUI _labelText;

        [Header("Colors (by mood level)")]
        [SerializeField] private Color _highMoodColor   = new Color(0.2f, 0.8f, 0.3f);
        [SerializeField] private Color _medMoodColor    = new Color(1.0f, 0.75f, 0.1f);
        [SerializeField] private Color _lowMoodColor    = new Color(0.9f, 0.2f, 0.2f);

        [Header("Drain Feedback")]
        [Tooltip("Pulsing effect when idle drain is active.")]
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseAmount = 0.15f;

        [Header("Smoothing")]
        [SerializeField] private float _smoothSpeed = 5f;

        // ── State ─────────────────────────────────────────────
        private float   _targetValue;
        private float   _displayValue;
        private bool    _isDraining;
        private Coroutine _pulseCoroutine;

        // ── Unity lifecycle ───────────────────────────────────
        private void OnEnable()
        {
            GameEvents.OnMoodChanged      += HandleMoodChanged;
            GameEvents.OnMoodDrainStarted += HandleDrainStarted;
            GameEvents.OnMoodDrainStopped += HandleDrainStopped;
        }

        private void OnDisable()
        {
            GameEvents.OnMoodChanged      -= HandleMoodChanged;
            GameEvents.OnMoodDrainStarted -= HandleDrainStarted;
            GameEvents.OnMoodDrainStopped -= HandleDrainStopped;
        }

        private void Update()
        {
            // Smooth the bar toward the target
            _displayValue = Mathf.Lerp(_displayValue, _targetValue, Time.deltaTime * _smoothSpeed);
            _moodSlider.value = _displayValue;

            // Update color based on level
            _fillImage.color = GetMoodColor(_displayValue);
        }

        // ── Event handlers ────────────────────────────────────

        private void HandleMoodChanged(float normalized)
        {
            _targetValue = normalized;
        }

        private void HandleDrainStarted()
        {
            _isDraining = true;
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = StartCoroutine(PulseEffect());
        }

        private void HandleDrainStopped()
        {
            _isDraining = false;
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
            // Reset to normal alpha
            Color c = _fillImage.color;
            c.a = 1f;
            _fillImage.color = c;
        }

        // ── Visual helpers ────────────────────────────────────

        private Color GetMoodColor(float normalized)
        {
            if (normalized > 0.5f)
                return Color.Lerp(_medMoodColor, _highMoodColor, (normalized - 0.5f) * 2f);
            else
                return Color.Lerp(_lowMoodColor, _medMoodColor, normalized * 2f);
        }

        private IEnumerator PulseEffect()
        {
            float elapsed = 0f;
            while (_isDraining)
            {
                elapsed += Time.deltaTime * _pulseSpeed;
                float alpha = 1f - (Mathf.Sin(elapsed) * 0.5f + 0.5f) * _pulseAmount;

                Color c = _fillImage.color;
                c.a = alpha;
                _fillImage.color = c;

                yield return null;
            }
        }
    }
}
