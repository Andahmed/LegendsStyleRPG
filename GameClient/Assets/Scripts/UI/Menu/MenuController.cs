using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;
using LegendsRPG.Team;
using LegendsRPG.World;

namespace LegendsRPG.UI
{
    /// <summary>
    /// Main menu + management screens (Home, Heroes, Bag, Shop, Missions).
    /// Landscape layout, 1920x1080 reference. While the menu is open the
    /// battle is paused (Time.timeScale = 0).
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance { get; private set; }
        public bool IsOpen { get; private set; }

        private TeamManager _team;
        private HUDController _hud;
        private Canvas _canvas;
        private RectTransform _screenHost;
        private Text _titleText, _goldText, _gemText, _toast;
        private Button _backBtn;
        private float _toastTimer;

        private struct Entry { public string title; public Action<RectTransform> build; }
        private readonly List<Entry> _history = new List<Entry>();

        // ======================= lifecycle =======================
        public void Build(TeamManager team, HUDController hud)
        {
            Instance = this;
            _team = team;
            _hud = hud;

            _canvas = UIBuilder.CreateCanvas("MenuCanvas");
            _canvas.sortingOrder = 50;
            var root = _canvas.transform;

            _screenHost = MenuTheme.Stretch(root, "Screens", Color.clear);

            // top bar (always on top of screens)
            var top = MenuTheme.Box(root, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -110f), Vector2.zero, new Color(0.08f, 0.05f, 0.03f, 0.92f), false);

            _backBtn = MenuTheme.BtnAt(top, "Back", "<  BACK", new Vector2(0f, 0.5f), new Vector2(120f, 0f),
                new Vector2(200f, 72f), new Color(0.55f, 0.32f, 0.15f), Color.white, 30, GoBack);

            _titleText = MenuTheme.Label(top, "", 44, MenuTheme.Gold, TextAnchor.MiddleCenter, true);

            var gold = MenuTheme.At(top, "GoldChip", new Vector2(1f, 0.5f), new Vector2(-480f, 0f), new Vector2(210f, 56f), new Color(0f, 0f, 0f, 0.55f));
            _goldText = MenuTheme.Label(gold, "0", 30, new Color(1f, 0.9f, 0.5f), TextAnchor.MiddleRight);
            MenuTheme.Circle(gold, "GoldCoin", new Vector2(0f, 0.5f), new Vector2(28f, 0f), 36f, MenuTheme.Gold);

            var gem = MenuTheme.At(top, "GemChip", new Vector2(1f, 0.5f), new Vector2(-230f, 0f), new Vector2(210f, 56f), new Color(0f, 0f, 0f, 0.55f));
            _gemText = MenuTheme.Label(gem, "0", 30, new Color(0.7f, 0.9f, 1f), TextAnchor.MiddleRight);
            var gi = MenuTheme.Circle(gem, "Gem", new Vector2(0f, 0.5f), new Vector2(28f, 0f), 32f, new Color(0.4f, 0.8f, 1f));
            gi.localRotation = Quaternion.Euler(0f, 0f, 45f);

            // toast
            var toastBox = MenuTheme.At(root, "Toast", new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(720f, 70f), new Color(0f, 0f, 0f, 0.85f));
            _toast = MenuTheme.Label(toastBox, "", 32, Color.white, TextAnchor.MiddleCenter);
            toastBox.gameObject.SetActive(false);

