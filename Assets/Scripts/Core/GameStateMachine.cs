// ============================================================
// GameStateMachine.cs
// Controls top-level game flow: Intro → Playing → Victory/Defeat
// Uses a clean state pattern with Enter/Exit/Update per state.
// Only one MonoBehaviour owns the machine — no singletons needed.
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using Booth.Core;
using Booth.UI;
using Booth.Customers;
using Booth.Mood;
using Booth.Timer;
using Booth.Difficulty;
using Booth.Audio;

namespace Booth.Core
{
    // ── State interface ───────────────────────────────────────
    public interface IGameState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }

    // ============================================================
    // GameStateMachine  (attach to a "GameManager" GameObject)
    // ============================================================
    public class GameStateMachine : MonoBehaviour
    {
        // ── Serialized deps ───────────────────────────────────
        [Header("System References")]
        [SerializeField] private CustomerManager  _customerManager;
        [SerializeField] private MoodSystem        _moodSystem;
        [SerializeField] private SessionTimer      _sessionTimer;
        [SerializeField] private UIManager         _uiManager;
        [SerializeField] private DifficultyManager _difficultyManager;
        [SerializeField] private AudioManager      _audioManager;

        [Header("Intro Settings")]
        [SerializeField] private float _introDuration = 3f;

        // ── State instances ───────────────────────────────────
        private IGameState _currentState;

        private IntroState   _introState;
        private PlayingState _playingState;
        private VictoryState _victoryState;
        private DefeatState  _defeatState;

        // ── Unity lifecycle ───────────────────────────────────
        private void Awake()
        {
            // Build states and inject only what they need
            _introState   = new IntroState(this, _uiManager, _introDuration);
            _playingState = new PlayingState(this, _customerManager, _moodSystem,
                                             _sessionTimer, _difficultyManager, _uiManager);
            _victoryState = new VictoryState(this, _uiManager, _audioManager);
            _defeatState  = new DefeatState(this, _uiManager, _audioManager);
        }

        private void Start()
        {
            // Subscribe to lose/win conditions raised by other systems
            GameEvents.OnTimerExpired += HandleTimerExpired;
            GameEvents.OnMoodChanged  += HandleMoodChanged;

            TransitionTo(_introState);
        }

        private void OnDestroy()
        {
            GameEvents.OnTimerExpired -= HandleTimerExpired;
            GameEvents.OnMoodChanged  -= HandleMoodChanged;
        }

        private void Update()
        {
            _currentState?.Tick(Time.deltaTime);
        }

        // ── Transition API (states call this) ─────────────────
        public void TransitionTo(IGameState next)
        {
            _currentState?.Exit();
            _currentState = next;
            _currentState.Enter();
        }

        // Public accessors so states can navigate
        public PlayingState PlayingState => _playingState;
        public VictoryState VictoryState => _victoryState;
        public DefeatState  DefeatState  => _defeatState;

        // ── Event handlers ────────────────────────────────────
        private void HandleTimerExpired()
        {
            if (_currentState == _playingState)
                TransitionTo(_victoryState);
        }

        private void HandleMoodChanged(float normalized)
        {
            if (_currentState == _playingState && normalized <= 0f)
                TransitionTo(_defeatState);
        }
    }

    // ============================================================
    // INTRO STATE — short orientation pause before play begins
    // ============================================================
    public class IntroState : IGameState
    {
        private readonly GameStateMachine _machine;
        private readonly UIManager        _ui;
        private readonly float            _duration;
        private float _elapsed;

        public IntroState(GameStateMachine machine, UIManager ui, float duration)
        {
            _machine  = machine;
            _ui       = ui;
            _duration = duration;
        }

        public void Enter()
        {
            _elapsed = 0f;
            _ui.ShowIntroScreen();
            GameEvents.TriggerAudioCue("crowd_distant");
        }

        public void Tick(float dt)
        {
            _elapsed += dt;
            if (_elapsed >= _duration)
                _machine.TransitionTo(_machine.PlayingState);
        }

        public void Exit() => _ui.HideIntroScreen();
    }

    // ============================================================
    // PLAYING STATE — the core game loop lives here
    // ============================================================
    public class PlayingState : IGameState
    {
        private readonly GameStateMachine  _machine;
        private readonly CustomerManager   _customers;
        private readonly MoodSystem        _mood;
        private readonly SessionTimer      _timer;
        private readonly DifficultyManager _difficulty;
        private readonly UIManager         _ui;

        public PlayingState(GameStateMachine machine, CustomerManager customers,
                            MoodSystem mood, SessionTimer timer,
                            DifficultyManager difficulty, UIManager ui)
        {
            _machine    = machine;
            _customers  = customers;
            _mood       = mood;
            _timer      = timer;
            _difficulty = difficulty;
            _ui         = ui;
        }

        public void Enter()
        {
            _ui.ShowGameHUD();
            _timer.StartTimer();
            _mood.Initialize();
            _difficulty.Initialize();
            _customers.SpawnNextCustomer();
            GameEvents.TriggerGameStarted();
            GameEvents.TriggerAudioCue("crowd_ambient_start");
        }

        public void Tick(float dt) { /* Systems update themselves via events */ }

        public void Exit()
        {
            _timer.StopTimer();
            _customers.ClearCurrentCustomer();
        }
    }

    // ============================================================
    // VICTORY STATE
    // ============================================================
    public class VictoryState : IGameState
    {
        private readonly GameStateMachine _machine;
        private readonly UIManager        _ui;
        private readonly AudioManager     _audio;

        public VictoryState(GameStateMachine machine, UIManager ui, AudioManager audio)
        {
            _machine = machine; _ui = ui; _audio = audio;
        }

        public void Enter()
        {
            _ui.ShowVictoryScreen();
            GameEvents.TriggerGameVictory();
            GameEvents.TriggerAudioCue("crowd_cheer");
        }

        public void Tick(float dt) { }
        public void Exit() => _ui.HideEndScreen();
    }

    // ============================================================
    // DEFEAT STATE
    // ============================================================
    public class DefeatState : IGameState
    {
        private readonly GameStateMachine _machine;
        private readonly UIManager        _ui;
        private readonly AudioManager     _audio;

        public DefeatState(GameStateMachine machine, UIManager ui, AudioManager audio)
        {
            _machine = machine; _ui = ui; _audio = audio;
        }

        public void Enter()
        {
            _ui.ShowDefeatScreen();
            GameEvents.TriggerGameDefeat();
            GameEvents.TriggerAudioCue("crowd_riot");
        }

        public void Tick(float dt) { }
        public void Exit() => _ui.HideEndScreen();
    }
}
