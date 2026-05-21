// ============================================================
// UIManager.cs
// Controls which UI screens are active at any time.
// Holds references to all UI panels and shows/hides them
// based on instructions from the GameStateMachine.
//
// This is a coordinator, not a logic system.
// Each panel handles its own internal logic.
// ============================================================

using UnityEngine;
using Booth.Core;

namespace Booth.UI
{
    public class UIManager : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Screens")]
        [SerializeField] private GameObject _introScreen;
        [SerializeField] private GameObject _gameHUD;
        [SerializeField] private GameObject _victoryScreen;
        [SerializeField] private GameObject _defeatScreen;

        [Header("End Screen References")]
        [SerializeField] private EndScreenUI _victoryScreenUI;
        [SerializeField] private EndScreenUI _defeatScreenUI;

        // ── Public API (called by GameStateMachine states) ────

        public void ShowIntroScreen()
        {
            HideAll();
            _introScreen.SetActive(true);
        }

        public void HideIntroScreen()
        {
            _introScreen.SetActive(false);
        }

        public void ShowGameHUD()
        {
            HideAll();
            _gameHUD.SetActive(true);
        }

        public void ShowVictoryScreen()
        {
            HideAll();
            _victoryScreen.SetActive(true);
            _victoryScreenUI?.Show(isVictory: true);
        }

        public void ShowDefeatScreen()
        {
            HideAll();
            _defeatScreen.SetActive(true);
            _defeatScreenUI?.Show(isVictory: false);
        }

        public void HideEndScreen()
        {
            _victoryScreen.SetActive(false);
            _defeatScreen.SetActive(false);
        }

        // ── Private ───────────────────────────────────────────

        private void HideAll()
        {
            _introScreen?.SetActive(false);
            _gameHUD?.SetActive(false);
            _victoryScreen?.SetActive(false);
            _defeatScreen?.SetActive(false);
        }
    }
}