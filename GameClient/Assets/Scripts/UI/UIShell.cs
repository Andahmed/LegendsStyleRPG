using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using LegendsRPG.Core;
using LegendsRPG.Team;
using LegendsRPG.World;
using LegendsRPG.Audio;

namespace LegendsRPG.UI
{
    // ====================================================================
    // UIX — extra code-first construction helpers on top of UIBuilder.
    // ====================================================================
    public static class UIX
    {
        /// <summary>Vertical scroll view. Returns the content rect to fill.</summary>
        public static RectTransform CreateScrollView(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var root = UIBuilder.CreatePanel(parent, name, new Color(0, 0, 0, 0), anchorMin, anchorMax, offsetMin, offsetMax);

            // CreatePanel already adds an Image; configure it instead of
            // adding a second Graphic to the same GameObject.
            var img = root.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = UIBuilder.SolidSprite;
                img.color = new Color(0, 0, 0, 0);
                img.raycastTarget = true;
            }

            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            root.gameObject.AddComponent<RectMask2D>();

            var content = new GameObject("Content");
            var rt = content.AddComponent<RectTransform>();
            rt.SetParent(root, false);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 100f);
            rt.anchoredPosition = Vector2.zero;

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 10f;
            layout.padding = new RectOffset(8, 8, 8, 16);

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = rt;
            scroll.viewport = root;
            return rt;
        }

        /// <summary>Row container with horizontal layout.</summary>
        public static RectTransform CreateRow(Transform parent, string name, float height,
            int padLeft = 8, int padRight = 8, int padTop = 6, int padBottom = 6)
        {
            var row = new GameObject(name);
            var rt = row.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(0f, height);

            var img = row.AddComponent<Image>();
            img.sprite = UIBuilder.SolidSprite;
            img.color = new Color(0.13f, 0.14f, 0.19f, 0.96f);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.spacing = 8f;
            layout.padding = new RectOffset(padLeft, padRight, padTop, padBottom);

            var le = row.AddComponent<LayoutElement>();
            le.minHeight = height;
            le.preferredHeight = height;
            return rt;
        }

        public static Text CreateRowText(Transform parent, string name, string value, int size,
            Color color, TextAnchor align, bool expand = false, float width = 240f)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            if (expand) { le.flexibleWidth = 1f; le.minWidth = 0f; }
            else { le.preferredWidth = width; le.minWidth = width * 0.5f; }

            var t = go.AddComponent<Text>();
            t.font = UIBuilder.BuiltinFont;
            t.text = value;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.resizeTextForBestFit = false;
            return t;
        }

        /// <summary>Full-width button laid out inside rows.</summary>
        public static Button CreateRowButton(Transform parent, string label, UnityAction onClick,
    Color color, float height = 84f, float width = 320f, int fontSize = 30)
{
    var go = new GameObject("Btn_" + label);
    var rt = go.AddComponent<RectTransform>();
    rt.SetParent(parent, false);

    var le = go.AddComponent<LayoutElement>();
    le.preferredWidth = width;
    le.preferredHeight = height;

    var img = go.AddComponent<Image>();
    img.sprite = UIBuilder.SolidSprite;
    img.color = color;
    img.raycastTarget = true;

    var btn = go.AddComponent<Button>();
    btn.targetGraphic = img;
    var colors = btn.colors;
    colors.normalColor = Color.white;
    colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    btn.colors = colors;
    if (onClick != null) btn.onClick.AddListener(onClick);

    var lgo = new GameObject("Label");
    var lrt = lgo.AddComponent<RectTransform>();
    lrt.SetParent(rt, false);
    lrt.anchorMin = Vector2.zero;
    lrt.anchorMax = Vector2.one;
    lrt.offsetMin = Vector2.zero;
    lrt.offsetMax = Vector2.zero;

    var t = lgo.AddComponent<Text>();
    t.font = UIBuilder.BuiltinFont;
    t.text = label;
    t.fontSize = fontSize;
    t.color = Color.white;
    t.alignment = TextAnchor.MiddleCenter;
    t.raycastTarget = false;

    return btn;
}

        public static Text AddChildText(RectTransform parent, string name, string value, int size,
            Color color, TextAnchor align, bool expand = false, float width = 240f)
        {
            return CreateRowText(parent, name, value, size, color, align, expand, width);
        }

        /// <summary>Thin progress bar that lives inside a row layout.</summary>
        public static Image CreateLayoutBar(Transform parent, string name, Color color, float width, float height)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);

            var bg = go.AddComponent<Image>();
            bg.sprite = UIBuilder.SolidSprite;
            bg.color = new Color(0f, 0f, 0f, 0.65f);

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;

            var fillGo = new GameObject("Fill");
            var frt = fillGo.AddComponent<RectTransform>();
            frt.SetParent(rt, false);
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(0f, 1f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.sizeDelta = new Vector2(width, 0f);
            frt.anchoredPosition = Vector2.zero;

            var fill = fillGo.AddComponent<Image>();
            fill.sprite = UIBuilder.SolidSprite;
            fill.color = color;
            fill.raycastTarget = false;

            return fill;
        }

        public static string TimeShort(TimeSpan t)
        {
            if (t.TotalDays >= 1) return string.Format("{0}d {1:00}h", (int)t.TotalDays, t.Hours);
            if (t.TotalHours >= 1) return string.Format("{0}h {1:00}m", (int)t.TotalHours, t.Minutes);
            return string.Format("{0:00}:{1:00}", t.Minutes, t.Seconds);
        }
    }

    // ====================================================================
    // SCREEN BASE — full-screen dimmed panel with title + close button.
    // ====================================================================
    public abstract class ScreenBase
    {
        protected RectTransform Root;
        protected RectTransform Backdrop;
        protected Text TitleText;
        protected RectTransform Content;
        protected UIManager Manager;

        public bool IsOpen { get; protected set; }

        public virtual void Build(UIManager manager, Transform canvasParent, string title)
        {
            Manager = manager;
            Backdrop = UIBuilder.CreatePanel(canvasParent, "ScreenBackdrop_" + GetType().Name,
                new Color(0.04f, 0.05f, 0.09f, 0.97f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Backdrop.gameObject.SetActive(false);

            Root = UIBuilder.CreateSafeArea(canvasParent, "ScreenSafeArea_" + GetType().Name);
            Root.gameObject.SetActive(false);

            TitleText = UIBuilder.CreateText(Root, "Title", title, 52,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f),
                new Vector2(0f, -95f), new Vector2(900f, 70f));

            var close = UIBuilder.CreateButton(Root, "CloseBtn", "X", new Vector2(84f, 84f),
                new Vector2(1f, 1f), new Vector2(-60f, -95f), new Color(0.55f, 0.2f, 0.2f, 0.95f),
                Color.white, 40, () => Manager.CloseScreen());

            Content = UIX.CreateScrollView(Root, "Scroll",
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(30f, 40f), new Vector2(-30f, -160f));
        }

        public virtual void Open()
        {
            IsOpen = true;
            if (Backdrop != null) Backdrop.gameObject.SetActive(true);
            Root.gameObject.SetActive(true);
            Root.SetAsLastSibling();
            Rebuild();
        }

        public virtual void Close()
        {
            IsOpen = false;
            Root.gameObject.SetActive(false);
            if (Backdrop != null) Backdrop.gameObject.SetActive(false);
        }

        public virtual void Tick() { }

        /// <summary>Refills the scroll content. Called on every Open().</summary>
        public abstract void Rebuild();

        protected void ClearContent()
        {
            for (int i = Content.childCount - 1; i >= 0; i--)
            {
                DestroySafe(Content.GetChild(i).gameObject);
            }
        }

        private static void DestroySafe(GameObject go)
        {
            if (go == null) return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(go);
            else
                UnityEngine.Object.DestroyImmediate(go);
        }
    }

    // ====================================================================
    // UI MANAGER — owns all screens, city HUD, battle-mode switching,
    // battle result popup, notifications and 3D click routing.
    // ====================================================================
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Transform _screenCanvas;
        private CityHUD _cityHud;
        private readonly Dictionary<string, ScreenBase> _screens = new Dictionary<string, ScreenBase>();
        private ScreenBase _current;
        private Text _notifyText;
        private float _notifyTimer;

        // battle result popup
        private RectTransform _resultPanel;
        private Text _resultTitle;
        private Text _resultLines;
        private Button _resultHomeBtn;
        private Button _resultRetryBtn;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void BuildAll()
        {
            // Prevent duplicate canvases and UI event subscriptions.
            if (_screenCanvas != null) return;

            var canvas = UIBuilder.CreateCanvas("ScreenCanvas");
            canvas.sortingOrder = 10;  // full-screen panels above everything
            _screenCanvas = canvas.transform;

            BuildNotifier(_screenCanvas);

            // register every screen
            Register(new SummonScreen(), "summon", "Summoning Circle");
            Register(new HeroScreen(), "heroes", "Heroes & Formation");
            Register(new HeroDetailScreen(), "hero_detail", "Hero Details");
            Register(new CampaignScreen(), "campaign", "Story Campaign");
            Register(new TowerScreen(), "tower", "Tower of Trials");
            Register(new DungeonScreen(), "dungeon", "Dungeons");
            Register(new ArenaScreen(), "arena", "PVP Arena");
            Register(new ContinentGateScreen(), "continent", "Continent Gate");
            Register(new RankScreen(), "rank", "Leaderboard");
            Register(new GuildScreen(), "guild", "Guild");
            Register(new QuestScreen(), "quests", "Daily Quests");
            Register(new ShopScreen(), "shop", "Shop");
            Register(new SignInScreen(), "signin", "Sign-in Rewards");
            Register(new EventsScreen(), "events", "Limited Event");
            Register(new OffersScreen(), "offers", "Special Offers");
            Register(new MerchantScreen(), "merchant", "Wandering Merchant");
            Register(new ChatScreen(), "chat", "Chat");
            Register(new TechScreen(), "tech", "Technologies");
            Register(new ForgeScreen(), "forge", "Forge");
            Register(new InventoryScreen(), "inventory", "Inventory");
            Register(new MineScreen(), "mine", "Gold Mine");
            Register(new BeastScreen(), "beast", "Beast Den");
            Register(new ProfileScreen(), "profile", "Player Profile");
            Register(new SettingsScreen(), "settings", "Settings");
            Register(new AFKPopup(), "afk", "AFK Rewards");

            // city HUD on its own canvas above everything
            var hudGo = new GameObject("CityHUD");
            hudGo.transform.SetParent(_screenCanvas.parent, false);
            _cityHud = hudGo.AddComponent<CityHUD>();
            _cityHud.Build(this);

            BuildResultPanel(_screenCanvas);
        }

        private void Register(ScreenBase screen, string key, string title)
        {
            screen.Build(this, _screenCanvas, title);
            _screens[key] = screen;
        }

        // ================= navigation =================
        public void OpenCityBuilding(string buildingId)
        {
            switch (buildingId)
            {
                case "afk": Open("afk"); break;
                case "summon": Open("summon"); break;
                case "guild": Open("guild"); break;
                case "arena": Open("arena"); break;
                case "dungeon": Open("dungeon"); break;
                case "continent": Open("continent"); break;
                case "rank": Open("rank"); break;
                case "mine": Open("mine"); break;
                case "beast": Open("beast"); break;
                case "shop": Open("shop"); break;
                case "forge": Open("forge"); break;
                default: Open(buildingId); break;
            }
        }

        public void OpenGuildTab(string tab)
        {
            ScreenBase screen;
            var gs = _screens.TryGetValue("guild", out screen) ? screen as GuildScreen : null;
            if (gs == null) return;

            gs.RequestTab(tab);
            Open("guild");
        }

        public void Open(string key)
        {
            ScreenBase s;
            if (!_screens.TryGetValue(key, out s)) return;

            if (_current != null && _current != s) _current.Close();
            _current = s;
            s.Open();
            AudioSystem.Play("Open");
        }

        public void OpenHeroDetail(string heroId)
        {
            ScreenBase screen;
            var hd = _screens.TryGetValue("hero_detail", out screen) ? screen as HeroDetailScreen : null;
            if (hd == null) return;

            hd.SetHero(heroId);
            Open("hero_detail");
        }

        public void CloseScreen()
        {
            if (_current != null)
            {
                _current.Close();
                _current = null;
            }
            AudioSystem.Play("UI");
        }

        public void CloseAll()
        {
            if (_current != null)
            {
                _current.Close();
                _current = null;
            }
        }

        public ScreenBase Get(string key)
        {
            ScreenBase s;
            return _screens.TryGetValue(key, out s) ? s : null;
        }

        // ================= city <-> battle =================
        public void EnterBattleMode(string title)
        {
            CloseAll();
            if (_cityHud != null) _cityHud.SetVisible(false);
            if (GameManager.Instance != null && GameManager.Instance.Hud != null)
            {
                GameManager.Instance.Hud.SetBattleHudVisible(true, title);
            }
        }

        public void ExitBattleMode()
        {
            BattleSession.ReturnToCity();
            if (_cityHud != null) _cityHud.SetVisible(true);
            if (GameManager.Instance != null && GameManager.Instance.Hud != null)
            {
                GameManager.Instance.Hud.SetBattleHudVisible(false, "");
            }
            if (GameManager.Instance != null) GameManager.Instance.SetState(GameManager.GameState.City);
        }

        // ================= battle result popup =================
        private void BuildResultPanel(Transform parent)
        {
            _resultPanel = UIBuilder.CreatePanel(parent, "BattleResult",
                new Color(0f, 0f, 0f, 0.86f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _resultPanel.SetAsLastSibling();
            _resultPanel.gameObject.SetActive(false);

            _resultTitle = UIBuilder.CreateText(_resultPanel, "ResultTitle", "VICTORY!", 72,
                new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 260f), new Vector2(980f, 100f));

            _resultLines = UIBuilder.CreateText(_resultPanel, "ResultLines", "", 38,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 90f), new Vector2(980f, 280f));

            _resultHomeBtn = UIBuilder.CreateButton(_resultPanel, "HomeBtn", "HOME", new Vector2(380f, 120f),
                new Vector2(0.5f, 0.5f), new Vector2(-210f, -160f), new Color(0.16f, 0.5f, 0.3f, 0.98f),
                Color.white, 40, () =>
                {
                    if (_resultPanel != null) _resultPanel.gameObject.SetActive(false);
                    ExitBattleMode();
                });

            _resultRetryBtn = UIBuilder.CreateButton(_resultPanel, "RetryBtn", "RETRY", new Vector2(380f, 120f),
                new Vector2(0.5f, 0.5f), new Vector2(210f, -160f), new Color(0.2f, 0.4f, 0.7f, 0.98f),
                Color.white, 40, () =>
                {
                    _resultPanel.gameObject.SetActive(false);
                    BattleSession.Retry();
                });
        }

        public void ShowBattleResult(bool win, string title, string[] lines)
        {
            if (_resultPanel == null || _resultTitle == null || _resultLines == null ||
                _resultRetryBtn == null) return;

            _resultTitle.text = win ? "VICTORY!" : "DEFEAT";
            _resultTitle.color = win ? new Color(1f, 0.85f, 0.3f) : new Color(0.95f, 0.4f, 0.35f);

            string resultDetails = lines == null || lines.Length == 0
                ? string.Empty
                : string.Join("\n", lines);
            _resultLines.text = (string.IsNullOrEmpty(title) ? "" : title + "\n\n") + resultDetails;
            _resultRetryBtn.gameObject.SetActive(true);
            _resultPanel.gameObject.SetActive(true);
            _resultPanel.SetAsLastSibling();
        }

        // ================= notifications =================
        private void BuildNotifier(Transform parent)
        {
            _notifyText = UIBuilder.CreateText(parent, "CityNotification", "", 36,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -330f),
                new Vector2(1000f, 60f));
        }

        public void Notify(string msg)
        {
            if (_notifyText == null) return;
            _notifyText.text = msg;
            _notifyTimer = 2.6f;
        }

        private void Update()
        {
            if (_notifyTimer > 0f)
            {
                _notifyTimer -= Time.unscaledDeltaTime;
                if (_notifyTimer <= 0f && _notifyText != null) _notifyText.text = "";
            }

            foreach (var kv in _screens)
            {
                if (kv.Value.IsOpen) kv.Value.Tick();
            }

            if (_cityHud != null) _cityHud.Tick();

            // living world: chat bots + wish wall fulfillment
            ChatSystem.Tick();
            Social.GuildSystem.TickWishes();
        }
    }

    // ====================================================================
    // CITY HUD — top player bar, event buttons, bottom navigation bar and
    // the AFK chest bubble. Matches the reference layout.
    // ====================================================================
    public class CityHUD : MonoBehaviour
    {
        private UIManager _manager;

        private Text _nameText;
        private Text _vipText;
        private Text _levelText;
        private Text _powerText;
        private Text _goldText;
        private Text _diamondText;
        private Text _afkText;
        private Image _afkFill;
        private Text _signinDot;
        private Text _questDot;
        private Text _giftDot;
        private Text _eventTimerText;
        private RectTransform _root;

        private static List<Characters.HeroData> _rosterCache;
        private static Dictionary<string, Characters.HeroData> _rosterMap;
        private static readonly List<Characters.HeroData> _teamDataCache = new List<Characters.HeroData>();

        public void Build(UIManager manager)
        {
            _manager = manager;
            var canvas = UIBuilder.CreateCanvas("CityHUDCanvas");
            canvas.sortingOrder = 5;   // screens (10) render above the city HUD
            _root = UIBuilder.CreateSafeArea(canvas.transform, "CityHUDSafeArea");

            BuildTopBar();
            BuildEventColumn();
            BuildSideButtons();
            BuildBottomNav();
            BuildAFKBubble();

            Economy.OnChanged += RefreshCurrencies;
            PlayerProfile.OnChanged += RefreshHeader;
        }

        private void OnDestroy()
        {
            Economy.OnChanged -= RefreshCurrencies;
            PlayerProfile.OnChanged -= RefreshHeader;
        }

        public void SetVisible(bool visible)
        {
            _root.gameObject.SetActive(visible);
        }

        // ---------------- top bar ----------------
        private void BuildTopBar()
        {
            var bar = UIBuilder.CreatePanel(_root, "TopBar", new Color(0.05f, 0.06f, 0.1f, 0.85f),
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -170f), new Vector2(0f, 0f));

            // avatar block (tap → profile)
            var avatar = UIBuilder.CreateButton(bar, "AvatarBtn", "", new Vector2(130f, 130f),
                new Vector2(0f, 1f), new Vector2(85f, -95f), new Color(0.25f, 0.3f, 0.5f, 0.95f),
                Color.white, 44, () => _manager.Open("profile"));
            UIBuilder.CreateText(avatar.transform, "AvatarIcon", "★", 42,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130f, 60f));

            // Player identity uses separate, non-overlapping fields inside the safe-area top bar.
            _nameText = UIBuilder.CreateText(bar, "PlayerName", "", 32, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(170f, -58f), new Vector2(470f, 44f));
            _vipText = UIBuilder.CreateText(bar, "Vip", "", 22, new Color(1f, 0.8f, 0.25f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(170f, -108f), new Vector2(145f, 34f));
            _levelText = UIBuilder.CreateText(bar, "PlayerLevel", "", 22, new Color(0.65f, 0.88f, 1f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(325f, -108f), new Vector2(110f, 34f));
            _powerText = UIBuilder.CreateText(bar, "Power", "", 22, new Color(0.6f, 0.9f, 1f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(445f, -108f), new Vector2(250f, 34f));

            // currency pills (right)
            var goldPill = UIBuilder.CreatePanel(bar, "GoldPill", new Color(0.1f, 0.09f, 0.05f, 0.9f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-370f, -110f), new Vector2(-20f, -40f));
            var goldIcon = UIBuilder.CreateFreeRect(goldPill, "GoldIcon", new Vector2(46f, 46f),
                new Vector2(0f, 0.5f), new Vector2(32f, 0f));
            var goldIconImg = goldIcon.gameObject.AddComponent<Image>();
            goldIconImg.sprite = UIBuilder.SolidSprite;
            goldIconImg.color = new Color(0.95f, 0.75f, 0.2f);
            _goldText = UIBuilder.CreateText(goldPill, "GoldAmount", "0", 30, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(88f, 0f), new Vector2(230f, 50f));
            UIBuilder.CreateButton(goldPill, "GoldPlus", "+", new Vector2(56f, 56f),
                new Vector2(1f, 0.5f), new Vector2(-34f, 0f), new Color(0.2f, 0.55f, 0.25f, 0.95f),
                Color.white, 36, () => _manager.Open("shop"));

            var diaPill = UIBuilder.CreatePanel(bar, "DiaPill", new Color(0.08f, 0.09f, 0.12f, 0.9f),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-370f, -178f), new Vector2(-20f, -108f));
            var diaIcon = UIBuilder.CreateFreeRect(diaPill, "DiaIcon", new Vector2(46f, 46f),
                new Vector2(0f, 0.5f), new Vector2(32f, 0f));
            var diaIconImg = diaIcon.gameObject.AddComponent<Image>();
            diaIconImg.sprite = UIBuilder.SolidSprite;
            diaIconImg.color = new Color(0.4f, 0.8f, 1f);
            _diamondText = UIBuilder.CreateText(diaPill, "DiaAmount", "0", 30, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(88f, 0f), new Vector2(230f, 50f));
            UIBuilder.CreateButton(diaPill, "DiaPlus", "+", new Vector2(56f, 56f),
                new Vector2(1f, 0.5f), new Vector2(-34f, 0f), new Color(0.2f, 0.55f, 0.25f, 0.95f),
                Color.white, 36, () => _manager.Open("shop"));
        }

        // ---------------- event column (right side) ----------------
        private void BuildEventColumn()
        {
            var col = UIBuilder.CreateFreeRect(_root, "EventColumn", new Vector2(120f, 560f),
                new Vector2(1f, 0.5f), new Vector2(-65f, 130f));

            MakeEventButton(col, "SIGN-IN", new Color(0.85f, 0.45f, 0.15f, 0.95f), new Vector2(0f, 0f),
                () => _manager.Open("signin"), out _signinDot);
            MakeEventButton(col, "OFFERS", new Color(0.75f, 0.25f, 0.5f, 0.95f), new Vector2(0f, -130f),
                () => _manager.Open("offers"), out _);
            MakeEventButton(col, "EVENT", new Color(0.2f, 0.55f, 0.8f, 0.95f), new Vector2(0f, -260f),
                () => _manager.Open("events"), out _);
            MakeEventButton(col, "MERCHANT", new Color(0.4f, 0.6f, 0.2f, 0.95f), new Vector2(0f, -390f),
                () => _manager.Open("merchant"), out _giftDot);

            _eventTimerText = UIBuilder.CreateText(col, "EventTimer", "", 24, new Color(1f, 0.85f, 0.4f),
                TextAnchor.MiddleCenter, new Vector2(0f, 0f), new Vector2(60f, -486f), new Vector2(160f, 40f));
        }

        private void MakeEventButton(Transform parent, string label, Color color, Vector2 pos,
            UnityEngine.Events.UnityAction onClick, out Text dot)
        {
            var btn = UIBuilder.CreateButton(parent, "Evt_" + label, label, new Vector2(120f, 120f),
                new Vector2(0f, 0f), pos, color, Color.white, 24, onClick);
            dot = UIBuilder.CreateText(btn.transform, "Dot", "", 34, new Color(1f, 0.25f, 0.2f),
                TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-8f, -6f), new Vector2(50f, 50f));
        }

        // ---------------- right side buttons ----------------
        private void BuildSideButtons()
        {
            var col = UIBuilder.CreateFreeRect(_root, "SideColumn", new Vector2(120f, 300f),
                new Vector2(0f, 0.5f), new Vector2(65f, 150f));

            UIBuilder.CreateButton(col, "SettingsBtn", "SET", new Vector2(110f, 110f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Color(0.3f, 0.32f, 0.4f, 0.95f),
                Color.white, 30, () => _manager.Open("settings"));
            UIBuilder.CreateButton(col, "ProfileBtn", "ME", new Vector2(110f, 110f),
                new Vector2(0f, 0f), new Vector2(0f, -125f), new Color(0.3f, 0.4f, 0.55f, 0.95f),
                Color.white, 30, () => _manager.Open("profile"));
            UIBuilder.CreateButton(col, "RankBtn", "RANK", new Vector2(110f, 110f),
                new Vector2(0f, 0f), new Vector2(0f, -250f), new Color(0.5f, 0.42f, 0.2f, 0.95f),
                Color.white, 26, () => _manager.Open("rank"));
        }

        // ---------------- bottom navigation ----------------
        private void BuildBottomNav()
        {
            var bar = UIBuilder.CreatePanel(_root, "BottomNav", new Color(0.05f, 0.06f, 0.1f, 0.92f),
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 190f));

            string[] labels = { "QUEST", "CHAT", "TECH", "SHOP", "REWARD", "FORGE", "HERO", "BAG" };
            Color[] colors =
            {
                new Color(0.25f, 0.5f, 0.3f, 0.95f), new Color(0.3f, 0.4f, 0.6f, 0.95f),
                new Color(0.5f, 0.35f, 0.6f, 0.95f), new Color(0.6f, 0.45f, 0.15f, 0.95f),
                new Color(0.75f, 0.35f, 0.2f, 0.95f), new Color(0.55f, 0.3f, 0.2f, 0.95f),
                new Color(0.2f, 0.5f, 0.55f, 0.95f), new Color(0.4f, 0.4f, 0.45f, 0.95f)
            };
            UnityEngine.Events.UnityAction[] actions =
            {
                () => _manager.Open("quests"), () => _manager.Open("chat"),
                () => _manager.Open("tech"), () => _manager.Open("shop"),
                () => _manager.Open("signin"), () => _manager.Open("forge"),
                () => _manager.Open("heroes"), () => _manager.Open("inventory")
            };

            for (int i = 0; i < labels.Length; i++)
            {
                float xAnchor = (i + 0.5f) / labels.Length;
                var btn = UIBuilder.CreateButton(bar, "Nav_" + labels[i], labels[i], new Vector2(112f, 118f),
                    new Vector2(xAnchor, 0f), new Vector2(0f, 118f), colors[i], Color.white, 22, actions[i]);
                if (labels[i] == "QUEST")
                {
                    _questDot = UIBuilder.CreateText(btn.transform, "Dot", "", 32, new Color(1f, 0.25f, 0.2f),
                        TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-6f, -4f), new Vector2(50f, 50f));
                }
            }
        }

        // ---------------- AFK chest bubble ----------------
        private void BuildAFKBubble()
        {
            // Keep AFK treasure centered above the bottom navigation; the event rail stays clear on the right.
            var panel = UIBuilder.CreateFreeRect(_root, "AFKBubble", new Vector2(420f, 170f),
                new Vector2(0.5f, 0f), new Vector2(0f, 300f));
            var bg = panel.gameObject.AddComponent<Image>();
            bg.sprite = UIBuilder.SolidSprite;
            bg.color = new Color(0.12f, 0.1f, 0.05f, 0.92f);

            var btn = panel.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => _manager.Open("afk"));

            var title = UIBuilder.CreateText(panel, "AFKTitle", "AFK TREASURE", 28, new Color(1f, 0.85f, 0.35f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(400f, 40f));

            _afkText = UIBuilder.CreateText(panel, "AFKAmount", "", 26, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(390f, 50f));

            var capBg = UIBuilder.CreateFreeRect(panel, "CapBg", new Vector2(380f, 16f),
                new Vector2(0.5f, 0f), new Vector2(0f, 20f));
            var capBgImg = capBg.gameObject.AddComponent<Image>();
            capBgImg.sprite = UIBuilder.SolidSprite;
            capBgImg.color = new Color(0f, 0f, 0f, 0.7f);

            var fillGo = new GameObject("CapFill");
            var frt = fillGo.AddComponent<RectTransform>();
            frt.SetParent(capBg, false);
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(0f, 1f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.sizeDelta = new Vector2(380f, 0f);
            frt.anchoredPosition = Vector2.zero;
            _afkFill = fillGo.AddComponent<Image>();
            _afkFill.sprite = UIBuilder.SolidSprite;
            _afkFill.color = new Color(1f, 0.8f, 0.3f);
            _afkFill.raycastTarget = false;
        }

        // ---------------- refresh ----------------
        public void Tick()
        {
            if (_root == null || !_root.gameObject.activeSelf) return;

            var pending = AFKReward.Pending();
            _afkText.text = Economy.Format(pending.gold) + " gold  •  " + pending.diamonds + " dia  •  " +
                            Economy.Format(pending.exp) + " xp";
            _afkFill.rectTransform.sizeDelta = new Vector2(380f * AFKReward.CapRatio(), 0f);

            _signinDot.text = SignInSystem.CanClaimToday() ? "!" : "";
            _questDot.text = QuestSystem.ClaimableCount() > 0 ? "!" : "";
            _giftDot.text = MerchantSystem.TimeToRefresh().TotalMinutes < 30 ? "!" : "";
            _eventTimerText.text = "Ends in " + UIX.TimeShort(LimitedEvent.TimeLeft);

            RefreshCurrencies();
            RefreshHeader();
        }

        private void RefreshCurrencies()
        {
            if (_goldText != null) _goldText.text = Economy.Format(Economy.Gold);
            if (_diamondText != null) _diamondText.text = Economy.Format(Economy.Diamonds);
        }

        private void RefreshHeader()
        {
            if (_nameText != null) _nameText.text = PlayerProfile.Name;
            if (_vipText != null) _vipText.text = "VIP " + Economy.VipLevel;
            if (_levelText != null) _levelText.text = "Lv." + PlayerProfile.Level;

            long power = 0;
            var team = TeamManager.InstanceRef;
            if (team != null && team.Heroes.Count > 0)
            {
                if (_rosterCache == null) _rosterCache = Characters.HeroDatabase.CreateDefaultRoster();
                if (_rosterMap == null)
                {
                    _rosterMap = new Dictionary<string, Characters.HeroData>();
                    foreach (var h in _rosterCache) _rosterMap[h.heroId] = h;
                }
                _teamDataCache.Clear();
                foreach (var hero in team.Heroes)
                {
                    if (hero != null && _rosterMap.ContainsKey(hero.Data.heroId)) _teamDataCache.Add(_rosterMap[hero.Data.heroId]);
                }
                power = HeroProgress.TeamPower(_teamDataCache);
            }
            if (_powerText != null) _powerText.text = "Power " + Economy.Format(power);
        }
    }
}
