using UnityEngine;

namespace LegendsRPG.World
{
    /// <summary>
    /// The twelve-act story spine for the Legends Reborn campaign.
    /// Every chapter has an opening, a midpoint reveal and a chapter-finale beat.
    /// The campaign remains playable without cutscene assets; these lines are used
    /// by the stage briefing and the victory panel.
    /// </summary>
    public static class CampaignStory
    {
        public sealed class Chapter
        {
            public readonly string title;
            public readonly string region;
            public readonly string synopsis;
            public readonly string opening;
            public readonly string turningPoint;
            public readonly string finale;
            public readonly string bossName;
            public readonly string bossKey;

            public Chapter(string title, string region, string synopsis,
                string opening, string turningPoint, string finale,
                string bossName, string bossKey)
            {
                this.title = title; this.region = region; this.synopsis = synopsis;
                this.opening = opening; this.turningPoint = turningPoint;
                this.finale = finale; this.bossName = bossName; this.bossKey = bossKey;
            }
        }

        private static readonly Chapter[] Chapters =
        {
            new Chapter("The First Ember", "Emberfall Woods",
                "A silent forest is burning from beneath the roots. Kael discovers that the flame is a broken piece of an ancient seal, and that the fire is calling the heroes together.",
                "A wounded scout leads the party into Emberfall. Something beneath the old grove has awakened.",
                "The flames move against the wind. The party finds the first seal and learns that someone is collecting them.",
                "The Ember Titan falls. The first seal is restored, but a voice from the ashes speaks the name of the Hollow King.",
                "Cindervault, the Ember Titan", "chapter_boss_01"),
            new Chapter("The Drowned Oath", "Tidebringer Coast",
                "Ships vanish beneath a glass-still sea. The Tidebringer relic is hidden in a lighthouse whose keeper has been guarding a false history.",
                "Follow the lanterns along the coast before the next tide erases the villages.",
                "The lighthouse records reveal that the seals were made to imprison a king, not to protect a treasure.",
                "The Leviathan Warden is defeated and the Tide seal is recovered. The party now knows the enemy is searching for the same prison.",
                "Nereth, Warden of the Deep", "chapter_boss_02"),
            new Chapter("The Veil Below", "Gloomshade Marsh",
                "Memories are being stolen from the marsh villages. The stolen memories form a map to the prison, and one of the heroes recognizes a forgotten oath.",
                "A village has forgotten its own name. Find the source before the fog spreads.",
                "The party sees fragments of the first war and learns that the Hollow King once fought beside the seal guardians.",
                "The Mire Matriarch is silenced. A hidden memory reveals the traitor who opened the first breach.",
                "Morvane, Mire Matriarch", "chapter_boss_03"),
            new Chapter("The Last Warmth", "Frostpeak Ascent",
                "A permanent winter covers Frostpeak. The mountain's heart is keeping the world alive, but its guardian has mistaken every traveller for an invader.",
                "Climb above the snow line and relight the beacon towers.",
                "The guardian tests the party's resolve and reveals that the seals are weakening from the inside.",
                "The Frostbound Colossus is defeated. The mountain releases a map to the Sunken Citadel.",
                "Varkuun, Frostbound Colossus", "chapter_boss_04"),
            new Chapter("The Sunken Record", "Sunken Citadel",
                "The lost citadel contains the original account of the seal war. Its drowned library reveals why the Hollow King was imprisoned and what the seals demand in return.",
                "Dive into the archive and recover the pages before the current tears them apart.",
                "The archive shows that the king tried to stop a cosmic force, but chose to sacrifice the guardians to do it.",
                "The Abyssal Archivist falls. The party keeps the truth, even though it makes the coming battle harder.",
                "Thalassa, Abyssal Archivist", "chapter_boss_05"),
            new Chapter("Storm of Names", "Stormvein Plains",
                "The storm carries the names of the fallen guardians. The party must awaken the six storm pylons before the plains are swallowed by lightning.",
                "Secure the storm road and reconnect the broken pylons.",
                "The storm reveals that the Hollow King is not the force behind the Void. Something has been feeding on his prison.",
                "The Tempest Sovereign is overcome. The sixth seal points toward the Ashen Wastes.",
                "Raijin, Tempest Sovereign", "chapter_boss_06"),
            new Chapter("The Broken Forge", "Ashen Wastes",
                "A forgotten forge can restore the seals, but its master has been turning defeated guardians into weapons for the enemy.",
                "Cross the ash dunes and find the forge before its final furnace opens.",
                "The forge master offers the party a shortcut: break the seals and free the king. The heroes refuse.",
                "The Ashen Forgemaster is defeated. The party recovers the tool needed to mend the final seal.",
                "Drazhul, Ashen Forgemaster", "chapter_boss_07"),
            new Chapter("The Night Covenant", "Nightfall Hollow",
                "The shadow covenant was created to guard the king's prison. Its last members have split into rival factions, each hiding a different part of the truth.",
                "Enter the hollow without trusting the lights that follow you.",
                "The party learns that one seal must be willingly released to reach the prison's outer gate.",
                "The Umbral Regent falls. The heroes choose to carry the released seal themselves.",
                "Nyxara, Umbral Regent", "chapter_boss_08"),
            new Chapter("The Crystal Witness", "Crystal Spire",
                "The spire records every possible future. Its crystal heart shows the cost of victory: the guardians may have to give up the power that made them legends.",
                "Reach the spire's summit and consult the crystal witness.",
                "The vision shows the Hollow King fighting the Void alone after the guardians abandoned him.",
                "The Prism Oracle is defeated. The party decides to save the king as well as the world.",
                "Auralith, Prism Oracle", "chapter_boss_09"),
            new Chapter("Beyond the Breach", "Void Breach",
                "The Void has entered the world through cracks in the prison. Its creatures imitate the heroes and turn their strongest memories against them.",
                "Seal the outer rifts before the Void reaches the city.",
                "The party discovers that the Hollow King has been resisting the Void from inside his prison.",
                "The Rift Devourer is destroyed. The way to the Celestial Gate is open.",
                "Vorath, Rift Devourer", "chapter_boss_10"),
            new Chapter("The Celestial Trial", "Celestial Gate",
                "The last guardians test the heroes. Only a united team can pass the gate and face the king without repeating the betrayal of the first war.",
                "Reunite the guardian emblems and open the celestial bridge.",
                "Each hero accepts a part of the burden once carried by the old guardians.",
                "The Celestial Arbiter yields. The throne is no longer protected by a seal, but by the choice of the living.",
                "Elyndra, Celestial Arbiter", "chapter_boss_11"),
            new Chapter("Throne of Legends", "Throne of Legends",
                "At the throne, the heroes meet the Hollow King and the Void that consumed his kingdom. The final choice is not to destroy a fallen guardian, but to stand with him.",
                "Enter the throne realm and defeat the echoes of the first war.",
                "The Hollow King reveals his true name and gives the heroes the last fragment of the seal.",
                "The Void is banished and the king is freed. A new order is founded by the heroes: the Legends Reborn.",
                "Aetherion, the Hollow King", "chapter_boss_12")
        };

