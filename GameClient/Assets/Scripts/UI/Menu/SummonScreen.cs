using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Summon screen (v0.4) — built inside MenuController via Push("SUMMON", ...).
    /// Layout mirrors the reference summon page:
    ///   left   banner tabs (Basic blue / Advanced gold / Supreme red)
    ///   centre banner stage: name, featured rarity, pity progress, rate table
    ///   bottom sum buttons: SUMMON x1 and SUMMON x10 with live costs
    ///   wish   Faction Wish panel (element picker once 60 total pulls are done)
    /// Results appear on a full-screen overlay and flip open one card at a time
    /// (unscaled time — the battle is paused while any menu is open).
    /// </summary>
    public static class MenuSummonScreen
    {
        private static int _banner = (int)BannerId.Advanced;
        private static MenuController _menu;
        private static RectTransform _body;

        private static Text _pityText, _totalText, _wishText;
        private static RectTransform _wishRow;
        private static RectTransform _overlay;

        // ======================= build =======================
        public static void Build(MenuController menu, RectTransform body)
        {
            _menu = menu;
            _body = body;

            BuildBannerTabs(body);
            BuildStage(body);
            BuildSumButtons(body);
        }

        // ======================= left: banner tabs =======================
        private static void BuildBannerTabs(RectTransform body)
        {
            for (int i = 0; i < GachaSystem.Banners.Length; i++)
            {
                int idx = i;
                var b = GachaSystem.Banners[i];
                var tab = MenuTheme.BtnAt(body, "Tab" + i, b.name.Split(' ')[0], new Vector2(0f, 1f),
                    new Vector2(140f, -90f - i * 130f), new Vector2(240f, 110f),
                    idx == _banner ? b.theme : Color.Lerp(b.theme, Color.black, 0.55f), Color.white, 32,
                    () => { _banner = idx; Rebuild(); });
            }

            // total pulls + wish unlock progress under the tabs
            var info = MenuTheme.Box(body, "PullInfo", new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.45f), true, true);
            info.sizeDelta = new Vector2(240f, 190f);
            info.anchoredPosition = new Vector2(140f, 60f);
            _totalText = MenuTheme.Label(info, "", 26, MenuTheme.Parchment, TextAnchor.UpperLeft);
            _totalText.rectTransform.offsetMin = new Vector2(10f, 10f);
            _totalText.rectTransform.offsetMax = new Vector2(-10f, -10f);
            _totalText.resizeTextForBestFit = false;
        }

        // ======================= centre: banner stage =======================
        private static void BuildStage(RectTransform body)
        {
            var b = GachaSystem.Banners[_banner];

            var stage = MenuTheme.Framed(body, "Stage", Vector2.zero, Vector2.one,
                new Vector2(300f, 30f), new Vector2(-640f, -30f), AnimeTheme.Rarity(Rarity.Legendary), AnimeTheme.Panel, 6f);

            var title = MenuTheme.Label(stage, b.name, 56, AnimeTheme.Gold, TextAnchor.UpperCenter, true);
            title.rectTransform.offsetMin = new Vector2(6f, 300f);
            title.rectTransform.offsetMax = new Vector2(-6f, 220f);

            var sub = MenuTheme.Label(stage, b.sub, 28, AnimeTheme.Text, TextAnchor.UpperCenter);
            sub.rectTransform.offsetMin = new Vector2(20f, 250f);
            sub.rectTransform.offsetMax = new Vector2(-20f, 200f);

            // featured rarity chips
            var chips = MenuTheme.Box(stage, "Chips", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, -320f), new Vector2(0f, -270f), Color.clear, false);
            string[] rar = { "RARE", "EPIC", "LEGENDARY", b.mythic > 0f ? "MYTHIC" : null };
            Color[] rc =
            {
                ItemDatabase.RarityColor(1), ItemDatabase.RarityColor(2),
                ItemDatabase.RarityColor(3), ItemDatabase.RarityColor(4)
            };
            int n = 0;
            for (int i = 0; i < rar.Length; i++)
            {
                if (rar[i] == null) continue;
                var chip = MenuTheme.Box(chips, "Chip" + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero, Color.Lerp(rc[i], Color.black, 0.25f), true, false);
                chip.sizeDelta = new Vector2(220f, 46f);
                chip.anchoredPosition = new Vector2((n - 1) * 240f, 0f);
                MenuTheme.Label(chip, rar[i], 26, Color.white, TextAnchor.MiddleCenter, true);
                n++;
            }

            // pity + rates
            _pityText = MenuTheme.Label(stage, "", 30, MenuTheme.Gold, TextAnchor.UpperCenter, true);
            _pityText.rectTransform.offsetMin = new Vector2(16f, 110f);
            _pityText.rectTransform.offsetMax = new Vector2(-16f, 40f);

            string rates = b.mythic > 0f
                ? string.Format("Rates:  Rare {0:0}%   Epic {1:0}%   Legendary {2:0}%   Mythic {3:0}%",
                    b.rare * 100f, b.epic * 100f, b.legendary * 100f, b.mythic * 100f)
                : string.Format("Rates:  Common {0:0}%   Rare {1:0}%   Epic {2:0}%   Legendary {3:0}%",
                    (1f - b.rare - b.epic - b.legendary) * 100f, b.rare * 100f, b.epic * 100f, b.legendary * 100f);
            var rateLbl = MenuTheme.Label(stage, rates, 26, MenuTheme.ParchmentDark, TextAnchor.MiddleCenter);
            rateLbl.rectTransform.offsetMin = new Vector2(16f, 80f);
            rateLbl.rectTransform.offsetMax = new Vector2(-16f, 40f);

            var floor = MenuTheme.Label(stage,
                "x10 guarantee: at least one " + b.floorRarity + "+", 26, MenuTheme.ParchmentDark, TextAnchor.MiddleCenter);
            floor.rectTransform.offsetMin = new Vector2(16f, 40f);
            floor.rectTransform.offsetMax = new Vector2(-16f, 6f);

            // faction wish panel (bottom of the stage)
            var wish = MenuTheme.Box(stage, "Wish", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(16f, 16f), new Vector2(-16f, 150f), new Color(0.16f, 0.10f, 0.04f, 0.9f), true, true);
            _wishText = MenuTheme.Label(wish, "", 26, MenuTheme.Gold, TextAnchor.UpperCenter, true);
            _wishText.rectTransform.offsetMin = new Vector2(10f, 56f);
            _wishText.rectTransform.offsetMax = new Vector2(-10f, 0f);

            _wishRow = MenuTheme.Box(wish, "WishRow", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(10f, 8f), new Vector2(-10f, 60f), Color.clear, false);
            BuildWishRow();
            RefreshTexts();
        }

        private static void BuildWishRow()
        {
            for (int i = _wishRow.childCount - 1; i >= 0; i--) Object.Destroy(_wishRow.GetChild(i).gameObject);

            bool unlocked = GachaSystem.WishUnlocked;
            string chosen = SaveSystem.Current.wishElement;
            string[] names = { "Fire", "Water", "Ice", "Lightning", "Nature", "Dark", "Light" };

            if (!unlocked)
            {
                var lockLbl = MenuTheme.Label(_wishRow,
                    "Faction Wish unlocks at " + GachaSystem.WishUnlockPulls + " total summons", 24,
                    MenuTheme.ParchmentDark, TextAnchor.MiddleCenter);
                lockLbl.resizeTextForBestFit = true;
                return;
            }

            for (int i = 0; i < names.Length; i++)
            {
                int idx = i;
                string nm = names[i];
                Color c = ElementalMatrix.GetElementColor((ElementType)(i + 1));
                var b = MenuTheme.Btn(_wishRow, "W" + i, nm, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero, chosen == nm ? c : Color.Lerp(c, Color.black, 0.55f), Color.white, 22,
                    () =>
                    {
                        GachaSystem.SetWish(nm);
                        if (_menu != null) _menu.ShowToast("Wish set: " + nm + " — the next guaranteed Legendary matches it");
                        Rebuild();
                    });
                var rt = (RectTransform)b.transform;
                rt.sizeDelta = new Vector2(150f, 52f);
                rt.anchoredPosition = new Vector2((i - 3) * 160f, 0f);
            }
        }

        // ======================= bottom: sum buttons =======================
        private static void BuildSumButtons(RectTransform body)
        {
            var b = GachaSystem.Banners[_banner];
            bool gold = GachaSystem.IsGoldBanner((BannerId)_banner);

            string c1 = gold ? MenuController.FormatNumber(b.costGold1) + " gold" : MenuController.FormatNumber(b.costGems1) + " gems";
            string c10 = gold ? MenuController.FormatNumber(b.costGold10) + " gold" : MenuController.FormatNumber(b.costGems10) + " gems";

            MenuTheme.BtnAt(body, "Pull1", "SUMMON x1  (" + c1 + ")", new Vector2(1f, 0f),
                new Vector2(-300f, 130f), new Vector2(560f, 90f), b.theme, Color.white, 32,
                () => DoPull(1));

            MenuTheme.BtnAt(body, "Pull10", "SUMMON x10  (" + c10 + ")", new Vector2(1f, 0f),
                new Vector2(-300f, 30f), new Vector2(560f, 90f), Color.Lerp(b.theme, Color.black, 0.25f), Color.white, 32,
                () => DoPull(10));

            var tip = MenuTheme.Label(body, "New heroes join your team automatically. Duplicates become Hero Essence + gold.",
                24, MenuTheme.ParchmentDark, TextAnchor.UpperRight);
            tip.rectTransform.anchorMin = new Vector2(1f, 0f);
            tip.rectTransform.anchorMax = new Vector2(1f, 0f);
            tip.rectTransform.offsetMin = new Vector2(-620f, 8f);
            tip.rectTransform.offsetMax = new Vector2(-24f, 34f);
        }

        // ======================= actions =======================
        private static void DoPull(int count)
        {
            if (_overlay != null) return;   // results still showing

            string error;
            var results = GachaSystem.Pull((BannerId)_banner, count, out error);
            if (results == null)
            {
                if (_menu != null) _menu.ShowToast(error);
                return;
            }

            RefreshTexts();
            ShowResults(results);
        }

        // ======================= results overlay =======================
        private static void ShowResults(List<GachaResult> results)
        {
            _overlay = MenuTheme.Box(_body, "ResultsOverlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.01f, 0.02f, 0.97f), false, true);

            var title = MenuTheme.Label(_overlay, "SUMMON RESULTS", 52, MenuTheme.Gold, TextAnchor.UpperCenter, true);
            title.rectTransform.offsetMin = new Vector2(0f, 190f);

            var grid = MenuTheme.ScrollGrid(_overlay, "Cards", Vector2.zero, Vector2.one,
                new Vector2(220f, 130f), new Vector2(-220f, 110f), new Vector2(200f, 250f), new Vector2(22f, 22f), 5);

            var covers = new List<RectTransform>();
            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                Color rc = ItemDatabase.RarityColor(1 + (int)r.hero.rarity);

                rc = AnimeTheme.Rarity(r.hero.rarity);
                var card = MenuTheme.Box(grid, "Card" + i, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, rc, true, false);
                var inner = MenuTheme.Box(card, "Fill", Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f),
                    AnimeTheme.Panel, true, false);

                var portrait = MenuTheme.HeroPortrait(inner, new Vector2(0.5f, 0.62f), Vector2.zero, 130f, r.hero, 1);
                AnimeTheme.AddRarityPips(inner, r.hero.rarity, new Vector2(0.5f, 0.30f), new Vector2(-((int)r.hero.rarity * 9.5f), 6f), 5);
                MenuTheme.Label(inner, r.hero.displayName, 26, AnimeTheme.Text, TextAnchor.LowerCenter);

                if (r.isNew)
                {
                    var badge = MenuTheme.Box(inner, "New", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                        Vector2.zero, Vector2.zero, MenuTheme.Red, true, false);
                    badge.sizeDelta = new Vector2(110f, 40f);
                    badge.anchoredPosition = new Vector2(0f, -20f);
                    MenuTheme.Label(badge, "NEW", 24, Color.white, TextAnchor.MiddleCenter, true);
                }
                else
                {
                    var badge = MenuTheme.Box(inner, "Dupe", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                        Vector2.zero, Vector2.zero, new Color(0.25f, 0.45f, 0.75f), true, false);
                    badge.sizeDelta = new Vector2(170f, 40f);
                    badge.anchoredPosition = new Vector2(0f, -20f);
                    MenuTheme.Label(badge, "+" + r.essence + " Essence", 22, Color.white, TextAnchor.MiddleCenter, true);
                }

                if (r.hero.rarity == Rarity.Mythic || r.hero.rarity == Rarity.Legendary)
                {
                    var glow = MenuTheme.Box(card, "Glow", Vector2.zero, Vector2.one, new Vector2(-4f, -4f),
                        new Vector2(4f, 4f), new Color(1f, 0.9f, 0.4f, 0.25f), true, false);
                    glow.GetComponent<Image>().raycastTarget = false;
                }

                // cover that hides the card until its reveal moment
                var cover = MenuTheme.Box(card, "Cover", Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f),
                        AnimeTheme.Ink, true, false);
                MenuTheme.Label(cover, "?", 90, new Color(1f, 1f, 1f, 0.35f), TextAnchor.MiddleCenter);
                covers.Add(cover);
            }

            var cont = MenuTheme.BtnAt(_overlay, "Continue", "TAP TO CONTINUE", new Vector2(0.5f, 0f),
                new Vector2(0f, 60f), new Vector2(520f, 90f), MenuTheme.GoldDark, Color.white, 34, CloseOverlay);
            cont.gameObject.SetActive(false);

            if (_menu != null)
                _menu.StartCoroutine(RevealRoutine(covers, cont));
        }

        private static System.Collections.IEnumerator RevealRoutine(List<RectTransform> covers, Button cont)
        {
            var wait = new WaitForSecondsRealtime(0.28f);
            for (int i = 0; i < covers.Count; i++)
            {
                yield return wait;
                if (covers[i] != null) Object.Destroy(covers[i].gameObject);
            }
            yield return new WaitForSecondsRealtime(0.15f);
            if (cont != null) cont.gameObject.SetActive(true);
        }

        private static void CloseOverlay()
        {
            if (_overlay == null) return;
            Object.Destroy(_overlay.gameObject);
            _overlay = null;
            Rebuild();
        }

        // ======================= refresh helpers =======================
        private static void RefreshTexts()
        {
            var s = SaveSystem.Current;

            if (_totalText != null)
            {
                _totalText.text = "Total summons: " + s.gachaTotalPulls +
                                  "\nCollected: " + CountOwned() + " / " + HeroDatabase.AllHeroes.Count +
                                  "\nEssence: " + Inventory.Count("hero_essence");
            }

            if (_pityText != null)
            {
                int toGo = GachaSystem.PullsToLegendary(_banner);
                _pityText.text = toGo <= 0
                    ? "LEGENDARY GUARANTEED ON THE NEXT PULL!"
                    : "Legendary guaranteed within " + toGo + " more pull" + (toGo == 1 ? "" : "s");
            }

            if (_wishText != null)
            {
                _wishText.text = GachaSystem.HasWish
                    ? "Faction Wish: " + SaveSystem.Current.wishElement
                    : "Faction Wish";
            }
        }

        private static int CountOwned()
        {
            int n = 0;
            var s = SaveSystem.Current;
            var roster = HeroDatabase.AllHeroes;
            for (int i = 0; i < roster.Count; i++)
            {
                if (s.unlockedHeroIds.Contains(roster[i].heroId)) n++;
            }
            return n;
        }

        private static void Rebuild()
        {
            if (_menu != null) _menu.RefreshScreens();
        }
    }
}
