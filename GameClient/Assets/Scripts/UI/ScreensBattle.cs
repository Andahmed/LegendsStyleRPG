using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Core;
using LegendsRPG.World;

namespace LegendsRPG.UI
{
    // ====================================================================
    // CAMPAIGN — chapter selector + 15-stage grid with stars & locks.
    // ====================================================================
    public class CampaignScreen : ScreenBase
    {
        private int _chapter = 1;

        public override void Rebuild()
        {
            ClearContent();
            _chapter = Mathf.Clamp(_chapter, 1, CampaignSystem.Chapters);

            // chapter switcher
            var nav = UIX.CreateRow(Content, "ChNav", 110f, 14, 14, 10, 10);
            UIX.CreateRowButton(nav, "<", () => { _chapter--; Rebuild(); }, new Color(0.3f, 0.35f, 0.5f), 90f, 110f, 44);
            var story = CampaignStory.GetChapter(_chapter);
            UIX.AddChildText(nav, "ChName",
                "ACT " + _chapter + " — " + story.title + "\n" + story.region,
                34, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);
            UIX.CreateRowButton(nav, ">", () => { _chapter++; Rebuild(); }, new Color(0.3f, 0.35f, 0.5f), 90f, 110f, 44);

            var progress = UIX.CreateRow(Content, "Progress", 90f);
            UIX.AddChildText(progress, "P",
                "Total stars: " + CampaignSystem.TotalStars() + " / " + (CampaignSystem.Chapters * CampaignSystem.StagesPerChapter * 3) +
                "   •   Stages cleared: " + CampaignSystem.ClearedStageCount(),
                28, new Color(0.7f, 0.85f, 1f), TextAnchor.MiddleCenter, true);

            var storyCard = UIX.CreateRow(Content, "StoryBrief", 210f, 18, 18, 12, 12);
            storyCard.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.24f, 0.98f);
            UIX.AddChildText(storyCard, "Story",
                story.synopsis + "\n\n" +
                "BOSS: " + story.bossName + "\n" +
                "OPENING: " + story.opening,
                25, new Color(0.88f, 0.9f, 0.96f), TextAnchor.MiddleLeft, true);

            int nextStage = 1;
            while (nextStage <= CampaignSystem.StagesPerChapter &&
                   CampaignSystem.StarsOf(_chapter, nextStage) > 0) nextStage++;
            string nextBrief = nextStage <= CampaignSystem.StagesPerChapter
                ? CampaignStory.GetStageBrief(_chapter, nextStage)
                : story.finale;
            var beat = UIX.CreateRow(Content, "StoryBeat", 105f);
            UIX.AddChildText(beat, "Beat",
                "NEXT STORY BEAT — " + (nextStage <= CampaignSystem.StagesPerChapter ? _chapter + "-" + nextStage : "CHAPTER COMPLETE") +
                "\n" + nextBrief,
                24, new Color(0.72f, 0.88f, 1f), TextAnchor.MiddleCenter, true);

