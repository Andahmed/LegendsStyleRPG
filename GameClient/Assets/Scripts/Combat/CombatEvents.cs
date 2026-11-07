using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegendsRPG.Combat
{
    public enum ElementType
    {
        None = 0,
        Fire = 1,
        Water = 2,
        Ice = 3,
        Lightning = 4,
        Nature = 5,
        Dark = 6,
        Light = 7
    }

    /// <summary>Data passed with every damage event (used by popups, stats, camera shake).</summary>
    public class DamageInfo
    {
        public GameObject target;
        public GameObject source;
        public float amount;
        public ElementType element;
        public bool isCrit;
        public bool isSkill;
        public bool killed;
    }

    /// <summary>Global static event bus — keeps combat systems decoupled from UI/camera/audio.</summary>
    public static class CombatEvents
    {
        public static event Action<DamageInfo> DamageDealt;
        public static event Action<GameObject> CharacterDied;         // dead GameObject
        public static event Action<GameObject> CharacterRespawned;
        public static event Action<int> HeroLevelUp;                  // hero slot
        public static event Action<string> Notification;              // HUD messages

        public static void RaiseDamage(DamageInfo info) { DamageDealt?.Invoke(info); }
        public static void RaiseDeath(GameObject go) { CharacterDied?.Invoke(go); }
        public static void RaiseRespawn(GameObject go) { CharacterRespawned?.Invoke(go); }
        public static void RaiseLevelUp(int slot) { HeroLevelUp?.Invoke(slot); }
        public static void RaiseNotification(string msg)
        {
            Notification?.Invoke(msg);
            Debug.Log("[LegendsRPG] " + msg);
        }
    }
}
