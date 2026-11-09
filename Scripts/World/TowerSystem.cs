using System;
using UnityEngine;
using LegendsRPG.Core;

namespace LegendsRPG.World
{
    /// <summary>
    /// Experience Tower: 4 difficulties x 200 floors.
    /// Difficulty is data-driven (multiplier tables in GameConfig) and floor
    /// scaling is a formula — adding floors or difficulties never requires
    /// duplicated hand-authored content.
    /// </summary>
    public class TowerSystem : MonoBehaviour
    {
        public enum TowerDifficulty { Easy = 0, Hard = 1, Hell = 2, Nightmare = 3 }

        public TowerDifficulty Difficulty { get; private set; } = TowerDifficulty.Easy;
        public int CurrentFloor { get; private set; } = 1;

        public static float CurrentHpScale { get; private set; } = 1f;
        public static float CurrentAtkScale { get; private set; } = 1f;
        public static float CurrentExpScale { get; private set; } = 1f;

        public event Action<int, TowerDifficulty> FloorStarted;

        public void RestoreFromSave()
        {
            int diff = Mathf.Clamp(SaveSystem.Current.activeDifficulty, 0, 3);
            Difficulty = (TowerDifficulty)diff;

            int best = 1;
            foreach (var p in SaveSystem.Current.tower)
            {
                if (p.difficulty == diff) { best = Mathf.Max(1, p.bestFloor); break; }
            }
            // resume at the best floor reached (the floor itself is re-fought)
            SetFloor(best, false);
        }

        public void SetDifficulty(TowerDifficulty d, bool notify = true)
        {
            Difficulty = d;
            SetFloor(1, notify);
        }

        public void SetFloor(int floor, bool notify = true)
        {
            CurrentFloor = Mathf.Clamp(floor, 1, GameConfig.FloorsPerDifficulty);
            RecomputeScales();

            if (notify && FloorStarted != null) FloorStarted(CurrentFloor, Difficulty);
        }

        public void AdvanceFloor()
        {
            if (CurrentFloor >= GameConfig.FloorsPerDifficulty)
            {
                Combat.CombatEvents.RaiseNotification(
                    "Tower completed on this difficulty! Switch difficulty from the top bar.");
                return;
            }
            SetFloor(CurrentFloor + 1, false);
        }

        private void RecomputeScales()
        {
            int f = CurrentFloor - 1;
            int d = (int)Difficulty;
            CurrentHpScale = (1f + GameConfig.TowerHpGrowthPerFloor * f) * GameConfig.DifficultyHpMult[d];
            CurrentAtkScale = (1f + GameConfig.TowerAtkGrowthPerFloor * f) * GameConfig.DifficultyAtkMult[d];
            CurrentExpScale = GameConfig.DifficultyExpMult[d] * (1f + 0.01f * f);
        }

        public static string DifficultyName(TowerDifficulty d)
        {
            switch (d)
            {
                case TowerDifficulty.Easy: return "Easy";
                case TowerDifficulty.Hard: return "Hard";
                case TowerDifficulty.Hell: return "Hell";
                case TowerDifficulty.Nightmare: return "Nightmare";
                default: return "Easy";
            }
        }
    }
}
