using System;
using UnityEngine;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Offline squad progression: the tower keeps generating a capped reward
    /// while the player is away. Rewards are stored in the existing save data
    /// and claimed explicitly from the management UI, so combat state is untouched.
    /// </summary>
    public static class IdleProgression
    {
        public const int MaxOfflineSeconds = 8 * 60 * 60;
        public const int GoldPerMinute = 18;
        public const int ExpPerMinute = 5;

        public static int PendingGold { get { return SaveSystem.Current.idlePendingGold; } }
        public static int PendingExp { get { return SaveSystem.Current.idlePendingExp; } }
        public static int PendingSeconds { get { return SaveSystem.Current.idlePendingSeconds; } }
        public static bool HasPending { get { return PendingGold > 0 || PendingExp > 0; } }

        public static void Reconcile()
        {
            var save = SaveSystem.Current;
            DateTime now = DateTime.UtcNow;
            DateTime last;
            if (!DateTime.TryParse(save.idleLastUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out last))
            {
                save.idleLastUtc = now.ToString("o");
                return;
            }

            int elapsed = Mathf.Clamp(Mathf.RoundToInt((float)(now - last).TotalSeconds), 0, MaxOfflineSeconds);
            if (elapsed <= 0) return;

            int floor = 1;
            if (save.tower != null)
            {
                for (int i = 0; i < save.tower.Count; i++)
                    floor = Mathf.Max(floor, save.tower[i].bestFloor);
            }
            float multiplier = 1f + Mathf.Clamp(floor - 1, 0, 100) * 0.025f;
            save.idlePendingGold += Mathf.Max(1, Mathf.RoundToInt(elapsed / 60f * GoldPerMinute * multiplier));
            save.idlePendingExp += Mathf.Max(1, Mathf.RoundToInt(elapsed / 60f * ExpPerMinute * multiplier));
            save.idlePendingSeconds = Mathf.Clamp(save.idlePendingSeconds + elapsed, 0, MaxOfflineSeconds);
            save.idleLastUtc = now.ToString("o");
        }

        public static bool Claim(Team.TeamManager team)
        {
            if (!HasPending) return false;
            var save = SaveSystem.Current;
            int gold = save.idlePendingGold;
            int exp = save.idlePendingExp;
            save.idlePendingGold = 0;
            save.idlePendingExp = 0;
            save.idlePendingSeconds = 0;
            Inventory.AddGold(gold);
            if (team != null) team.ShareEXP(exp);
            return true;
        }

        public static void MarkSessionSaved()
        {
            SaveSystem.Current.idleLastUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
