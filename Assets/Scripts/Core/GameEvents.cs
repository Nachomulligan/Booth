// ============================================================
// GameEvents.cs
// Central static event bus. All systems communicate through
// these events — no direct references between managers.
// Philosophy: fire-and-forget, listener-based, zero coupling.
// ============================================================

using Booth.Customers;
using System;
using UnityEngine;

namespace Booth.Core
{
    /// <summary>
    /// Static event bus. Systems subscribe/unsubscribe freely.
    /// No MonoBehaviour needed — pure C# events.
    /// </summary>
    public static class GameEvents
    {
        // ── Game State ────────────────────────────────────────
        /// <summary>Fired when the game session officially begins.</summary>
        public static event Action OnGameStarted;

        /// <summary>Fired when the player wins (timer ends, mood > 0).</summary>
        public static event Action OnGameVictory;

        /// <summary>Fired when the player loses (mood reaches zero).</summary>
        public static event Action OnGameDefeat;

        /// <summary>Fired on any game-over (pass result string: "victory"/"defeat").</summary>
        public static event Action<string> OnGameOver;

        // ── Customer ──────────────────────────────────────────
        /// <summary>Fired when a new customer arrives at the window.</summary>
        public static event Action<CustomerData> OnCustomerArrived;

        /// <summary>Fired when the current customer leaves (accepted or rejected).</summary>
        public static event Action<CustomerData, DecisionResult> OnCustomerDeparted;

        // ── Decision ──────────────────────────────────────────
        /// <summary>Player pressed ACCEPT.</summary>
        public static event Action OnDecisionAccept;

        /// <summary>Player pressed REJECT.</summary>
        public static event Action OnDecisionReject;

        /// <summary>Player pressed ASK FOR MORE.</summary>
        public static event Action OnDecisionAskMore;

        /// <summary>
        /// Fired after a decision is fully evaluated.
        /// bool isCorrect: was the decision right?
        /// </summary>
        public static event Action<DecisionResult> OnDecisionResolved;

        // ── Mood ──────────────────────────────────────────────
        /// <summary>Fired whenever mood value changes. float = new 0-1 normalized value.</summary>
        public static event Action<float> OnMoodChanged;

        /// <summary>Fired when the 5-second idle delay starts draining mood.</summary>
        public static event Action OnMoodDrainStarted;

        /// <summary>Fired when the player acts and drain stops.</summary>
        public static event Action OnMoodDrainStopped;

        // ── Timer ─────────────────────────────────────────────
        /// <summary>Fired every second. float = remaining seconds.</summary>
        public static event Action<float> OnTimerTick;

        /// <summary>Fired when the 5-minute session timer expires.</summary>
        public static event Action OnTimerExpired;

        // ── Difficulty ────────────────────────────────────────
        /// <summary>Fired when difficulty tier increases. int = new tier (0,1,2,3).</summary>
        public static event Action<int> OnDifficultyChanged;

        // ── Audio Cues ────────────────────────────────────────
        /// <summary>Request the audio system to play a named cue.</summary>
        public static event Action<string> OnAudioCueRequested;

        // ── Invocation helpers (called by owning systems) ─────

        public static void TriggerGameStarted()               => OnGameStarted?.Invoke();
        public static void TriggerGameVictory()               { OnGameVictory?.Invoke(); OnGameOver?.Invoke("victory"); }
        public static void TriggerGameDefeat()                { OnGameDefeat?.Invoke();  OnGameOver?.Invoke("defeat");  }

        public static void TriggerCustomerArrived(CustomerData d)                        => OnCustomerArrived?.Invoke(d);
        public static void TriggerCustomerDeparted(CustomerData d, DecisionResult r)     => OnCustomerDeparted?.Invoke(d, r);

        public static void TriggerDecisionAccept()            => OnDecisionAccept?.Invoke();
        public static void TriggerDecisionReject()            => OnDecisionReject?.Invoke();
        public static void TriggerDecisionAskMore()           => OnDecisionAskMore?.Invoke();
        public static void TriggerDecisionResolved(DecisionResult r) => OnDecisionResolved?.Invoke(r);

        public static void TriggerMoodChanged(float normalized) => OnMoodChanged?.Invoke(normalized);
        public static void TriggerMoodDrainStarted()          => OnMoodDrainStarted?.Invoke();
        public static void TriggerMoodDrainStopped()          => OnMoodDrainStopped?.Invoke();

        public static void TriggerTimerTick(float remaining)  => OnTimerTick?.Invoke(remaining);
        public static void TriggerTimerExpired()              => OnTimerExpired?.Invoke();

        public static void TriggerDifficultyChanged(int tier) => OnDifficultyChanged?.Invoke(tier);
        public static void TriggerAudioCue(string cueName)    => OnAudioCueRequested?.Invoke(cueName);
    }
}
