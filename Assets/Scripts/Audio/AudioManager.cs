// ============================================================
// AudioManager.cs
// Versión adaptada para un solo ambient clip.
// Si solo hay un clip en el array, lo usa siempre sin cambiar.
// Cuando haya más clips, el sistema de mood switching funciona
// automáticamente sin tocar nada.
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using Booth.Core;

namespace Booth.Audio
{
    public class AudioManager : MonoBehaviour
    {
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
        [Tooltip("Con 1 clip: se usa siempre. Con 3 clips: cambia según mood (0=bajo, 1=medio, 2=alto).")]
        [SerializeField] private AudioClip[] _ambientByMood;

        [Header("Ambient Settings")]
        [SerializeField] private float _ambientFadeSpeed = 2f;
        [SerializeField][Range(0f, 1f)] private float _ambientVolume = 0.4f;

        private Dictionary<string, AudioClip> _clipMap;
        private float _targetAmbientVolume;
        private int _currentAmbientIndex = -1;

        private void Awake()
        {
            BuildClipMap();
            _targetAmbientVolume = _ambientVolume;
        }

        private void OnEnable()
        {
            GameEvents.OnAudioCueRequested += HandleAudioCue;
            GameEvents.OnMoodChanged += HandleMoodChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnAudioCueRequested -= HandleAudioCue;
            GameEvents.OnMoodChanged -= HandleMoodChanged;
        }

        private void Update()
        {
            if (_ambientSource != null)
                _ambientSource.volume = Mathf.MoveTowards(
                    _ambientSource.volume, _targetAmbientVolume,
                    Time.deltaTime * _ambientFadeSpeed);
        }

        private void HandleAudioCue(string cueName)
        {
            if (cueName == "crowd_ambient_start")
            {
                // Siempre arranca en el índice más alto disponible
                int startIndex = _ambientByMood != null ? _ambientByMood.Length - 1 : 0;
                PlayAmbient(startIndex);
                return;
            }

            if (_clipMap.TryGetValue(cueName, out AudioClip clip) && clip != null)
                _sfxSource.PlayOneShot(clip);
            else
                Debug.LogWarning($"[AudioManager] No clip for cue: '{cueName}'");
        }

        private void HandleMoodChanged(float normalized)
        {
            // Si solo hay 1 clip, no cambiar nunca
            if (_ambientByMood == null || _ambientByMood.Length <= 1) return;

            int targetIndex;
            if (normalized > 0.6f) targetIndex = Mathf.Min(2, _ambientByMood.Length - 1);
            else if (normalized > 0.3f) targetIndex = Mathf.Min(1, _ambientByMood.Length - 1);
            else targetIndex = 0;

            if (targetIndex != _currentAmbientIndex)
                PlayAmbient(targetIndex);
        }

        private void PlayAmbient(int index)
        {
            if (_ambientByMood == null || _ambientByMood.Length == 0) return;

            // Clampear al rango disponible — funciona con 1, 2 o 3 clips
            index = Mathf.Clamp(index, 0, _ambientByMood.Length - 1);

            if (_ambientByMood[index] == null) return;
            if (index == _currentAmbientIndex) return;

            _currentAmbientIndex = index;
            _ambientSource.clip = _ambientByMood[index];
            _ambientSource.loop = true;
            _ambientSource.volume = 0f;
            _targetAmbientVolume = _ambientVolume;
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
                { "crowd_distant",           null               },
            };
        }
    }
}