            // stage grid 3 per row
            for (int s = 1; s <= CampaignSystem.StagesPerChapter; s += 3)
            {
                var row = UIX.CreateRow(Content, "StageRow" + s, 130f, 10, 10, 6, 6);
                for (int j = 0; j < 3 && s + j <= CampaignSystem.StagesPerChapter; j++)
                {
                    int stage = s + j;
                    bool unlocked = CampaignSystem.IsUnlocked(_chapter, stage);
                    int stars = CampaignSystem.StarsOf(_chapter, stage);
                    bool boss = stage == CampaignSystem.StagesPerChapter;

                    var cell = new GameObject("Stage" + stage);
                    var rt = cell.AddComponent<RectTransform>();
                    rt.SetParent(row, false);
                    var le = cell.AddComponent<LayoutElement>();
                    le.flexibleWidth = 1f;

                    var img = cell.AddComponent<Image>();
                    img.sprite = UIBuilder.SolidSprite;
                    img.color = !unlocked ? new Color(0.16f, 0.16f, 0.2f, 0.95f)
                        : stars > 0 ? new Color(0.16f, 0.4f, 0.24f, 0.97f)
                        : boss ? new Color(0.55f, 0.25f, 0.15f, 0.97f)
                        : new Color(0.3f, 0.35f, 0.5f, 0.97f);

                    var labelGo = new GameObject("Label");
                    var labelRt = labelGo.AddComponent<RectTransform>();
                    labelRt.SetParent(cell.transform, false);
                    labelRt.anchorMin = Vector2.zero;
                    labelRt.anchorMax = Vector2.one;
                    labelRt.offsetMin = Vector2.zero;
                    labelRt.offsetMax = Vector2.zero;

                    var txt = labelGo.AddComponent<Text>();
                    txt.font = UIBuilder.BuiltinFont;
                    txt.text = _chapter + "-" + stage + (boss ? "\nBOSS" : "") + "\n" +
                               (unlocked ? new string('*', stars) + new string('.', 3 - stars) : "LOCKED");
                    txt.fontSize = 28;
                    txt.color = unlocked ? Color.white : new Color(0.45f, 0.45f, 0.5f);
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.raycastTarget = false;

                    var btn = cell.AddComponent<Button>();
                    btn.targetGraphic = img;
                    btn.onClick.AddListener(() =>
                    {
                        if (!unlocked)
                        {
                            Manager.Notify("Clear the previous stage first!");
                            return;
                        }
                        Manager.CloseScreen();
                        CampaignSystem.StartStage(_chapter, stage);
                    });
                }
            }

