// ============================================================
// AudioManager.cs
// Handles all audio feedback. Listens to OnAudioCueRequested
// and plays the appropriate clip. No other system touches audio.
//
// Ambient crowd sound evolves with mood — this reinforces the
// GDD's "diegetic only" design: the crowd IS the soundtrack.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using Booth.Core;

namespace Booth.Audio
{
    public class AudioManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _ambientSource;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip _acceptCorrectClip;
        [SerializeField] private AudioClip _acceptWrongClip;
        [SerializeField] private AudioClip _rejectCorrectClip;
        [SerializeField] private AudioClip _rejectWrongClip;
        [SerializeField] private AudioClip _askMoreClip;
        [SerializeField] private AudioClip _crowdGroanClip;
        [SerializeField] private AudioClip _crowdCheerClip;
        [SerializeField] private AudioClip _crowdRiotClip;
        [SerializeField] private AudioClip _buttonClickClip;

        [Header("Ambient Clips (by mood level)")]
        [Tooltip("Ambient crowd tracks for different mood ranges. " +
                 "Index 0=low mood (tense), Index 1=mid, Index 2=high mood (excited)")]
        [SerializeField] private AudioClip[] _ambientByMood;

        [Header("Ambient Settings")]
        [SerializeField] private float _ambientFadeSpeed = 2f;
        [SerializeField] [Range(0f, 1f)] private float _ambientVolume = 0.4f;

        // ── State ─────────────────────────────────────────────
        private Dictionary<string, AudioClip> _clipMap;
        private float _targetAmbientVolume;
        private int   _currentAmbientIndex = -1;

        // ── Unity lifecycle ───────────────────────────────────
        private void Awake()
        {
            BuildClipMap();
            _targetAmbientVolume = _ambientVolume;
        }

        private void OnEnable()
        {
            GameEvents.OnAudioCueRequested += HandleAudioCue;
            GameEvents.OnMoodChanged       += HandleMoodChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnAudioCueRequested -= HandleAudioCue;
            GameEvents.OnMoodChanged       -= HandleMoodChanged;
        }

        private void Update()
        {
            // Smooth ambient volume transitions
            if (_ambientSource != null)
                _ambientSource.volume = Mathf.MoveTowards(
                    _ambientSource.volume, _targetAmbientVolume,
                    Time.deltaTime * _ambientFadeSpeed);
        }

        // ── Event handlers ────────────────────────────────────

        private void HandleAudioCue(string cueName)
        {
            // Special: start ambient
            if (cueName == "crowd_ambient_start")
            {
                PlayAmbient(2); // start at excited/high mood
                return;
            }

            if (_clipMap.TryGetValue(cueName, out AudioClip clip) && clip != null)
            {
                _sfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] No clip for cue: '{cueName}'");
            }
        }

        private void HandleMoodChanged(float normalized)
        {
            // Switch ambient track based on mood level
            int targetIndex;
            if      (normalized > 0.6f) targetIndex = 2; // high — excited crowd
            else if (normalized > 0.3f) targetIndex = 1; // mid — restless
            else                        targetIndex = 0; // low — tense/angry

            if (targetIndex != _currentAmbientIndex)
                PlayAmbient(targetIndex);
        }

        // ── Private helpers ───────────────────────────────────

        private void PlayAmbient(int index)
        {
            if (_ambientByMood == null || index >= _ambientByMood.Length) return;
            if (_ambientByMood[index] == null) return;
            if (index == _currentAmbientIndex) return;

            _currentAmbientIndex       = index;
            _ambientSource.clip        = _ambientByMood[index];
            _ambientSource.loop        = true;
            _ambientSource.volume      = 0f;
            _targetAmbientVolume       = _ambientVolume;
            _ambientSource.Play();
        }

        private void BuildClipMap()
        {
            _clipMap = new Dictionary<string, AudioClip>
            {
                { "decision_accept_correct", _acceptCorrectClip },
                { "decision_accept_wrong",   _acceptWrongClip   },
                { "decision_reject_correct", _rejectCorrectClip },
                { "decision_reject_wrong",   _rejectWrongClip   },
                { "decision_ask_more",       _askMoreClip       },
                { "crowd_groan",             _crowdGroanClip    },
                { "crowd_cheer",             _crowdCheerClip    },
                { "crowd_riot",              _crowdRiotClip     },
                { "crowd_distant",           null               }, // ambient — handled separately
            };
        }
    }
}
