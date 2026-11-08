using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Characters;
using LegendsRPG.Core;
using LegendsRPG.Team;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Main hub screen (v0.3): the game's "home" shown over the 3D hub
    /// diorama. Mirrors the reference layout:
    ///   top-left   player card (avatar, name, level, VIP, power) + Lineup/Tips/Preview
    ///   top-right  gold + gem chips with (+) buttons
    ///   left side  Gifted Gear card, Feed, Mail
    ///   right side events column (Fire Crown, 8-Day Sign in, Relic Wish, Benefit,
    ///              Merchant) + timed event banner + daily purchase
    ///   bottom     nav bar: Missions, Chat strip, Tech, Shop, Bounty, Forge, Heroes, Bag
    ///   centre     AFK chest chip with countdown + ENTER BATTLE button
    /// Runs while Time.timeScale = 0 (battle paused); all timers use unscaled time.
    /// </summary>
    public class HubScreen : MonoBehaviour
    {
        public static HubScreen Instance { get; private set; }
        public bool IsOpen { get; private set; }

        /// <summary>Optional bridge to the old management screens (set from the GameRoot patch).</summary>
        public Func<string, bool> ManagementRouter;

        private TeamManager _team;
        private HUDController _hud;
        private MenuController _menu;
        private Canvas _canvas;
        private Text _goldText, _gemText, _toast, _powerText;
        private Text _afkTimerText, _eventTimerText, _chatText, _gearText, _afkRewardText;
        private Button _afkClaimButton;
        private RectTransform _afkBar;
        private float _toastTimer;
        private float _afkRemaining = 8f * 3600f;   // 08:00:00 like the reference
        private float _eventRemaining = 107f * 60f;   // 01:47:16 style countdown
        private float _chatTimer;
        private int _chatIndex;
        private bool _enteredBattle;

        // fake chat feed (until the backend chat lands)
        private static readonly string[] ChatLines =
        {
            "[K580] KAISEN: gl hf everyone",
            "[K582] Kulangot1: wla bang pinoy ***o",
            "[K583] NewAdventurer4979208: abc",
            "[K580] KAISEN: floor 11 clear, finally",
            "[S1] Aurelia: guild recruiting, pm me",
            "[K577] DragoonX: that rose execute is broken",
        };

        // ======================= lifecycle =======================
        public void Build(TeamManager team, HUDController hud, MenuController menu)
        {
            Instance = this;
            _team = team;
            _hud = hud;
            _menu = menu;

            _canvas = UIBuilder.CreateCanvas("HubCanvas");
            _canvas.sortingOrder = 60;
            var root = _canvas.transform;

            BuildTopLeftProfile(root);
            BuildTopRightCurrencies(root);
            BuildLeftColumn(root);
            BuildRightEvents(root);
            BuildBottomNav(root);
            BuildAfkChip(root);
            BuildToast(root);
        }

        private void OnDestroy()
        {
            Inventory.Changed -= RefreshCurrency;
            if (Instance == this) Instance = null;
        }

        // ======================= open / close =======================
        public void Open()
        {
            IsOpen = true;
            _enteredBattle = false;
            _canvas.gameObject.SetActive(true);
            Time.timeScale = 0f;
            if (_hud != null) _hud.SetVisible(false);
            if (MenuController.Instance != null && MenuController.Instance.IsOpen)
                MenuController.Instance.CloseToBattle(); // keep the old menu hidden under us
            Inventory.Changed -= RefreshCurrency;
            Inventory.Changed += RefreshCurrency;
            RefreshCurrency();
        }

        /// <summary>Hide the hub and resume the battle (ENTER BATTLE).</summary>
        public void EnterBattle()
        {
            IsOpen = false;
            _enteredBattle = true;
            _canvas.gameObject.SetActive(false);
            if (_menu != null) _menu.CloseToBattle();
            else Time.timeScale = 1f;
        }

        /// <summary>Hide the hub and open one of the existing management screens (heroes/bag/shop/missions).</summary>
        public void OpenManagement(string id)
        {
            if (ManagementRouter != null && ManagementRouter(id))
            {
                IsOpen = false;
                _canvas.gameObject.SetActive(false);
            }
            else Toast(id + ": coming soon");
        }

        /// <summary>3D building taps arrive here.</summary>
        public void NotifyBuilding(string id)
        {
            switch (id)
            {
                case "summon": OpenManagement("summon"); break; // v0.4: hero summoning is live
                case "afk": Toast("AFK chest is still collecting..."); break;
                case "forge": Toast("Forge: crafting arrives with the equipment update"); break;
                case "beast": Toast("Beast: dispatch system coming soon"); break;
                case "mine": Toast("Dwarven Mine: economy expansion coming soon"); break;
                case "dungeon": Toast("Dungeon: daily dungeons coming soon"); break;
                case "guild": Toast("Guild hall: guilds arrive in the guild phase"); break;
                case "arena": Toast("Arena: PvP arrives in the PvP phase"); break;
                case "tower": Toast("Power Leaderboard coming soon"); break;
                case "continent": OpenManagement("campaign"); break; // campaign map is the main-line portal
                default: Toast(id); break;
            }
        }

        // ======================= update =======================
        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            if (_toastTimer > 0f)
            {
                _toastTimer -= dt;
                if (_toastTimer <= 0f) _toast.transform.parent.gameObject.SetActive(false);
            }

            if (!IsOpen)
            {
                // entered battle earlier and the floor just ended -> bring the player home
                if (_enteredBattle && GameManager.Instance != null &&
                    GameManager.Instance.State == GameManager.GameState.FloorCleared)
                    Open();
                return;
            }

            // countdowns
            if (_afkRemaining > 0f) { _afkRemaining -= dt; if (_afkRemaining < 0f) _afkRemaining = 0f; }
            if (_eventRemaining > 0f) { _eventRemaining -= dt; if (_eventRemaining < 0f) _eventRemaining = 24f * 3600f; }
            if (_afkTimerText != null) _afkTimerText.text = FormatClock(_afkRemaining);
            if (_afkRewardText != null)
            {
                _afkRewardText.text = IdleProgression.HasPending
                    ? "+" + MenuController.FormatNumber(IdleProgression.PendingGold) + " GOLD  /  +" +
                      MenuController.FormatNumber(IdleProgression.PendingExp) + " EXP"
                    : "AFK rewards are collecting";
            }
            if (_afkClaimButton != null) _afkClaimButton.interactable = IdleProgression.HasPending;
            if (_eventTimerText != null) _eventTimerText.text = FormatClock(_eventRemaining);
            if (_afkBar != null)
            {
                float k = 1f - _afkRemaining / (8f * 3600f);
                _afkBar.sizeDelta = new Vector2(260f * Mathf.Clamp01(k), 14f);
            }

            // chat ticker
            _chatTimer -= dt;
            if (_chatTimer <= 0f)
            {
                _chatTimer = 4f;
                _chatIndex = (_chatIndex + 1) % ChatLines.Length;
                if (_chatText != null) _chatText.text = ChatLines[_chatIndex];
            }
        }

        public static string FormatClock(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int h = (int)(seconds / 3600f);
            int m = (int)((seconds % 3600f) / 60f);
            int s = (int)(seconds % 60f);
            return h > 0
                ? string.Format("{0:00}:{1:00}:{2:00}", h, m, s)
                : string.Format("{0:00}:{1:00}", m, s);
        }

        // ======================= top-left: player card =======================
        private void BuildTopLeftProfile(Transform root)
        {
            var card = MenuTheme.Box(root, "PlayerCard", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(14f, -14f), new Vector2(560f, -4f), new Color(0.07f, 0.04f, 0.03f, 0.9f), true, true);

            // avatar (first hero stands in for the player portrait)
            HeroData avatar = _team != null && _team.Heroes.Count > 0 ? _team.Heroes[0].Data : null;
            int level = _team != null && _team.Heroes.Count > 0 ? _team.Heroes[0].Stats.Level : 1;
            if (avatar != null)
                MenuTheme.HeroPortrait(card, new Vector2(0f, 1f), new Vector2(64f, -62f), 104f, avatar, level);

            var nameCol = MenuTheme.Box(card, "NameCol", new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(128f, 10f), new Vector2(-150f, -10f), Color.clear, false);
            MenuTheme.Label(nameCol, "1" + (avatar != null ? avatar.displayName : "Adventurer"), 34, Color.white, TextAnchor.MiddleLeft, true);
            // XP bar
            var xp = MenuTheme.Framed(nameCol, "Xp", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(2f, 14f), new Vector2(-2f, 30f), MenuTheme.GoldDark, new Color(0.2f, 0.5f, 0.25f), 2f);
            MenuTheme.Label(xp, "", 1, Color.clear, TextAnchor.MiddleCenter);

            // VIP badge
            var vip = MenuTheme.Box(card, "Vip", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(132f, -26f), new Vector2(268f, 26f), new Color(0.45f, 0.2f, 0.65f), true);
            MenuTheme.Label(vip, "VIP 3", 28, new Color(0.95f, 0.8f, 1f), TextAnchor.MiddleCenter, true);

            // power
            var pow = MenuTheme.Box(card, "Power", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(282f, -26f), new Vector2(548f, 26f), Color.clear, false);
            _powerText = MenuTheme.Label(pow, "", 32, MenuTheme.Gold, TextAnchor.MiddleLeft, true);
            // TODO: replace with the real power formula once hero stats define one
            int power = 0;
            if (_team != null)
                for (int i = 0; i < _team.Heroes.Count; i++)
                    power += _team.Heroes[i].Stats.Level * 997 + 1337;
            _powerText.text = "⚔ " + MenuController.FormatNumber(power);

            // Lineup / Tips / Preview
            string[] small = { "Lineup", "Tips", "Preview" };
            for (int i = 0; i < small.Length; i++)
            {
                int idx = i;
                var btn = MenuTheme.BtnAt(card, "Small" + small[i], small[i], new Vector2(0f, 0f), new Vector2(152f + i * 138f, 30f),
                    new Vector2(128f, 52f), new Color(0.24f, 0.15f, 0.09f), MenuTheme.Parchment, 26,
                    () =>
                    {
                        if (idx == 0) OpenManagement("heroes");
                        else Toast(small[idx] + ": coming soon");
                    });
            }
        }

        // ======================= top-right: currencies =======================
        private void BuildTopRightCurrencies(Transform root)
        {
            var bar = MenuTheme.Box(root, "Currencies", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-640f, -14f), new Vector2(-14f, -4f), Color.clear, false);

            BuildCurrencyChip(bar, "Gold", new Vector2(1f, 0.5f), new Vector2(-430f, -28f),
                new Color(1f, 0.85f, 0.35f), true, () => Toast("Gold: earned from battles and AFK chest"));
            BuildCurrencyChip(bar, "Gems", new Vector2(1f, 0.5f), new Vector2(-140f, -28f),
                new Color(0.55f, 0.8f, 1f), false, () => Toast("Gems: premium currency, shop coming soon"));
        }

        private void BuildCurrencyChip(Transform parent, string id, Vector2 anchor, Vector2 pos,
            Color coinColor, bool isGold, Action onPlus)
        {
            var chip = MenuTheme.Box(parent, id + "Chip", anchor, anchor, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.55f), true, true);
            chip.sizeDelta = new Vector2(260f, 60f);
            chip.anchoredPosition = pos;

            var coin = MenuTheme.Circle(chip, "Coin", new Vector2(0f, 0.5f), new Vector2(30f, 0f), 44f, coinColor);
            if (!isGold)
            {
                coin.localRotation = Quaternion.Euler(0f, 0f, 45f);
                coin.sizeDelta = new Vector2(38f, 38f);
            }

            var txt = MenuTheme.Label(chip, "0", 30, Color.white, TextAnchor.MiddleCenter);
            txt.rectTransform.offsetMin = new Vector2(56f, 4f);
            txt.rectTransform.offsetMax = new Vector2(-56f, -4f);
            if (isGold) _goldText = txt; else _gemText = txt;

            var plus = MenuTheme.BtnAt(chip, "Plus", "+", new Vector2(1f, 0.5f), new Vector2(-28f, 0f),
                new Vector2(44f, 44f), new Color(0.85f, 0.6f, 0.15f), Color.white, 32, () => onPlus());
            var b = plus.GetComponent<Button>();
            var colors = b.colors;
            colors.highlightedColor = new Color(1.2f, 1.05f, 0.6f);
            b.colors = colors;
        }

        private void RefreshCurrency()
        {
            if (_goldText != null) _goldText.text = MenuController.FormatNumber(Inventory.Gold);
            if (_gemText != null) _gemText.text = MenuController.FormatNumber(Inventory.Gems);
        }

        // ======================= left column =======================
        private void BuildLeftColumn(Transform root)
        {
            // Gifted Gear set card
            var gear = MenuTheme.Box(root, "GearCard", new Vector2(0f, 0.62f), new Vector2(0f, 0.62f),
                Vector2.zero, Vector2.zero, new Color(0.09f, 0.06f, 0.04f, 0.88f), true, true);
            gear.sizeDelta = new Vector2(300f, 120f);
            gear.anchoredPosition = new Vector2(168f, 0f);
            var gearIcon = MenuTheme.Circle(gear, "Icon", new Vector2(0f, 0.5f), new Vector2(52f, 0f), 76f,
                new Color(0.55f, 0.3f, 0.85f));
            MenuTheme.Label(gearIcon, "G", 40, Color.white, TextAnchor.MiddleCenter, true);
            MenuTheme.Label(gear, "Gifted Gear Set", 28, Color.white, TextAnchor.MiddleLeft).rectTransform.offsetMin = new Vector2(110f, 40f);
            var bar = MenuTheme.Framed(gear, "Prog", new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(112f, 22f), new Vector2(-24f, 52f), Color.black, new Color(0.25f, 0.6f, 0.3f), 2f);
            _gearText = MenuTheme.Label(bar, "3/4", 26, Color.white, TextAnchor.MiddleCenter);
            MenuTheme.Circle(gear, "Dot", new Vector2(1f, 1f), new Vector2(-14f, -14f), 22f,
                new Color(1f, 0.22f, 0.18f));

            // Feed + Mail buttons (left edge, like the reference)
            BuildSideButton(root, "Feed", "Feed", new Vector2(0f, 0.42f), new Vector2(90f, 0f),
                new Color(0.35f, 0.5f, 0.3f), false, () => Toast("Feed: pet system coming soon"));
            BuildSideButton(root, "Mail", "Mail", new Vector2(0f, 0.30f), new Vector2(90f, 0f),
                new Color(0.8f, 0.55f, 0.2f), true, () => Toast("Mail: no new messages"));
        }

        private void BuildSideButton(Transform root, string id, string label, Vector2 anchor, Vector2 pos,
            Color iconColor, bool dot, Action onClick)
        {
            var b = MenuTheme.BtnAt(root, id, "", anchor, pos, new Vector2(150f, 130f),
                new Color(0.12f, 0.08f, 0.05f, 0.85f), Color.white, 1, () => onClick());
            var icon = MenuTheme.Circle(b.transform, "Icon", new Vector2(0.5f, 0.5f), new Vector2(0f, 14f), 72f, iconColor);
            MenuTheme.Label(icon, id.Substring(0, 1), 36, Color.white, TextAnchor.MiddleCenter, true);
            MenuTheme.Label(b.transform, label, 26, Color.white, TextAnchor.MiddleCenter).rectTransform.offsetMax = new Vector2(0f, -8f);
            if (dot) MenuTheme.Circle(b.transform, "Dot", new Vector2(1f, 1f), new Vector2(-16f, -16f), 24f, new Color(1f, 0.22f, 0.18f));
        }

        // ======================= right column: events =======================
        private void BuildRightEvents(Transform root)
        {
            string[] evNames = { "Fire Crown", "Purchase", "8-Day\nSign in", "Relic Wish", "Benefit", "Merchant" };
            Color[] evColors =
            {
                new Color(0.85f, 0.3f, 0.2f), new Color(0.85f, 0.75f, 0.3f),
                new Color(0.9f, 0.5f, 0.8f), new Color(0.35f, 0.6f, 1f),
                new Color(0.9f, 0.45f, 0.2f), new Color(0.55f, 0.4f, 0.9f)
            };
            bool[] dots = { true, false, true, true, true, false };

            for (int i = 0; i < evNames.Length; i++)
            {
                int idx = i;
                float y = -100f - i * 138f;
                var btn = MenuTheme.BtnAt(root, "Ev" + i, "", new Vector2(1f, 1f),
                    new Vector2(-95f, y), new Vector2(150f, 128f), new Color(0f, 0f, 0f, 0.001f),
                    Color.white, 1, () => Toast(evNames[idx].Replace("\n", " ") + ": coming soon"));
                btn.GetComponent<Image>().raycastTarget = true;

                var icon = MenuTheme.Circle(btn.transform, "Icon", new Vector2(0.5f, 0.62f), Vector2.zero, 82f,
                    evColors[i]);
                MenuTheme.Label(icon, evNames[i].Substring(0, 1), 38, Color.white, TextAnchor.MiddleCenter, true);
                MenuTheme.Label(btn.transform, evNames[i], 24, Color.white, TextAnchor.MiddleCenter, true)
                    .rectTransform.offsetMax = new Vector2(0f, -6f);
                if (dots[i]) MenuTheme.Circle(btn.transform, "Dot", new Vector2(1f, 1f), new Vector2(-14f, -10f), 24f,
                    new Color(1f, 0.22f, 0.18f));
            }

            // timed event banner (Eerie Pumpkin Night style)
            var banner = MenuTheme.Box(root, "EventBanner", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-330f, -1000f), new Vector2(-30f, -860f), new Color(0.16f, 0.06f, 0.22f, 0.95f), true, true);
            var title = MenuTheme.Label(banner, "EERIE PUMPKIN\nNIGHT", 34, new Color(1f, 0.65f, 0.25f), TextAnchor.MiddleCenter, true);
            title.rectTransform.offsetMin = new Vector2(6f, 40f);
            _eventTimerText = MenuTheme.Label(banner, "", 30, MenuTheme.Gold, TextAnchor.MiddleCenter, true);
            _eventTimerText.rectTransform.offsetMax = new Vector2(-6f, -40f);
            MenuTheme.Circle(banner, "Dot", new Vector2(1f, 1f), new Vector2(-16f, -16f), 24f, new Color(1f, 0.22f, 0.18f));

            // daily purchase gift
            var gift = MenuTheme.BtnAt(root, "DailyPurchase", "", new Vector2(1f, 1f), new Vector2(-95f, -1030f),
                new Vector2(150f, 128f), new Color(0f, 0f, 0f, 0.001f), Color.white, 1,
                () => Toast("Daily Purchase: coming soon"));
            gift.GetComponent<Image>().raycastTarget = true;
            var giftIcon = MenuTheme.Circle(gift.transform, "Icon", new Vector2(0.5f, 0.62f), Vector2.zero, 82f,
                new Color(0.9f, 0.25f, 0.2f));
            MenuTheme.Label(giftIcon, "G", 38, Color.white, TextAnchor.MiddleCenter, true);
            MenuTheme.Label(gift.transform, "Daily\nPurchase", 24, Color.white, TextAnchor.MiddleCenter, true)
                .rectTransform.offsetMax = new Vector2(0f, -6f);
        }

        // ======================= bottom nav =======================
        private void BuildBottomNav(Transform root)
        {
            var nav = MenuTheme.Box(root, "NavBar", new Vector2(0f, 0f), new Vector2(1f, 0f),
                Vector2.zero, new Vector2(0f, 152f), new Color(0.06f, 0.04f, 0.03f, 0.95f), false);

            BuildNavButton(nav, "Missions", "Missions", new Vector2(0.03f, 0.5f), new Color(0.75f, 0.6f, 0.2f), true,
                () => OpenManagement("missions"));

            // chat strip (flex)
            var chat = MenuTheme.Box(nav, "Chat", new Vector2(0.06f, 0f), new Vector2(0.40f, 1f),
                new Vector2(20f, 14f), new Vector2(-20f, -14f), new Color(0f, 0f, 0f, 0.55f), true, true);
            var chatIcon = MenuTheme.Circle(chat, "Icon", new Vector2(0f, 0.5f), new Vector2(40f, 0f), 64f, new Color(0.2f, 0.35f, 0.6f));
            MenuTheme.Label(chatIcon, "C", 30, Color.white, TextAnchor.MiddleCenter, true);
            _chatText = MenuTheme.Label(chat, ChatLines[0], 26, new Color(0.85f, 1f, 0.75f), TextAnchor.MiddleLeft);
            _chatText.rectTransform.offsetMin = new Vector2(86f, 8f);
            _chatText.rectTransform.offsetMax = new Vector2(-70f, -8f);
            var chatBadge = MenuTheme.Circle(chat, "Badge", new Vector2(0f, 1f), new Vector2(46f, -8f), 40f, new Color(0.9f, 0.2f, 0.15f));
            MenuTheme.Label(chatBadge, "49", 24, Color.white, TextAnchor.MiddleCenter, true);

            BuildNavButton(nav, "Tech", "Tech", new Vector2(0.43f, 0.5f), new Color(0.4f, 0.35f, 0.55f), true,
                () => Toast("Tech: technology books coming soon"));
            BuildNavButton(nav, "Shop", "Shop", new Vector2(0.51f, 0.5f), new Color(0.7f, 0.45f, 0.15f), true,
                () => OpenManagement("shop"));
            BuildNavButton(nav, "Bounty", "Bounty", new Vector2(0.59f, 0.5f), new Color(0.6f, 0.5f, 0.3f), false,
                () => Toast("Bounty board: coming soon"));
            BuildNavButton(nav, "Forge", "Forge", new Vector2(0.67f, 0.5f), new Color(0.65f, 0.3f, 0.12f), false,
                () => Toast("Forge: crafting coming soon"));
            BuildNavButton(nav, "Heroes", "Heroes", new Vector2(0.75f, 0.5f), new Color(0.55f, 0.25f, 0.6f), true,
                () => OpenManagement("heroes"));
            BuildNavButton(nav, "Bag", "Bag", new Vector2(0.83f, 0.5f), new Color(0.35f, 0.5f, 0.35f), true,
                () => OpenManagement("bag"));
        }

        private void BuildNavButton(Transform nav, string id, string label, Vector2 anchor, Color iconColor,
            bool dot, Action onClick)
        {
            var b = MenuTheme.BtnAt(nav, id, "", anchor, Vector2.zero, new Vector2(120f, 140f),
                new Color(0f, 0f, 0f, 0.001f), Color.white, 1, () => onClick());
            var icon = MenuTheme.Circle(b.transform, "Icon", new Vector2(0.5f, 0.5f), new Vector2(0f, 16f), 84f, iconColor);
            MenuTheme.Label(icon, label.Substring(0, 1), 40, Color.white, TextAnchor.MiddleCenter, true);
            MenuTheme.Label(b.transform, label, 26, Color.white, TextAnchor.MiddleCenter, true)
                .rectTransform.offsetMax = new Vector2(0f, -10f);
            if (dot) MenuTheme.Circle(b.transform, "Dot", new Vector2(1f, 1f), new Vector2(-10f, -8f), 24f,
                new Color(1f, 0.22f, 0.18f));
        }

        // ======================= AFK chip + enter battle =======================
        private void BuildAfkChip(Transform root)
        {
            var chip = MenuTheme.Box(root, "AfkChip", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                Vector2.zero, Vector2.zero, new Color(0.08f, 0.05f, 0.03f, 0.88f), true, true);
            chip.sizeDelta = new Vector2(430f, 120f);
            chip.anchoredPosition = new Vector2(-380f, 190f);

            var icon = MenuTheme.Circle(chip, "Icon", new Vector2(0f, 0.5f), new Vector2(56f, 0f), 84f,
                new Color(0.85f, 0.6f, 0.15f));
            MenuTheme.Label(icon, "◆", 40, Color.white, TextAnchor.MiddleCenter, true);

            MenuTheme.Label(chip, "AFK Chest", 30, Color.white, TextAnchor.MiddleLeft).rectTransform.offsetMin = new Vector2(112f, 66f);
            _afkTimerText = MenuTheme.Label(chip, "08:00:00", 30, MenuTheme.Gold, TextAnchor.MiddleLeft, true);
            _afkTimerText.rectTransform.offsetMin = new Vector2(112f, 8f);
            _afkTimerText.rectTransform.offsetMax = new Vector2(-16f, -30f);
            _afkRewardText = MenuTheme.Label(chip, "AFK rewards are collecting", 18, AnimeTheme.Cyan, TextAnchor.MiddleLeft);
            _afkRewardText.rectTransform.offsetMin = new Vector2(112f, 42f);
            _afkRewardText.rectTransform.offsetMax = new Vector2(-16f, 68f);

            _afkClaimButton = MenuTheme.BtnAt(chip, "ClaimAfk", "CLAIM", new Vector2(1f, 0f),
                new Vector2(-58f, -44f), new Vector2(112f, 42f), AnimeTheme.Gold, AnimeTheme.Ink, 18,
                ClaimOfflineRewards);

            var track = MenuTheme.Box(chip, "Track", new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(112f, 18f), new Vector2(-24f, 44f), Color.black, false);
            _afkBar = MenuTheme.Box(track, "Fill", new Vector2(0f, 0f), new Vector2(0f, 1f),
                Vector2.zero, Vector2.zero, new Color(0.3f, 0.7f, 0.35f), false);

            // ENTER BATTLE — prominent, right-bottom like a main quest button
            MenuTheme.BtnAt(root, "EnterBattle", "ENTER BATTLE  >", new Vector2(1f, 0f),
                new Vector2(-330f, 210f), new Vector2(420f, 110f), new Color(0.8f, 0.3f, 0.14f), Color.white, 42,
                EnterBattle);
        }

        private void ClaimOfflineRewards()
        {
            if (!IdleProgression.Claim(_team)) return;
            SaveSystem.Save(GameManager.Instance != null ? GameManager.Instance.Tower : null, _team);
            Toast("Offline rewards collected");
        }

        // ======================= toast =======================
        private void BuildToast(Transform root)
        {
            var toastBox = MenuTheme.At(root, "Toast", new Vector2(0.5f, 0f), new Vector2(0f, 240f),
                new Vector2(820f, 70f), new Color(0f, 0f, 0f, 0.85f));
            _toast = MenuTheme.Label(toastBox, "", 30, Color.white, TextAnchor.MiddleCenter);
            toastBox.gameObject.SetActive(false);
        }

        private void Toast(string msg)
        {
            _toast.text = msg;
            _toast.transform.parent.gameObject.SetActive(true);
            _toastTimer = 2.2f;
        }
    }
}