            var hint = UIX.CreateRow(Content, "Hint", 110f);
            UIX.AddChildText(hint, "H",
                "1 star = clear  •  +1 star = no hero fell  •  +1 star = finish above 50% team HP\n" +
                "First clear grants diamonds; chapter bosses grant hero shards.",
                26, new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);
        }
    }

    // ====================================================================
    // TOWER — floor + difficulty selection, best-floor records.
    // ====================================================================
    public class TowerScreen : ScreenBase
    {
        private int _difficulty = 0;

        public override void Rebuild()
        {
            ClearContent();

            var tower = GameManager.Instance != null ? GameManager.Instance.Tower : null;
            int currentFloor = tower != null ? tower.CurrentFloor : 1;

            var head = UIX.CreateRow(Content, "Head", 170f);
            UIX.AddChildText(head, "T", "Floor " + currentFloor + "\nEndless enemies — every 10th floor is a BOSS", 34,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            var diffHeader = UIX.CreateRow(Content, "DiffHeader", 60f);
            UIX.AddChildText(diffHeader, "T", "DIFFICULTY", 28, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            var diffRow = UIX.CreateRow(Content, "DiffRow", 110f, 12, 12, 8, 8);
            string[] names = { "Easy", "Hard", "Hell", "Nightmare" };
            for (int i = 0; i < 4; i++)
            {
                int d = i;
                var btn = UIX.CreateRowButton(diffRow, names[i],
                    () => { _difficulty = d; Rebuild(); },
                    _difficulty == d ? new Color(0.7f, 0.45f, 0.1f) : new Color(0.25f, 0.28f, 0.38f),
                    90f, 215f, 28);
            }

            // records
            foreach (var rec in SaveSystem.Current.tower)
            {
                var row = UIX.CreateRow(Content, "Rec" + rec.difficulty, 76f);
                UIX.AddChildText(row, "N", names[Mathf.Clamp(rec.difficulty, 0, 3)], 28, Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "B", "Best floor " + rec.bestFloor, 28, new Color(0.7f, 0.85f, 1f), TextAnchor.MiddleRight, true);
            }

            var fight = UIX.CreateRow(Content, "Fight", 130f, 20, 20, 12, 12);
            UIX.CreateRowButton(fight, "CHALLENGE FLOOR " + currentFloor + " (" + names[_difficulty] + ")",
                () =>
                {
                    Manager.CloseScreen();
                    World.ProgressionSystems.StartTowerBattle(currentFloor, _difficulty, names[_difficulty]);
                },
                new Color(0.75f, 0.3f, 0.15f), 100f, 940f, 34);
        }
    }

    // ====================================================================
    // DUNGEONS — gold & exp, tiers, daily attempts.
    // ====================================================================
    public class DungeonScreen : ScreenBase
    {
        private int _tier = 0;

        public override void Rebuild()
        {
            ClearContent();

            var attempts = UIX.CreateRow(Content, "Attempts", 90f);
            UIX.AddChildText(attempts, "A",
                "Gold attempts left: " + DungeonSystem.AttemptsLeft(true) + " / " + DungeonSystem.AttemptsPerDay +
                "    EXP attempts left: " + DungeonSystem.AttemptsLeft(false) + " / " + DungeonSystem.AttemptsPerDay,
                28, new Color(0.7f, 0.85f, 1f), TextAnchor.MiddleCenter, true);

            var tierRow = UIX.CreateRow(Content, "TierRow", 110f, 12, 12, 8, 8);
            UIX.AddChildText(tierRow, "T", "Tier", 30, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true, 140f);
            for (int i = 0; i < 3; i++)
            {
                int t = i;
                UIX.CreateRowButton(tierRow, (t + 1).ToString(),
                    () => { _tier = t; Rebuild(); },
                    _tier == t ? new Color(0.7f, 0.45f, 0.1f) : new Color(0.25f, 0.28f, 0.38f), 80f, 220f, 32);
            }

            // gold dungeon card
            var goldCard = UIX.CreateRow(Content, "GoldCard", 240f);
            var gImg = goldCard.GetComponent<Image>();
            gImg.color = new Color(0.22f, 0.18f, 0.06f, 0.98f);
            UIX.AddChildText(goldCard, "Title", "GOLD MINE DEPTHS  —  Tier " + (_tier + 1), 36,
                new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter, true);
            UIX.AddChildText(goldCard, "Desc",
                "Fight through treasure guardians.\nReward: ~" + Economy.Format((_tier + 1) * (_tier + 1) * 9000L) + " gold",
                28, Color.white, TextAnchor.MiddleCenter, true);
            var gBtnRow = new GameObject("GBtn");
            var grt = gBtnRow.AddComponent<RectTransform>();
            grt.SetParent(goldCard, false);
            UIX.CreateRowButton(goldCard, "ENTER",
                () => { Manager.CloseScreen(); DungeonSystem.Start(true, _tier); },
                new Color(0.75f, 0.55f, 0.1f), 90f, 400f, 32);

            // exp dungeon card
            var expCard = UIX.CreateRow(Content, "ExpCard", 240f);
            var eImg = expCard.GetComponent<Image>();
            eImg.color = new Color(0.08f, 0.16f, 0.24f, 0.98f);
            UIX.AddChildText(expCard, "Title", "TRIAL OF EXPERIENCE  —  Tier " + (_tier + 1), 36,
                new Color(0.5f, 0.85f, 1f), TextAnchor.MiddleCenter, true);
            UIX.AddChildText(expCard, "Desc",
                "Ancient trial grounds. Winners' whole roster gains levels.\nReward: +" + (1 + _tier) + " levels per team hero",
                28, Color.white, TextAnchor.MiddleCenter, true);
            UIX.CreateRowButton(expCard, "ENTER",
                () => { Manager.CloseScreen(); DungeonSystem.Start(false, _tier); },
                new Color(0.2f, 0.45f, 0.7f), 90f, 400f, 32);

            var reset = UIX.CreateRow(Content, "Reset", 80f);
            UIX.AddChildText(reset, "R", "Attempts reset every day at 00:00 UTC", 26,
                new Color(0.6f, 0.6f, 0.7f), TextAnchor.MiddleCenter, true);
        }
    }

    // ====================================================================
    // PVP ARENA — rank, season, 3 opponents, real fights.
    // ====================================================================
    public class ArenaScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();
            ArenaSystem.EnsureOpponents();

            var head = UIX.CreateRow(Content, "Head", 200f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.24f, 0.12f, 0.08f, 0.98f);
            UIX.AddChildText(head, "Rank", "Rank " + ArenaSystem.Rank + "  —  " + ArenaSystem.RankTitle(), 40,
                new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter, true);
            UIX.AddChildText(head, "Rec",
                "Season wins: " + ArenaSystem.SeasonWins + "   •   Lifetime: " + ArenaSystem.Wins + "W / " + ArenaSystem.Losses + "L\n" +
                "Season ends in " + UIX.TimeShort(ArenaSystem.SeasonTimeLeft),
                28, Color.white, TextAnchor.MiddleCenter, true);

            var oppHeader = UIX.CreateRow(Content, "OppHeader", 60f);
            UIX.AddChildText(oppHeader, "T", "CHOOSE YOUR OPPONENT", 28, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            foreach (var opp in ArenaSystem.Opponents)
            {
                var row = UIX.CreateRow(Content, "Opp_" + opp.name, 120f);
                UIX.AddChildText(row, "Name", opp.name, 30, Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "Info", "Rank " + opp.rank + "\nPower " + Economy.Format(opp.power),
                    26, new Color(0.8f, 0.85f, 0.95f), TextAnchor.MiddleLeft, true);
                UIX.CreateRowButton(row, "FIGHT", () => { Manager.CloseScreen(); ArenaSystem.StartFight(opp); },
                    new Color(0.75f, 0.3f, 0.2f), 84f, 240f, 30);
            }

            var defRow = UIX.CreateRow(Content, "Defense", 110f);
            UIX.AddChildText(defRow, "D",
                "Your defense team is your battle formation (" + SaveSystem.Current.teamHeroIds.Count + " heroes).\n" +
                "Edit it in Heroes & Formation.",
                26, new Color(0.65f, 0.65f, 0.75f), TextAnchor.MiddleCenter, true);
        }
    }

    // ====================================================================
    // CONTINENT GATE — 5 regions of endgame stages.
    // ====================================================================
    public class ContinentGateScreen : ScreenBase
    {
        private int _region = 1;

        public override void Rebuild()
        {
            ClearContent();
            _region = Mathf.Clamp(_region, 1, ContinentGateSystem.Regions);

            var gateInfo = UIX.CreateRow(Content, "GateInfo", 100f);
            bool gated = CampaignSystem.ClearedStageCount() < 20;
            UIX.AddChildText(gateInfo, "G",
                gated ? "The Gate opens after 20 campaign stages (" + CampaignSystem.ClearedStageCount() + "/20)"
                      : "The Gate is open! Endgame regions with first-clear rewards",
                28, gated ? new Color(0.9f, 0.6f, 0.3f) : new Color(0.5f, 0.95f, 0.7f), TextAnchor.MiddleCenter, true);

            var nav = UIX.CreateRow(Content, "RegionNav", 110f, 14, 14, 10, 10);
            UIX.CreateRowButton(nav, "<", () => { _region--; Rebuild(); }, new Color(0.3f, 0.35f, 0.5f), 90f, 110f, 44);
            UIX.AddChildText(nav, "RName", "Region " + _region + "\n" + ContinentGateSystem.RegionNames[_region - 1],
                32, new Color(0.6f, 0.95f, 0.8f), TextAnchor.MiddleCenter, true);
            UIX.CreateRowButton(nav, ">", () => { _region++; Rebuild(); }, new Color(0.3f, 0.35f, 0.5f), 90f, 110f, 44);

            for (int s = 1; s <= ContinentGateSystem.StagesPerRegion; s += 5)
            {
                var row = UIX.CreateRow(Content, "CGRow" + s, 120f, 10, 10, 6, 6);
                for (int j = 0; j < 5 && s + j <= ContinentGateSystem.StagesPerRegion; j++)
                {
                    int stage = s + j;
                    bool unlocked = ContinentGateSystem.IsUnlocked(_region, stage);
                    bool cleared = ContinentGateSystem.StarsOf(_region, stage) > 0;

                    var cell = new GameObject("CG" + stage);
                    var rt = cell.AddComponent<RectTransform>();
                    rt.SetParent(row, false);
                    var le = cell.AddComponent<LayoutElement>();
                    le.flexibleWidth = 1f;

                    var img = cell.AddComponent<Image>();
                    img.sprite = UIBuilder.SolidSprite;
                    img.color = !unlocked ? new Color(0.16f, 0.16f, 0.2f, 0.95f)
                        : cleared ? new Color(0.15f, 0.42f, 0.3f, 0.97f)
                        : new Color(0.25f, 0.45f, 0.42f, 0.97f);

                    var labelGo = new GameObject("Label");
                    var labelRt = labelGo.AddComponent<RectTransform>();
                    labelRt.SetParent(cell.transform, false);
                    labelRt.anchorMin = Vector2.zero;
                    labelRt.anchorMax = Vector2.one;
                    labelRt.offsetMin = Vector2.zero;
                    labelRt.offsetMax = Vector2.zero;

                    var txt = labelGo.AddComponent<Text>();
                    txt.font = UIBuilder.BuiltinFont;
                    txt.text = stage + (stage == ContinentGateSystem.StagesPerRegion ? "\nBOSS" : "") + "\n" + (unlocked ? (cleared ? "DONE" : "READY") : "LOCK");
                    txt.fontSize = 26;
                    txt.color = unlocked ? Color.white : new Color(0.45f, 0.45f, 0.5f);
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.raycastTarget = false;

                    var btn = cell.AddComponent<Button>();
                    btn.targetGraphic = img;
                    btn.onClick.AddListener(() =>
                    {
                        Manager.CloseScreen();
                        ContinentGateSystem.StartStage(_region, stage);
                    });
                }
            }

            bool worldBossUnlocked = ContinentGateSystem.IsUnlocked(_region, 1);
            int worldBossAttempts = WorldBossSystem.AttemptsLeft(_region);
            var worldBoss = UIX.CreateRow(Content, "WorldBoss", 170f, 16, 16, 12, 12);
            worldBoss.GetComponent<Image>().color = new Color(0.22f, 0.10f, 0.18f, 0.98f);
            UIX.AddChildText(worldBoss, "BossInfo",
                "WORLD BOSS — " + WorldBossSystem.BossName(_region) + "\n" +
                (worldBossUnlocked ? "Daily attempts left: " + worldBossAttempts + " / " + WorldBossSystem.AttemptsPerRegionPerDay
                    : "Unlock this region's gate to challenge its World Boss."),
                27, new Color(1f, 0.82f, 0.65f), TextAnchor.MiddleLeft, true);
            UIX.CreateRowButton(worldBoss, worldBossAttempts > 0 ? "CHALLENGE" : "DAILY ATTEMPT USED",
                () => { Manager.CloseScreen(); WorldBossSystem.Start(_region); },
                new Color(0.68f, 0.22f, 0.18f), 100f, 260f, 26)
                .interactable = worldBossUnlocked && worldBossAttempts > 0;

            var reward = UIX.CreateRow(Content, "Reward", 90f);
            UIX.AddChildText(reward, "R", "First clear of every stage: +120 diamonds + random equipment", 26,
                new Color(0.7f, 0.85f, 1f), TextAnchor.MiddleCenter, true);
        }
    }
}
