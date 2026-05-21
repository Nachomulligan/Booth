// ============================================================
// TimerConfig.cs
// ScriptableObject for session timer settings.
// Create via: Assets > Create > Booth > Timer Config
// ============================================================

using UnityEngine;

namespace Booth.Timer
{
    [CreateAssetMenu(fileName = "TimerConfig", menuName = "Booth/Timer Config")]
    public class TimerConfig : ScriptableObject
    {
        [Header("Session Duration")]
        [Tooltip("Total game session in seconds. GDD: 5 minutes = 300 seconds.")]
        public float SessionDuration = 300f;
    }
}