            Inventory.Changed += RefreshCurrency;
            DailyMissions.Hook(team);
        }

        private void OnDestroy()
        {
            Inventory.Changed -= RefreshCurrency;
            if (Instance == this) Instance = null;
        }

        public void Open()
        {
            IsOpen = true;
            Inventory.EnsureToday();
            _canvas.gameObject.SetActive(true);
            Time.timeScale = 0f;
            if (_hud != null) _hud.SetVisible(false);
            _history.Clear();
            Push("LEGENDS TOWER", BuildHome);
            RefreshCurrency();
        }

        public void CloseToBattle()
        {
            IsOpen = false;
            _canvas.gameObject.SetActive(false);
            Time.timeScale = 1f;
            if (_hud != null) _hud.SetVisible(true);
            SaveSystem.Save(GameManager.Instance != null ? GameManager.Instance.Tower : null, _team);
        }

        /// <summary>Jump straight to a management screen from the hub (v0.3).</summary>
        public void ShowScreen(string id)
        {
            switch (id)
            {
                case "missions": Push("DAILY MISSIONS", BuildMissions); break;
                case "heroes": Push("HEROES", BuildHeroes); break;
                case "bag": Push("BAG", BuildBag); break;
                case "shop": Push("SHOP", BuildShop); break;
                case "summon": Push("SUMMON", BuildSummon); break;
                default: break;
            }
        }

        // ======================= navigation =======================
        private void Push(string title, Action<RectTransform> build)
        {
            _history.Add(new Entry { title = title, build = build });
            Show(_history[_history.Count - 1]);
        }

        private void GoBack()
        {
            if (_history.Count <= 1) return;
            _history.RemoveAt(_history.Count - 1);
            Show(_history[_history.Count - 1]);
        }

        private void Show(Entry e)
        {
            for (int i = _screenHost.childCount - 1; i >= 0; i--) Destroy(_screenHost.GetChild(i).gameObject);
            var page = MenuTheme.Stretch(_screenHost, "Page", MenuTheme.Wood);
            // content starts below the top bar
            var body = MenuTheme.Box(page, "Body", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -110f), Color.clear, false);
            _titleText.text = e.title;
            _backBtn.gameObject.SetActive(_history.Count > 1);
            e.build(body);
            RefreshCurrency();
        }

        private void RefreshCurrency()
        {
            if (_goldText != null) _goldText.text = FormatNumber(Inventory.Gold);
            if (_gemText != null) _gemText.text = FormatNumber(Inventory.Gems);
        }

        public static string FormatNumber(int n)
        {
            if (n >= 1000000) return (n / 1000000f).ToString("0.#") + "M";
            if (n >= 10000) return (n / 1000f).ToString("0.#") + "K";
            return n.ToString();
        }

        /// <summary>Public hooks for the sub-screens (SummonScreen v0.4).</summary>
        public void RefreshScreens() { Refresh(); }
        public void ShowToast(string msg) { Toast(msg); }

        private void BuildSummon(RectTransform body)
        {
            SummonScreen.Build(this, body);
        }

        private void Toast(string msg)
        {
            _toast.text = msg;
            _toast.transform.parent.gameObject.SetActive(true);
            _toastTimer = 2f;
        }

        private void Update()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.unscaledDeltaTime;
                if (_toastTimer <= 0f) _toast.transform.parent.gameObject.SetActive(false);
            }
        }

        // ======================= HOME =======================
        private void BuildHome(RectTransform body)
        {
            // soft lighter band behind the content
            MenuTheme.Box(body, "Band", new Vector2(0f, 0.18f), new Vector2(1f, 0.92f), Vector2.zero, Vector2.zero,
                new Color(1f, 0.8f, 0.4f, 0.05f), false);

            var tower = GameManager.Instance != null ? GameManager.Instance.Tower : null;
            int floor = tower != null ? tower.CurrentFloor : 1;
            string diff = tower != null ? TowerSystem.DifficultyName(tower.Difficulty) : "Easy";

            // center tower card
            var card = MenuTheme.Framed(body, "TowerCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-420f, -60f), new Vector2(420f, 390f), MenuTheme.Gold, new Color(0.12f, 0.08f, 0.06f, 0.95f), 5f);
            MenuTheme.Label(card, "TOWER OF TRIALS", 54, MenuTheme.Gold, TextAnchor.UpperCenter, true).rectTransform.offsetMin = new Vector2(6f, 200f);
            var sub = MenuTheme.Label(card, diff.ToUpper() + "  -  FLOOR " + floor, 40, Color.white, TextAnchor.MiddleCenter, true);
            sub.rectTransform.offsetMin = new Vector2(6f, 120f); sub.rectTransform.offsetMax = new Vector2(-6f, -90f);
            MenuTheme.BtnAt(card, "Battle", "ENTER BATTLE", new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(520f, 110f),
                new Color(0.80f, 0.30f, 0.14f), Color.white, 46, CloseToBattle);

            // lineup row
            var lineupLabel = MenuTheme.At(body, "LineupLabel", new Vector2(0.5f, 0.5f), new Vector2(0f, -95f), new Vector2(500f, 40f), Color.clear);
            MenuTheme.Label(lineupLabel, "MY LINEUP", 30, MenuTheme.ParchmentDark, TextAnchor.MiddleCenter);
            var lineup = MenuTheme.At(body, "Lineup", new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(1000f, 130f), Color.clear);
            for (int i = 0; i < _team.Heroes.Count; i++)
            {
                var h = _team.Heroes[i];
                MenuTheme.HeroPortrait(lineup, new Vector2(0.5f, 0.5f), new Vector2((i - (_team.Heroes.Count - 1) / 2f) * 160f, 0f), 120f, h.Data, h.Stats.Level);
            }

            // bottom nav
            var nav = MenuTheme.Box(body, "Nav", new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 190f),
                new Color(0.07f, 0.04f, 0.03f, 0.95f), false);
            string[] names = { "MISSIONS", "HEROES", "SUMMON", "BAG", "SHOP", "FORGE" };
            Action[] acts =
            {
                () => Push("DAILY MISSIONS", BuildMissions),
                () => Push("HEROES", BuildHeroes),
                () => Push("SUMMON", BuildSummon),
                () => Push("BAG", BuildBag),
                () => Push("SHOP", BuildShop),
                () => Toast("Forge: coming in the equipment update"),
            };
            for (int i = 0; i < names.Length; i++)
            {
                float x = (i + 0.5f) / names.Length;
                var act = acts[i];
                var b = MenuTheme.Btn(nav, names[i], names[i], new Vector2(x, 0.5f), new Vector2(x, 0.5f), Vector2.zero, Vector2.zero,
                    i == 5 ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.45f, 0.28f, 0.14f), Color.white, 34, () => act());
                var rt = (RectTransform)b.transform;
                rt.sizeDelta = new Vector2(280f, 120f);
                rt.anchoredPosition = Vector2.zero;
                if (i == 0 && MissionClaimable()) Dot(rt);
            }
        }

        private static bool MissionClaimable()
        {
            for (int i = 0; i < DailyMissions.All.Length; i++) if (DailyMissions.CanClaim(i)) return true;
            return false;
        }

        private static void Dot(RectTransform parent)
        {
            MenuTheme.Circle(parent, "Dot", new Vector2(1f, 1f), new Vector2(-8f, -8f), 28f, MenuTheme.Red);
        }

        // ======================= HEROES =======================
        private int _heroSel;
        private int _heroFilter = -1;   // -1 = all, else (int)ElementType

        private void BuildHeroes(RectTransform body)
        {
            var s = SaveSystem.Current;
            var roster = HeroDatabase.AllHeroes;

            // collected counter (reference: "114/265")
            int owned = 0;
            for (int i = 0; i < roster.Count; i++) if (s.unlockedHeroIds.Contains(roster[i].heroId)) owned++;
            var countBar = MenuTheme.Box(body, "Count", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.55f), true, false);
            countBar.sizeDelta = new Vector2(360f, 64f);
            countBar.anchoredPosition = new Vector2(120f, -52f);
            MenuTheme.Label(countBar, "COLLECTED  " + owned + " / " + roster.Count, 30, MenuTheme.Gold, TextAnchor.MiddleCenter, true);

            // left: detail panel
            var detail = MenuTheme.Framed(body, "Detail", new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(30f, 30f), new Vector2(620f, -30f), MenuTheme.GoldDark, new Color(0.88f, 0.80f, 0.64f, 1f), 5f);
            BuildHeroDetail(detail);

            // right: element filter
            var filterCol = MenuTheme.Box(body, "Filter", new Vector2(1f, 0f), new Vector2(1f, 1f),
                new Vector2(-130f, 30f), new Vector2(-20f, -30f), new Color(0f, 0f, 0f, 0.35f));
            var allBtn = MenuTheme.BtnAt(filterCol, "All", "ALL", new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(90f, 80f),
                _heroFilter == -1 ? MenuTheme.GoldDark : new Color(0.3f, 0.2f, 0.12f), Color.white, 26, () => { _heroFilter = -1; Refresh(); });
            for (int e = 1; e <= 7; e++)
            {
                int el = e;
                var c = ElementalMatrix.GetElementColor((ElementType)e);
                string[] ab = { "", "FI", "WA", "IC", "LT", "NA", "DK", "LG" };
                var b = MenuTheme.BtnAt(filterCol, "El" + e, ab[e], new Vector2(0.5f, 1f),
                    new Vector2(0f, -60f - e * 92f), new Vector2(90f, 80f),
                    _heroFilter == e ? c : Color.Lerp(c, Color.black, 0.6f), Color.white, 26, () => { _heroFilter = el; Refresh(); });
            }

            // center: FULL roster grid — locked heroes appear as "?" cards
            var grid = MenuTheme.ScrollGrid(body, "Grid", Vector2.zero, Vector2.one,
                new Vector2(670f, 110f), new Vector2(-150f, -30f), new Vector2(210f, 260f), new Vector2(18f, 18f), 4);
            for (int i = 0; i < roster.Count; i++)
            {
                var data = roster[i];
                if (_heroFilter != -1 && (int)data.element != _heroFilter) continue;
                int idx = i;
                bool isOwned = s.unlockedHeroIds.Contains(data.heroId);

                RectTransform cardOuter;
                if (isOwned)
                {
                    int level = 1;
                    for (int t = 0; t < _team.Heroes.Count; t++)
                    {
                        if (_team.Heroes[t] != null && _team.Heroes[t].Data.heroId == data.heroId)
                        {
                            level = _team.Heroes[t].Stats.Level;
                            break;
                        }
                    }
                    cardOuter = MenuTheme.Box(grid, "Hero" + i, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                        idx == _heroSel ? MenuTheme.Gold : ItemDatabase.RarityColor(1 + (int)data.rarity), true, true);
                    var cardIn = MenuTheme.Box(cardOuter, "Fill", Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f), new Color(0.12f, 0.15f, 0.22f));
                    MenuTheme.HeroPortrait(cardIn, new Vector2(0.5f, 0.62f), Vector2.zero, 150f, data, level);
                    MenuTheme.Stars(cardIn, Mathf.Clamp((int)data.rarity + 1, 1, 5), new Vector2(0.5f, 0.2f), new Vector2(0f, 12f), 26f);
                    MenuTheme.Label(cardIn, data.displayName, 28, Color.white, TextAnchor.LowerCenter);
                }
                else
                {
                    cardOuter = MenuTheme.Box(grid, "Hero" + i, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                        new Color(0.16f, 0.13f, 0.14f), true, true);
                    var cardIn = MenuTheme.Box(cardOuter, "Fill", Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f), new Color(0.07f, 0.06f, 0.08f));
                    var q = MenuTheme.Circle(cardIn, "Q", new Vector2(0.5f, 0.62f), Vector2.zero, 130f, new Color(0.15f, 0.13f, 0.17f));
                    MenuTheme.Label(q, "?", 64, new Color(1f, 1f, 1f, 0.22f), TextAnchor.MiddleCenter);
                    MenuTheme.Label(cardIn, "???", 28, new Color(1f, 1f, 1f, 0.4f), TextAnchor.LowerCenter);
                    var unl = MenuTheme.Label(cardIn, "SUMMON TO UNLOCK", 20, MenuTheme.ParchmentDark, TextAnchor.UpperCenter);
                    unl.rectTransform.offsetMax = new Vector2(0f, -14f);
                }

                var btn = cardOuter.gameObject.AddComponent<Button>();
                btn.targetGraphic = cardOuter.GetComponent<Image>();
                btn.onClick.AddListener(() => { _heroSel = idx; Refresh(); });
            }
        }

        private void Refresh()
        {
            if (_history.Count > 0) Show(_history[_history.Count - 1]);
        }

        private void BuildHeroDetail(RectTransform p)
        {
            var roster = HeroDatabase.AllHeroes;
            if (roster.Count == 0) return;
            _heroSel = Mathf.Clamp(_heroSel, 0, roster.Count - 1);
            var d = roster[_heroSel];
            var s = SaveSystem.Current;
            bool owned = s.unlockedHeroIds.Contains(d.heroId);

            // live team instance (null when the hero is owned but benched)
            HeroCharacter live = null;
            for (int i = 0; i < _team.Heroes.Count; i++)
            {
                if (_team.Heroes[i] != null && _team.Heroes[i].Data.heroId == d.heroId) { live = _team.Heroes[i]; break; }
            }

            Color el = ElementalMatrix.GetElementColor(d.element);

            // ---------- locked hero ----------
            if (!owned)
            {
                var q = MenuTheme.Circle(p, "Silhouette", new Vector2(0.5f, 1f), new Vector2(0f, -150f), 210f, new Color(0.14f, 0.12f, 0.15f));
                MenuTheme.Label(q, "?", 110, new Color(1f, 1f, 1f, 0.22f), TextAnchor.MiddleCenter);
                var nm = MenuTheme.Label(p, "???", 52, MenuTheme.Ink, TextAnchor.UpperCenter);
                nm.rectTransform.offsetMax = new Vector2(-6f, -290f);
                nm.rectTransform.offsetMin = new Vector2(6f, 0f);
                nm.alignment = TextAnchor.UpperCenter;

                var hint = MenuTheme.Label(p, "Summon this hero from the\nSUMMON screen.", 32, MenuTheme.Ink, TextAnchor.UpperCenter);
                hint.rectTransform.offsetMin = new Vector2(20f, 330f);
                hint.rectTransform.offsetMax = new Vector2(-20f, 250f);

                var chip = MenuTheme.At(p, "ElementChip", new Vector2(0.5f, 1f), new Vector2(0f, -415f), new Vector2(360f, 46f), el);
                MenuTheme.Label(chip, d.element + "  -  " + d.heroClass, 28, Color.white, TextAnchor.MiddleCenter, true);
                return;
            }

            int level = live != null ? live.Stats.Level : Mathf.Max(1, s.playerLevel);
            float maxHp = live != null ? live.Stats.MaxHP : d.baseHP;
            float atk = live != null ? live.Stats.Attack : d.baseAttack;
            float def = live != null ? live.Stats.Defense : d.baseDefense;
            float ms = live != null ? live.Stats.MoveSpeed : d.baseMoveSpeed;
            float crit = live != null ? live.Stats.CritChance : GameConfig.CritDefaultChance;

            MenuTheme.HeroPortrait(p, new Vector2(0.5f, 1f), new Vector2(0f, -150f), 230f, d, level);
            var name = MenuTheme.Label(p, d.displayName, 52, MenuTheme.Ink, TextAnchor.UpperCenter);
            name.rectTransform.offsetMax = new Vector2(-6f, -290f);
            name.rectTransform.offsetMin = new Vector2(6f, 0f);
            name.alignment = TextAnchor.UpperCenter;

            MenuTheme.Stars(p, Mathf.Clamp((int)d.rarity + 1, 1, 5), new Vector2(0.5f, 1f), new Vector2(0f, -360f), 36f);

            var chip2 = MenuTheme.At(p, "ElementChip", new Vector2(0.5f, 1f), new Vector2(0f, -415f), new Vector2(360f, 46f), el);
            MenuTheme.Label(chip2, d.element + "  -  " + d.heroClass, 28, Color.white, TextAnchor.MiddleCenter, true);

            string stats =
                "HP        " + Mathf.RoundToInt(maxHp) + "\n" +
                "ATK       " + Mathf.RoundToInt(atk) + "\n" +
                "DEF       " + Mathf.RoundToInt(def) + "\n" +
                "SPEED     " + ms.ToString("0.0") + "\n" +
                "CRIT      " + Mathf.RoundToInt(crit * 100f) + "%";
            var st = MenuTheme.Label(p, stats, 34, MenuTheme.Ink, TextAnchor.UpperLeft);
            st.rectTransform.offsetMax = new Vector2(-20f, -450f);
            st.rectTransform.offsetMin = new Vector2(30f, 170f);
            st.resizeTextForBestFit = false;
            st.fontStyle = FontStyle.Normal;

            string skills = "1. " + d.skill1.displayName + "\n2. " + d.skill2.displayName + "\nULT: " + d.ultimate.displayName;
            var sk = MenuTheme.Label(p, skills, 28, new Color(0.35f, 0.2f, 0.1f), TextAnchor.LowerLeft);
            sk.rectTransform.offsetMax = new Vector2(-20f, 170f);
            sk.rectTransform.offsetMin = new Vector2(30f, 60f);
            sk.resizeTextForBestFit = false;

            if (live != null)
            {
                float need = CharacterStats.NeededExp(live.Stats.Level);
                MenuTheme.Label(p, "EXP " + Mathf.RoundToInt(live.Stats.Exp) + " / " + Mathf.RoundToInt(need), 24, MenuTheme.Ink, TextAnchor.LowerCenter)
                    .rectTransform.offsetMax = new Vector2(-6f, 170f);
            }

            // ---------- v0.5: equipment + level up screen ----------
            if (live != null)
            {
                string manageId = d.heroId;
                MenuTheme.BtnAt(p, "Manage", "EQUIP / LEVEL UP", new Vector2(1f, 1f), new Vector2(-150f, -40f), new Vector2(270f, 64f),
                    new Color(0.95f, 0.7f, 0.2f), new Color(0.25f, 0.12f, 0.02f), 24, () => OpenHeroScreen(manageId));
            }

            // ---------- team loadout toggle (v0.4) ----------
            bool inTeam = s.teamLoadout.Contains(d.heroId);
            bool canAdd = !inTeam && s.teamLoadout.Count < GameConfig.TeamSize;
            bool canRemove = inTeam && s.teamLoadout.Count > 1;
            bool interactive = canAdd || canRemove;
            string btnLabel = inTeam
                ? (canRemove ? "REMOVE FROM TEAM" : "IN BATTLE TEAM")
                : (canAdd ? "ADD TO TEAM" : "TEAM FULL");
            string heroId = d.heroId;
            bool teamBtnInteractive = interactive;
            bool teamBtnIn = inTeam;
            bool teamBtnCanAdd = canAdd;
            MenuTheme.BtnAt(p, "Team", btnLabel, new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(470f, 84f),
                !interactive ? new Color(0.45f, 0.42f, 0.40f)
                    : inTeam ? new Color(0.75f, 0.32f, 0.20f) : new Color(0.25f, 0.55f, 0.30f), Color.white, 30, () =>
                {
                    if (!teamBtnInteractive) return;
                    if (teamBtnIn) s.teamLoadout.Remove(heroId);
                    else if (teamBtnCanAdd) s.teamLoadout.Add(heroId);
                    else return;
                    SaveSystem.Save(GameManager.Instance != null ? GameManager.Instance.Tower : null, _team);
                    if (TeamManager.InstanceRef != null) TeamManager.InstanceRef.RebuildTeam();
                    Refresh();
                });
        }

        // ======================= HERO DETAIL =======================
        private string _detailHeroId;

        private void OpenHeroScreen(string id) { _detailHeroId = id; Push("HERO", BuildHeroScreen); }

        private HeroCharacter FindTeamHero(string id)
        {
            foreach (var h in _team.Heroes) if (h != null && h.Data.heroId == id) return h;
            return null;
        }

        private void CycleHero(int dir)
        {
            int n = _team.Heroes.Count;
            if (n == 0) return;
            int cur = 0;
            for (int i = 0; i < n; i++) if (_team.Heroes[i].Data.heroId == _detailHeroId) cur = i;
            _detailHeroId = _team.Heroes[(cur + dir + n) % n].Data.heroId;
            Refresh();
        }

        private void BuildHeroScreen(RectTransform body)
        {
            var hero = FindTeamHero(_detailHeroId);
            if (hero == null)
            {
                MenuTheme.Label(body, "Hero not in lineup", 40, Color.white, TextAnchor.MiddleCenter);
                return;
            }
            var d = hero.Data;
            Color el = ElementalMatrix.GetElementColor(d.element);

            // stage
            MenuTheme.Circle(body, "Glow", new Vector2(0f, 0.5f), new Vector2(400f, 10f), 640f, new Color(el.r, el.g, el.b, 0.14f));
            MenuTheme.HeroPortrait(body, new Vector2(0f, 0.5f), new Vector2(400f, 40f), 400f, d, 0);
            MenuTheme.At(body, "StageBase", new Vector2(0f, 0.5f), new Vector2(400f, -200f), new Vector2(560f, 70f), new Color(0.2f, 0.14f, 0.1f, 0.9f));

            // equipment slots (2 columns x 3 rows)
            EquipSlot(body, hero, 0, new Vector2(130f, 250f));
            EquipSlot(body, hero, 1, new Vector2(670f, 250f));
            EquipSlot(body, hero, 2, new Vector2(130f, 20f));
            EquipSlot(body, hero, 3, new Vector2(670f, 20f));
            LockedSlot(body, new Vector2(130f, -210f));
            LockedSlot(body, new Vector2(670f, -210f));

            MenuTheme.BtnAt(body, "Prev", "<", new Vector2(0f, 0.5f), new Vector2(190f, -330f), new Vector2(90f, 90f), new Color(0.2f, 0.2f, 0.25f), Color.white, 48, () => CycleHero(-1));
            MenuTheme.BtnAt(body, "Next", ">", new Vector2(0f, 0.5f), new Vector2(610f, -330f), new Vector2(90f, 90f), new Color(0.2f, 0.2f, 0.25f), Color.white, 48, () => CycleHero(1));

            // right info panel
            var panel = MenuTheme.Framed(body, "Info", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-900f, 30f), new Vector2(-30f, -30f),
                MenuTheme.GoldDark, MenuTheme.Slate, 5f);

            MenuTheme.Stars(panel, Mathf.Clamp((int)d.rarity + 1, 1, 5), new Vector2(0.5f, 1f), new Vector2(0f, -40f), 44f);
            var nm = MenuTheme.At(panel, "Name", new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(700f, 70f), Color.clear);
            MenuTheme.Label(nm, d.displayName, 58, new Color(1f, 0.55f, 0.85f), TextAnchor.MiddleCenter, true);
            var cls = MenuTheme.At(panel, "Class", new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(700f, 44f), Color.clear);
            MenuTheme.Label(cls, d.heroClass + "  |  " + d.element, 30, new Color(0.85f, 0.85f, 0.9f), TextAnchor.MiddleCenter);

            var pow = MenuTheme.At(panel, "Power", new Vector2(0.5f, 1f), new Vector2(0f, -240f), new Vector2(380f, 64f), new Color(0f, 0f, 0f, 0.55f));
            MenuTheme.Label(pow, "POWER  " + HeroPower(hero), 38, MenuTheme.Gold, TextAnchor.MiddleCenter, true);

            string[] sl = { "LEVEL", "HP", "ATK", "DEF" };
            string[] sv = { hero.Stats.Level.ToString(), Mathf.RoundToInt(hero.Stats.MaxHP).ToString(),
                            Mathf.RoundToInt(hero.Stats.Attack).ToString(), Mathf.RoundToInt(hero.Stats.Defense).ToString() };
            for (int i = 0; i < 4; i++)
            {
                float x = -300f + i * 200f;
                var a = MenuTheme.At(panel, "SL" + i, new Vector2(0.5f, 1f), new Vector2(x, -325f), new Vector2(190f, 36f), Color.clear);
                MenuTheme.Label(a, sl[i], 26, new Color(0.7f, 0.75f, 0.85f), TextAnchor.MiddleCenter);
                var b = MenuTheme.At(panel, "SV" + i, new Vector2(0.5f, 1f), new Vector2(x, -375f), new Vector2(190f, 56f), Color.clear);
                MenuTheme.Label(b, sv[i], 44, Color.white, TextAnchor.MiddleCenter, true);
            }

            // skills
            SkillData[] sk = { d.skill1, d.skill2, d.ultimate };
            string[] st = { "S1", "S2", "ULT" };
            for (int i = 0; i < 4; i++)
            {
                float x = -300f + i * 200f;
                bool has = i < 3;
                var dia = MenuTheme.At(panel, "SkillDia" + i, new Vector2(0.5f, 1f), new Vector2(x, -510f), new Vector2(104f, 104f),
                    has ? Color.Lerp(el, Color.black, 0.3f) : new Color(0.25f, 0.25f, 0.28f), true);
                dia.localRotation = Quaternion.Euler(0f, 0f, 45f);
                var tx = MenuTheme.At(panel, "SkillTx" + i, new Vector2(0.5f, 1f), new Vector2(x, -510f), new Vector2(110f, 60f), Color.clear);
                MenuTheme.Label(tx, has ? st[i] : "?", 36, Color.white, TextAnchor.MiddleCenter, true);
                var nmb = MenuTheme.At(panel, "SkillNm" + i, new Vector2(0.5f, 1f), new Vector2(x, -595f), new Vector2(200f, 48f), Color.clear);
                MenuTheme.Label(nmb, has ? sk[i].displayName : "Locked", 24, new Color(0.85f, 0.85f, 0.9f), TextAnchor.MiddleCenter);
            }

            // level up
            int lvl = hero.Stats.Level;
            int cost = 100 * lvl;
            bool maxed = lvl >= 60;
            float need = CharacterStats.NeededExp(lvl);
            var exp = MenuTheme.At(panel, "Exp", new Vector2(0.5f, 0f), new Vector2(0f, 250f), new Vector2(700f, 44f), Color.clear);
            MenuTheme.Label(exp, "EXP  " + Mathf.RoundToInt(hero.Stats.Exp) + " / " + Mathf.RoundToInt(need), 28, new Color(0.7f, 0.9f, 1f), TextAnchor.MiddleCenter);
            var costBox = MenuTheme.At(panel, "Cost", new Vector2(0.5f, 0f), new Vector2(0f, 200f), new Vector2(700f, 44f), Color.clear);
            bool afford = Inventory.Gold >= cost;
            MenuTheme.Label(costBox, "Gold  " + FormatNumber(Inventory.Gold) + " / " + FormatNumber(cost), 30,
                afford ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f), TextAnchor.MiddleCenter);
            MenuTheme.BtnAt(panel, "LevelUp", maxed ? "MAX LEVEL" : "LEVEL UP", new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(700f, 110f),
                maxed ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.95f, 0.7f, 0.2f), new Color(0.25f, 0.12f, 0.02f), 50, () =>
                {
                    if (maxed) return;
                    if (!Inventory.SpendGold(cost)) { Toast("Not enough gold"); return; }
                    hero.Stats.GrantExp(CharacterStats.NeededExp(hero.Stats.Level) - hero.Stats.Exp + 0.01f);
                    SaveSystem.Save(null, null);
                    Toast(d.displayName + " reached Lv." + hero.Stats.Level);
                    Refresh();
                });
        }

        private void EquipSlot(RectTransform body, HeroCharacter hero, int slot, Vector2 pos)
        {
            string itemId = Equipment.Get(hero.Data.heroId, slot);
            var def = itemId != null ? ItemDatabase.Get(itemId) : null;
            RectTransform tile;
            if (def != null)
            {
                tile = MenuTheme.ItemTile(body, def, 0, 130f);
                tile.anchorMin = tile.anchorMax = new Vector2(0f, 0.5f);
                tile.sizeDelta = new Vector2(130f, 130f);
                tile.anchoredPosition = pos;
            }
            else
            {
                tile = MenuTheme.At(body, "Slot" + slot, new Vector2(0f, 0.5f), pos, new Vector2(130f, 130f), MenuTheme.GoldDark, true, true);
                var fill = MenuTheme.Box(tile, "Fill", Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f), new Color(0.12f, 0.1f, 0.12f));
                MenuTheme.Label(fill, "+", 70, new Color(1f, 1f, 1f, 0.45f), TextAnchor.MiddleCenter);
            }
            var cap = MenuTheme.At(body, "SlotName" + slot, new Vector2(0f, 0.5f), pos + new Vector2(0f, -88f), new Vector2(170f, 34f), Color.clear);
            MenuTheme.Label(cap, ItemDatabase.SlotNames[slot], 24, MenuTheme.ParchmentDark, TextAnchor.MiddleCenter);
            var btn = tile.gameObject.AddComponent<Button>();
            btn.targetGraphic = tile.GetComponent<Image>();
            int s = slot;
            btn.onClick.AddListener(() => OpenEquipPicker(hero, s));
        }

        private void LockedSlot(RectTransform body, Vector2 pos)
        {
            var t = MenuTheme.At(body, "Locked", new Vector2(0f, 0.5f), pos, new Vector2(130f, 130f), new Color(0.3f, 0.3f, 0.3f, 0.6f), true, true);
            MenuTheme.Label(t, "LOCKED", 22, new Color(1f, 1f, 1f, 0.5f), TextAnchor.MiddleCenter);
            var btn = t.gameObject.AddComponent<Button>();
            btn.targetGraphic = t.GetComponent<Image>();
            btn.onClick.AddListener(() => Toast("Unlocks with a later update"));
        }

        private void OpenEquipPicker(HeroCharacter hero, int slot)
        {
            var overlay = MenuTheme.Stretch(_screenHost, "EquipOverlay", new Color(0f, 0f, 0f, 0.72f));
            overlay.GetComponent<Image>().raycastTarget = true;
            var panel = MenuTheme.Framed(overlay, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-480f, -330f), new Vector2(480f, 330f), MenuTheme.GoldDark, new Color(0.88f, 0.80f, 0.64f, 1f), 5f);
            var title = MenuTheme.At(panel, "Title", new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(700f, 60f), Color.clear);
            MenuTheme.Label(title, "Select " + ItemDatabase.SlotNames[slot], 44, MenuTheme.Ink, TextAnchor.MiddleCenter);

            var grid = MenuTheme.ScrollGrid(panel, "Grid", Vector2.zero, Vector2.one, new Vector2(20f, 120f), new Vector2(-20f, -90f),
                new Vector2(140f, 140f), new Vector2(14f, 14f), 6);
            int shown = 0;
            foreach (var def in ItemDatabase.List)
            {
                if (def.slot != slot) continue;
                int have = Inventory.Count(def.id);
                if (have <= 0) continue;
                shown++;
                var tile = MenuTheme.ItemTile(grid, def, have, 140f);
                string id = def.id;
                var b = tile.gameObject.AddComponent<Button>();
                b.targetGraphic = tile.GetComponent<Image>();
                b.onClick.AddListener(() =>
                {
                    Equipment.Equip(hero.Data.heroId, slot, id);
                    Equipment.Apply(hero);
                    SaveSystem.Save(null, null);
                    Destroy(overlay.gameObject);
                    Toast("Equipped " + def.name);
                    Refresh();
                });
            }
            if (shown == 0)
            {
                var none = MenuTheme.At(panel, "None", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 120f), Color.clear);
                MenuTheme.Label(none, "No " + ItemDatabase.SlotNames[slot].ToLower() + " in your bag. Open Mystic Chests to find some.", 34, MenuTheme.Ink, TextAnchor.MiddleCenter);
            }

            if (Equipment.Get(hero.Data.heroId, slot) != null)
            {
                MenuTheme.BtnAt(panel, "Remove", "REMOVE", new Vector2(0.5f, 0f), new Vector2(-220f, 55f), new Vector2(330f, 80f),
                    new Color(0.7f, 0.25f, 0.2f), Color.white, 36, () =>
                    {
                        Equipment.Unequip(hero.Data.heroId, slot);
                        Equipment.Apply(hero);
                        SaveSystem.Save(null, null);
                        Destroy(overlay.gameObject);
                        Refresh();
                    });
            }
            MenuTheme.BtnAt(panel, "Close", "CLOSE", new Vector2(0.5f, 0f), new Vector2(Equipment.Get(hero.Data.heroId, slot) != null ? 220f : 0f, 55f),
                new Vector2(330f, 80f), new Color(0.45f, 0.28f, 0.14f), Color.white, 36, () => Destroy(overlay.gameObject));
        }

        private static int HeroPower(HeroCharacter h)
        {
            return Mathf.RoundToInt(h.Stats.MaxHP * 0.1f + h.Stats.Attack * 6f + h.Stats.Defense * 10f);
        }

        // ======================= BAG =======================
        private ItemTab _bagTab = ItemTab.Items;
        private string _bagSel;

        private void BuildBag(RectTransform body)
        {
            // tabs
            string[] tabs = { "Items", "Shards", "Equip" };
            for (int i = 0; i < tabs.Length; i++)
            {
                var tab = (ItemTab)i;
                MenuTheme.BtnAt(body, "Tab" + i, tabs[i], new Vector2(0f, 1f), new Vector2(140f, -90f - i * 110f), new Vector2(240f, 90f),
                    _bagTab == tab ? MenuTheme.GoldDark : new Color(0.38f, 0.24f, 0.14f), Color.white, 34, () => { _bagTab = tab; _bagSel = null; Refresh(); });
            }

            // grid
            var panel = MenuTheme.Framed(body, "GridPanel", Vector2.zero, Vector2.one, new Vector2(290f, 30f), new Vector2(-640f, -30f),
                MenuTheme.GoldDark, new Color(0.84f, 0.76f, 0.60f, 1f), 5f);
            var grid = MenuTheme.ScrollGrid(panel, "Grid", Vector2.zero, Vector2.one, new Vector2(10f, 10f), new Vector2(-10f, -10f),
                new Vector2(150f, 150f), new Vector2(14f, 14f), 5);

            var s = SaveSystem.Current;
            int shown = 0;
            for (int i = 0; i < s.itemIds.Count; i++)
            {
                var def = ItemDatabase.Get(s.itemIds[i]);
                if (def == null || def.tab != _bagTab) continue;
                shown++;
                var tile = MenuTheme.ItemTile(grid, def, s.itemCounts[i], 150f);
                string id = def.id;
                var b = tile.gameObject.AddComponent<Button>();
                b.targetGraphic = tile.GetComponent<Image>();
                b.onClick.AddListener(() => { _bagSel = id; Refresh(); });
                if (_bagSel == id) MenuTheme.Box(tile, "Sel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.18f));
            }
            if (shown == 0) MenuTheme.Label(panel, "Nothing here yet.", 36, MenuTheme.Ink, TextAnchor.MiddleCenter);

            // detail
            var det = MenuTheme.Framed(body, "Detail", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-620f, 30f), new Vector2(-30f, -30f),
                MenuTheme.GoldDark, new Color(0.88f, 0.80f, 0.64f, 1f), 5f);
            var sel = _bagSel != null ? ItemDatabase.Get(_bagSel) : null;
            if (sel == null || Inventory.Count(sel.id) <= 0)
            {
                MenuTheme.Label(det, "Select an item", 36, MenuTheme.Ink, TextAnchor.MiddleCenter);
                return;
            }
            var big = MenuTheme.ItemTile(det, sel, 0, 190f);
            var bigRt = big;
            bigRt.anchorMin = new Vector2(0.5f, 1f); bigRt.anchorMax = new Vector2(0.5f, 1f);
            bigRt.sizeDelta = new Vector2(190f, 190f); bigRt.anchoredPosition = new Vector2(0f, -120f);
            var nm = MenuTheme.Label(det, sel.name, 44, ItemDatabase.RarityColor(sel.rarity) * 0.7f, TextAnchor.UpperCenter);
            nm.rectTransform.offsetMax = new Vector2(-6f, -230f);
            MenuTheme.Label(det, "Owned: " + Inventory.Count(sel.id), 32, MenuTheme.Ink, TextAnchor.UpperCenter)
                .rectTransform.offsetMax = new Vector2(-6f, -300f);
            var ds = MenuTheme.Label(det, sel.desc, 30, MenuTheme.Ink, TextAnchor.UpperLeft);
            ds.rectTransform.offsetMax = new Vector2(-24f, -370f);
            ds.rectTransform.offsetMin = new Vector2(24f, 150f);
            ds.fontStyle = FontStyle.Normal;
            ds.resizeTextForBestFit = false;

            bool usable = sel.id == "exp_elixir" || sel.id == "mystic_chest";
            string selId = sel.id;
            MenuTheme.BtnAt(det, "Use", usable ? "USE" : "-", new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(360f, 90f),
                usable ? new Color(0.85f, 0.6f, 0.15f) : new Color(0.4f, 0.4f, 0.4f), Color.white, 40, () =>
                {
                    if (!usable) return;
                    string msg = Inventory.Use(selId, _team);
                    if (msg != null) Toast(msg);
                    Refresh();
                });
        }

        // ======================= SHOP =======================
        private void BuildShop(RectTransform body)
        {
            MenuTheme.BtnAt(body, "TabDaily", "Daily", new Vector2(0f, 1f), new Vector2(140f, -90f), new Vector2(240f, 90f),
                MenuTheme.GoldDark, Color.white, 34, null);
            string[] soon = { "Points", "Limited", "PVP" };
            for (int i = 0; i < soon.Length; i++)
            {
                var b = MenuTheme.BtnAt(body, "Tab" + soon[i], soon[i], new Vector2(0f, 1f), new Vector2(140f, -200f - i * 110f), new Vector2(240f, 90f),
                    new Color(0.25f, 0.2f, 0.17f), new Color(1f, 1f, 1f, 0.5f), 34, () => Toast("Coming soon"));
            }

            var grid = MenuTheme.ScrollGrid(body, "Offers", Vector2.zero, Vector2.one, new Vector2(300f, 70f), new Vector2(-40f, -20f),
                new Vector2(330f, 400f), new Vector2(24f, 24f), 4);

            foreach (var o in DailyShop.Offers)
            {
                var offer = o;
                var def = ItemDatabase.Get(offer.itemId);
                int bought = DailyShop.Bought(offer);
                bool soldOut = bought >= offer.dailyLimit;

                var card = MenuTheme.Box(grid, "Offer_" + offer.itemId, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MenuTheme.ParchmentDark);
                var inner = MenuTheme.Box(card, "Fill", Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f), MenuTheme.Parchment);

                var ribbon = MenuTheme.At(inner, "Ribbon", new Vector2(0f, 1f), new Vector2(75f, -22f), new Vector2(150f, 44f), MenuTheme.Red);
                MenuTheme.Label(ribbon, offer.discountPct + "% OFF", 26, Color.white, TextAnchor.MiddleCenter, true);

                var icon = MenuTheme.ItemTile(inner, def, offer.qty, 150f);
                icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 0.62f);
                icon.sizeDelta = new Vector2(150f, 150f);
                icon.anchoredPosition = Vector2.zero;

                var nm = MenuTheme.Label(inner, def.name, 28, MenuTheme.Ink, TextAnchor.MiddleCenter);
                nm.rectTransform.anchorMin = new Vector2(0f, 0.28f); nm.rectTransform.anchorMax = new Vector2(1f, 0.4f);
                nm.rectTransform.offsetMin = nm.rectTransform.offsetMax = Vector2.zero;

                var lim = MenuTheme.Label(inner, "Daily Limit: " + bought + "/" + offer.dailyLimit, 24, new Color(0.4f, 0.25f, 0.1f), TextAnchor.MiddleCenter);
                lim.rectTransform.anchorMin = new Vector2(0f, 0.2f); lim.rectTransform.anchorMax = new Vector2(1f, 0.3f);
                lim.rectTransform.offsetMin = lim.rectTransform.offsetMax = Vector2.zero;

                MenuTheme.BtnAt(inner, "Buy", soldOut ? "SOLD OUT" : offer.priceGems + " gems", new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(270f, 70f),
                    soldOut ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.25f, 0.5f, 0.85f), Color.white, 32, () =>
                    {
                        string err;
                        if (DailyShop.Buy(offer, out err)) { Toast("Purchased " + def.name + " x" + offer.qty); SaveSystem.Save(null, null); }
                        else Toast(err);
                        Refresh();
                    });
            }

            var t = TimeSpan.FromSeconds(Inventory.SecondsToReset());
            var restock = MenuTheme.At(body, "Restock", new Vector2(0.5f, 0f), new Vector2(120f, 35f), new Vector2(600f, 40f), Color.clear);
            MenuTheme.Label(restock, "Daily restock: " + t.Hours.ToString("00") + ":" + t.Minutes.ToString("00"), 26, MenuTheme.ParchmentDark, TextAnchor.MiddleCenter);
        }

        // ======================= MISSIONS =======================
        private void BuildMissions(RectTransform body)
        {
            var list = MenuTheme.ScrollList(body, "List", Vector2.zero, Vector2.one, new Vector2(60f, 30f), new Vector2(-60f, -20f), 16f);

            for (int i = 0; i < DailyMissions.All.Length; i++)
            {
                int idx = i;
                var m = DailyMissions.All[i];
                int prog = DailyMissions.Progress(i);
                bool claimed = DailyMissions.Claimed(i);
                bool can = DailyMissions.CanClaim(i);

                var row = MenuTheme.Box(list, "Mission" + i, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, MenuTheme.ParchmentDark);
                var le = row.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 150f;
                var inner = MenuTheme.Box(row, "Fill", Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f), MenuTheme.Parchment);

                var title = MenuTheme.Label(inner, m.title, 38, MenuTheme.Ink, TextAnchor.MiddleLeft);
                title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                title.rectTransform.offsetMin = new Vector2(30f, 0f);

                var reward = MenuTheme.Label(inner, "+" + m.rewardGems + " gems   +" + m.rewardGold + " gold", 30, new Color(0.45f, 0.28f, 0.08f), TextAnchor.MiddleCenter);
                reward.rectTransform.anchorMin = new Vector2(0.45f, 0f); reward.rectTransform.anchorMax = new Vector2(0.72f, 0.5f);
                reward.rectTransform.offsetMin = reward.rectTransform.offsetMax = Vector2.zero;

                // progress bar
                var barBg = MenuTheme.Box(inner, "Bar", new Vector2(0.45f, 0.62f), new Vector2(0.72f, 0.85f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.55f));
                var fill = MenuTheme.Box(barBg, "Fill", Vector2.zero, new Vector2(Mathf.Clamp01(prog / (float)m.target), 1f), new Vector2(2f, 2f), new Vector2(-2f, -2f), MenuTheme.Green);
                MenuTheme.Label(barBg, prog + "/" + m.target, 24, Color.white, TextAnchor.MiddleCenter, true);

                string label = claimed ? "CLAIMED" : can ? "CLAIM" : "GO";
                Color col = claimed ? new Color(0.4f, 0.4f, 0.4f) : can ? new Color(0.2f, 0.55f, 0.9f) : new Color(0.85f, 0.6f, 0.15f);
                MenuTheme.BtnAt(inner, "Action", label, new Vector2(1f, 0.5f), new Vector2(-170f, 0f), new Vector2(280f, 90f), col, Color.white, 36, () =>
                {
                    if (claimed) return;
                    if (can)
                    {
                        DailyMissions.Claim(idx);
                        SaveSystem.Save(null, null);
                        Toast("Reward claimed!");
                        Refresh();
                    }
                    else CloseToBattle();   // "GO": back to the tower
                });
            }
        }
    }
}
