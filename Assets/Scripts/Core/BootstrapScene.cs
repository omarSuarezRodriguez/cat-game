using UnityEngine;

namespace WhiskerHaven.Core
{
    /// <summary>
    /// Drag this onto a GameObject in the Bootstrap scene.
    /// It pre-warms all singleton instances so they exist before GameManager.Start().
    /// This avoids race conditions in Awake order.
    /// </summary>
    public class BootstrapScene : MonoBehaviour
    {
        private void Awake()
        {
            // Touch all singletons to ensure they're created in the right order
            _ = SaveSystem.Instance;
            _ = ResourceManager.Instance;
            _ = AudioManager.Instance;

            // Gameplay
            _ = CatManager.Instance;
            _ = HabitatManager.Instance;
            _ = VolunteerSystem.Instance;
            _ = PurrPowerSystem.Instance;
            _ = MissionSystem.Instance;
            _ = AchievementSystem.Instance;
            _ = IdleManager.Instance;
        }
    }
}