        public static Chapter GetChapter(int chapter)
        {
            if (chapter < 1 || chapter > Chapters.Length) return Chapters[0];
            return Chapters[chapter - 1];
        }

        public static string GetStageBrief(int chapter, int stage)
        {
            var c = GetChapter(chapter);
            switch (Mathf.Clamp(stage, 1, 15))
            {
                case 1: return c.opening;
                case 2: return "Track the first corrupted patrol through " + c.region + ". " + c.synopsis;
                case 3: return "Rescue the stranded settlers and learn which path the enemy is guarding.";
                case 4: return "Break the enemy supply line before the corruption reaches the next refuge.";
                case 5: return "Recover the first relic fragment and protect it from the pursuing hunters.";
                case 6: return "Defend the forward camp while the team studies the relic's warning.";
                case 7: return "Defeat the guardian's champion and open the route to the inner territory.";
                case 8: return c.turningPoint;
                case 9: return "Enter the inner ruins and follow the evidence left by the previous seal keepers.";
                case 10: return "Destroy the corrupted ward before it can summon another wave.";
                case 11: return "Escort the surviving keeper through the hostile approach.";
                case 12: return "Recover the missing seal piece and bring it to the chapter sanctuary.";
                case 13: return "Hunt down the lieutenant protecting " + c.bossName + ".";
                case 14: return "Secure the sanctuary, restore the team's strength and prepare for " + c.bossName + ".";
                default: return "FINAL ENCOUNTER — " + c.bossName + ". " + c.finale;
            }
        }

        public static string GetChapterSummary(int chapter) { return GetChapter(chapter).synopsis; }
        public static string GetOpening(int chapter) { return GetChapter(chapter).opening; }
        public static string GetBossName(int chapter) { return GetChapter(chapter).bossName; }
        public static string GetBossKey(int chapter) { return GetChapter(chapter).bossKey; }
        public static string GetFinale(int chapter) { return GetChapter(chapter).finale; }
        public static int Count { get { return Chapters.Length; } }
    }
}
