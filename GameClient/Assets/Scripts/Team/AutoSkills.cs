using UnityEngine;

namespace LegendsRPG.Team
{
    /// <summary>
    /// v1.1 AUTO toggle (Legends Reborn style). Movement and basic attacks are
    /// always automatic; while AUTO is ON every hero also fires its skills and
    /// ultimate automatically when ready. While OFF the player fires the
    /// ultimates manually by tapping the hero portraits in the HUD.
    /// Replaces the old AutoBattleController (which drove a manually-controlled
    /// active hero) — that control model was removed in v1.1.
    /// </summary>
    public class AutoSkills : MonoBehaviour
    {
        public static AutoSkills Instance { get; private set; }

        public bool Enabled { get; private set; }
        public event System.Action<bool> OnToggled;

        private void Awake()
        {
            Instance = this;
            Enabled = false;   // starts OFF: the player taps the ultimates
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetEnabled(bool value)
        {
            Enabled = value;
            if (OnToggled != null) OnToggled(value);
            Combat.CombatEvents.RaiseNotification("Auto Skills: " + (value ? "ON" : "OFF"));
            Audio.AudioSystem.Play("UI");
        }

        public void Toggle()
        {
            SetEnabled(!Enabled);
        }
    }
}
