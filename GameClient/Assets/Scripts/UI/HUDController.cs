using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Cam;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;
using LegendsRPG.InputCtl;
using LegendsRPG.Team;
using LegendsRPG.World;

namespace LegendsRPG.UI
{
    /// <summary>
    /// The complete touch HUD, built entirely in code:
    /// joystick, attack button, skill buttons with radial cooldowns,
    /// ultimate charge, team portraits with HP + tap-to-switch,
    /// auto-battle toggle, floor/difficulty header, FPS meter,
    /// floor-cleared panel and notifications.
    /// Every button is functional.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static bool AttackButtonHeld { get; private set; }

        private TeamManager _team;
        private IsometricCameraController _camera;

        private GameObject _canvasGo;
        private Text _floorText;
        private Text _difficultyText;
        private Text _fpsText;
        private Text _notificationText;
        private Text _activeHeroText;
        private Text _activeHeroElementText;
        private Image _activeHeroHpFill;
        private Image _activeHeroExpFill;
        private Image _ultChargeFill;
        private Image[] _skillCooldownFills;
        private Text[] _skillCooldownTexts;
        private Text[] _skillLabels;
        private Button[] _skillButtons;
        private readonly Image[] _portraitFills = new Image[6];
        private readonly Image[] _portraitFrames = new Image[6];
        private Button _autoBattleButton;
        private Text _autoBattleLabel;
        private RectTransform _floorClearedPanel;
        private Text _floorClearedLabel;
        private float _notificationTimer;
        private float _fpsRefresh;

        public void Build(TeamManager team, IsometricCameraController camera)
        {
            _team = team;
            _camera = camera;

            var canvas = UIBuilder.CreateCanvas("HUDCanvas");
            _canvasGo = canvas.gameObject;

            BuildHeader(canvas.transform);
            BuildTeamPortraits(canvas.transform);
            BuildBottomBar(canvas.transform);
            BuildSkills(canvas.transform);
            BuildAutoBattleButton(canvas.transform);
            BuildFloorClearedPanel(canvas.transform);

            _notificationText = UIBuilder.CreateText(canvas.transform, "Notification", "", 34,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -320f),
                new Vector2(900f, 60f));

            CombatEvents.DamageDealt += OnDamageDealt;
            CombatEvents.CharacterDied += OnCharacterDied;
            CombatEvents.Notification += OnNotification;
            CombatEvents.HeroLevelUp += OnHeroLevelUp;
        }

        private void OnDestroy()
        {
            CombatEvents.DamageDealt -= OnDamageDealt;
            CombatEvents.CharacterDied -= OnCharacterDied;
            CombatEvents.Notification -= OnNotification;
            CombatEvents.HeroLevelUp -= OnHeroLevelUp;
        }

        // ================= construction =================
        private void BuildHeader(Transform parent)
        {
            var bar = UIBuilder.CreatePanel(parent, "TopBar", AnimeTheme.Ink,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -150f), new Vector2(0f, 0f));

            UIBuilder.CreateText(bar, "Brand", "LEGENDS  /  TOWER", 28, AnimeTheme.Cyan,
                TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(290f, 0f), new Vector2(300f, 42f));

            // MENU button (opens the management screens and pauses the battle)
            UIBuilder.CreateButton(bar, "MenuBtn", "MENU", new Vector2(150f, 80f),
                new Vector2(0f, 0.5f), new Vector2(100f, 0f), AnimeTheme.PanelRaised,
                AnimeTheme.Text, 26, () => { if (MenuController.Instance != null) MenuController.Instance.Open(); });

            _difficultyText = UIBuilder.CreateText(bar, "Difficulty", "EASY", 30,
                AnimeTheme.Gold, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f),
                new Vector2(340f, 0f), new Vector2(320f, 44f));

            _floorText = UIBuilder.CreateText(bar, "Floor", "Floor 1", 34,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(320f, 44f));

            _fpsText = UIBuilder.CreateText(bar, "FPS", "60 FPS", 24,
                new Color(0.6f, 1f, 0.6f), TextAnchor.MiddleRight, new Vector2(1f, 0.5f),
                new Vector2(-180f, 0f), new Vector2(160f, 40f));
        }

        private void BuildTeamPortraits(Transform parent)
        {
            var row = UIBuilder.CreateFreeRect(parent, "TeamRow", new Vector2(660f, 110f),
                new Vector2(0f, 1f), new Vector2(345f, -230f));

            for (int i = 0; i < GameConfig.TeamSize; i++)
            {
                Vector2 anchor = new Vector2(0f, 0.5f);
                Vector2 pos = new Vector2(60f + i * 108f, 0f);
                var slot = UIBuilder.CreateFreeRect(row, "Portrait" + i, new Vector2(96f, 96f), anchor, pos);

                var frame = slot.gameObject.AddComponent<Image>();
                frame.sprite = UIBuilder.RoundedSprite;
                frame.type = Image.Type.Sliced;
                frame.color = i < _team.Heroes.Count && _team.Heroes[i] != null
                    ? AnimeTheme.Rarity(_team.Heroes[i].Data.rarity) : AnimeTheme.PanelRaised;
                _portraitFrames[i] = frame;

                UIBuilder.CreateText(slot, "HeroName", i < _team.Heroes.Count && _team.Heroes[i] != null
                    ? _team.Heroes[i].Data.displayName : "HERO", 16, AnimeTheme.Text,
                    TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(92f, 24f));

                var fillArea = UIBuilder.CreateFreeRect(slot, "HpFill", new Vector2(84f, 12f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 12f));
                var bg = fillArea.gameObject.AddComponent<Image>();
                bg.sprite = UIBuilder.SolidSprite;
                bg.color = new Color(0f, 0f, 0f, 0.7f);

                var fillGo = new GameObject("Fill");
                var frt = fillGo.AddComponent<RectTransform>();
                frt.SetParent(fillArea, false);
                frt.anchorMin = new Vector2(0f, 0.5f);
                frt.anchorMax = new Vector2(0f, 0.5f);
                frt.pivot = new Vector2(0f, 0.5f);
                frt.sizeDelta = new Vector2(84f, 12f);
                frt.anchoredPosition = new Vector2(0f, 0f);
                var fill = fillGo.AddComponent<Image>();
                fill.sprite = UIBuilder.SolidSprite;
                fill.color = new Color(0.2f, 0.9f, 0.3f);
                _portraitFills[i] = fill;

                var label = UIBuilder.CreateText(slot, "SlotLabel", (i + 1).ToString(), 34,
                    Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -32f),
                    new Vector2(90f, 40f));

                int index = i;
                var btn = slot.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => OnPortraitTapped(index));
            }
        }

        private void BuildBottomBar(Transform parent)
        {
            UIBuilder.CreatePanel(parent, "ActiveHeroCard", AnimeTheme.Panel,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 245f), new Vector2(520f, 395f));

            // active hero bar above joystick
            _activeHeroText = UIBuilder.CreateText(parent, "ActiveHero", "Kael  Lv.1", 32,
                AnimeTheme.Text, TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(40f, 330f),
                new Vector2(420f, 40f));
            _activeHeroElementText = UIBuilder.CreateText(parent, "ActiveHeroElement", "FIRE", 22,
                AnimeTheme.Gold, TextAnchor.MiddleRight, new Vector2(0f, 0f), new Vector2(450f, 330f),
                new Vector2(70f, 40f));

            var hpBg = UIBuilder.CreateFreeRect(parent, "HpBg", new Vector2(430f, 26f),
                new Vector2(0f, 0f), new Vector2(40f, 300f));
            var hpBgImg = hpBg.gameObject.AddComponent<Image>();
            hpBgImg.sprite = UIBuilder.SolidSprite;
            hpBgImg.color = new Color(0f, 0f, 0f, 0.72f);

            var hpFill = new GameObject("HpFill");
            var hpRt = hpFill.AddComponent<RectTransform>();
            hpRt.SetParent(hpBg, false);
            hpRt.anchorMin = new Vector2(0f, 0.5f);
            hpRt.anchorMax = new Vector2(0f, 0.5f);
            hpRt.pivot = new Vector2(0f, 0.5f);
            hpRt.sizeDelta = new Vector2(426f, 22f);
            hpRt.anchoredPosition = new Vector2(2f, 0f);
            _activeHeroHpFill = hpFill.AddComponent<Image>();
            _activeHeroHpFill.sprite = UIBuilder.SolidSprite;
            _activeHeroHpFill.color = new Color(0.15f, 0.85f, 0.3f);

            var expBg = UIBuilder.CreateFreeRect(parent, "ExpBg", new Vector2(430f, 12f),
                new Vector2(0f, 0f), new Vector2(40f, 268f));
            var expBgImg = expBg.gameObject.AddComponent<Image>();
            expBgImg.sprite = UIBuilder.SolidSprite;
            expBgImg.color = new Color(0f, 0f, 0f, 0.6f);

            var expFill = new GameObject("ExpFill");
            var expRt = expFill.AddComponent<RectTransform>();
            expRt.SetParent(expBg, false);
            expRt.anchorMin = new Vector2(0f, 0.5f);
            expRt.anchorMax = new Vector2(0f, 0.5f);
            expRt.pivot = new Vector2(0f, 0.5f);
            expRt.sizeDelta = new Vector2(426f, 8f);
            expRt.anchoredPosition = new Vector2(2f, 0f);
            _activeHeroExpFill = expFill.AddComponent<Image>();
            _activeHeroExpFill.sprite = UIBuilder.SolidSprite;
            _activeHeroExpFill.color = AnimeTheme.Cyan;

            // ---- joystick ----
            var joyArea = UIBuilder.CreateFreeRect(parent, "Joystick", new Vector2(340f, 340f),
                new Vector2(0f, 0f), new Vector2(230f, 240f));
            var joyBg = joyArea.gameObject.AddComponent<Image>();
            joyBg.sprite = UIBuilder.SolidSprite;
            joyBg.color = new Color(1f, 1f, 1f, 0.10f);
            joyBg.raycastTarget = true;

            var knob = UIBuilder.CreateFreeRect(joyArea, "Knob", new Vector2(150f, 150f),
                new Vector2(0.5f, 0.5f), Vector2.zero);
            var knobImg = knob.gameObject.AddComponent<Image>();
            knobImg.sprite = UIBuilder.SolidSprite;
            knobImg.color = new Color(1f, 1f, 1f, 0.28f);
            knobImg.raycastTarget = false;

            if (TouchInputController.Instance != null)
            {
                TouchInputController.Instance.BindJoystick(joyArea, knob);
            }
        }

        private void BuildSkills(Transform parent)
        {
            _skillCooldownFills = new Image[3];
            _skillCooldownTexts = new Text[3];
            _skillLabels = new Text[3];
            _skillButtons = new Button[3];

            // attack button
            var attack = UIBuilder.CreateButton(parent, "AttackBtn", "ATTACK", new Vector2(220f, 220f),
                new Vector2(1f, 0f), new Vector2(-170f, 200f), AnimeTheme.Danger,
                AnimeTheme.Text, 32, () => { });
            AddHoldBehavior(attack.gameObject);

            // skill 1 & 2
            MakeSkillButton(parent, 0, "S1", new Vector2(1f, 0f), new Vector2(-420f, 170f), new Color(0.2f, 0.5f, 0.9f, 0.95f));
            MakeSkillButton(parent, 1, "S2", new Vector2(1f, 0f), new Vector2(-370f, 380f), new Color(0.55f, 0.3f, 0.9f, 0.95f));

            // ultimate
            var ult = UIBuilder.CreateButton(parent, "UltBtn", "ULT", new Vector2(196f, 196f),
                new Vector2(1f, 0f), new Vector2(-170f, 450f), AnimeTheme.Gold,
                AnimeTheme.Ink, 34, () => CastSkill(2));
            _skillButtons[2] = ult;
            _skillLabels[2] = ult.GetComponentInChildren<Text>();

            var ultFill = new GameObject("UltCharge");
            var ultRt = ultFill.AddComponent<RectTransform>();
            ultRt.SetParent(ult.transform, false);
            ultRt.anchorMin = new Vector2(0.5f, 1f);
            ultRt.anchorMax = new Vector2(0.5f, 1f);
            ultRt.pivot = new Vector2(0.5f, 1f);
            ultRt.sizeDelta = new Vector2(196f, 196f);
            ultRt.anchoredPosition = new Vector2(0f, -2f);
            _ultChargeFill = ultFill.AddComponent<Image>();
            _ultChargeFill.sprite = UIBuilder.SolidSprite;
            _ultChargeFill.color = new Color(0f, 0f, 0f, 0.55f);
            _ultChargeFill.raycastTarget = false;
        }

        private void MakeSkillButton(Transform parent, int index, string label, Vector2 anchor,
            Vector2 pos, Color color)
        {
            var btn = UIBuilder.CreateButton(parent, "Skill" + index, label, new Vector2(150f, 150f),
                anchor, pos, color, AnimeTheme.Text, 28, () => CastSkill(index));
            _skillLabels[index] = btn.GetComponentInChildren<Text>();

            var cdBg = new GameObject("Cooldown");
            var cdRt = cdBg.AddComponent<RectTransform>();
            cdRt.SetParent(btn.transform, false);
            cdRt.anchorMin = new Vector2(0.5f, 0.5f);
            cdRt.anchorMax = new Vector2(0.5f, 0.5f);
            cdRt.sizeDelta = new Vector2(138f, 138f);
            cdRt.anchoredPosition = Vector2.zero;
            var cdImg = cdBg.AddComponent<Image>();
            cdImg.sprite = UIBuilder.SolidSprite;
            cdImg.color = new Color(0f, 0f, 0f, 0.6f);
            cdImg.type = Image.Type.Filled;
            cdImg.fillMethod = Image.FillMethod.Radial360;
            cdImg.fillOrigin = 2;
            cdImg.fillClockwise = false;
            cdImg.raycastTarget = false;
            _skillCooldownFills[index] = cdImg;

            _skillCooldownTexts[index] = UIBuilder.CreateText(cdBg.transform, "CdText", "", 34,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(140f, 50f));
            _skillButtons[index] = btn;
        }

        private void AddHoldBehavior(GameObject buttonGo)
        {
            var holder = buttonGo.AddComponent<AttackHoldButton>();
        }

        private void BuildAutoBattleButton(Transform parent)
        {
            _autoBattleButton = UIBuilder.CreateButton(parent, "AutoBtn", "AUTO\nOFF", new Vector2(150f, 130f),
                new Vector2(1f, 1f), new Vector2(-100f, -230f), AnimeTheme.PanelRaised,
                AnimeTheme.Text, 26, ToggleAutoBattle);
            _autoBattleLabel = _autoBattleButton.GetComponentInChildren<Text>();
        }

        private void ToggleAutoBattle()
        {
            if (AutoBattleController.Instance != null)
            {
                AutoBattleController.Instance.Toggle();
                if (_autoBattleLabel != null)
                {
                    _autoBattleLabel.text = AutoBattleController.Instance.Enabled ? "AUTO\nON" : "AUTO\nOFF";
                }
            }
        }

        private void BuildFloorClearedPanel(Transform parent)
        {
            _floorClearedPanel = UIBuilder.CreatePanel(parent, "FloorCleared", new Color(0f, 0f, 0f, 0.75f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _floorClearedPanel.GetComponent<Image>().color = new Color(AnimeTheme.Ink.r, AnimeTheme.Ink.g, AnimeTheme.Ink.b, 0.94f);
            _floorClearedPanel.gameObject.SetActive(false);

            _floorClearedLabel = UIBuilder.CreateText(_floorClearedPanel, "Title", "FLOOR CLEARED!", 64,
                AnimeTheme.Gold, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 160f), new Vector2(900f, 90f));

            UIBuilder.CreateText(_floorClearedPanel, "Sub", "Loot & EXP collected", 34,
                AnimeTheme.Text, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 60f), new Vector2(700f, 50f));

            UIBuilder.CreateButton(_floorClearedPanel, "NextBtn", "NEXT FLOOR", new Vector2(420f, 120f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), AnimeTheme.Mint,
                AnimeTheme.Ink, 34, () =>
                {
                    if (GameManager.Instance != null) GameManager.Instance.StartNextFloor();
                });
        }

        // ================= behavior =================
        private void OnPortraitTapped(int index)
        {
            if (_team != null) _team.SwitchTo(index);
            Audio.AudioSystem.Play("UI");
        }

        private void CastSkill(int index)
        {
            var hero = _team != null ? _team.ActiveHero : null;
            if (hero == null || hero.HP == null || hero.HP.IsDead) return;
            hero.Caster.TryCast(index, null);
        }

        private void Update()
        {
            UpdateHeader();
            UpdateActiveHeroBars();
            UpdateSkillButtons();
            UpdatePortraits();
            UpdateNotification();
        }

        private void UpdateHeader()
        {
            if (GameManager.Instance == null || GameManager.Instance.Tower == null) return;
            var tower = GameManager.Instance.Tower;
            _floorText.text = "Floor " + tower.CurrentFloor;
            _difficultyText.text = TowerSystem.DifficultyName(tower.Difficulty).ToUpper();

            _fpsRefresh += Time.unscaledDeltaTime;
            if (_fpsRefresh >= 0.5f)
            {
                _fpsRefresh = 0f;
                float fps = GameManager.Instance.CurrentFps;
                _fpsText.text = Mathf.RoundToInt(fps) + " FPS";
                _fpsText.color = fps >= 45f ? new Color(0.6f, 1f, 0.6f)
                    : fps >= 30f ? new Color(1f, 0.85f, 0.4f) : new Color(1f, 0.4f, 0.35f);
            }
        }

        private void UpdateActiveHeroBars()
        {
            var hero = _team != null ? _team.ActiveHero : null;
            if (hero == null || hero.HP == null) return;

            _activeHeroText.text = hero.Data.displayName + "  Lv." + hero.Stats.Level;
            if (_activeHeroElementText != null)
            {
                _activeHeroElementText.text = hero.Data.element.ToString().ToUpper();
                _activeHeroElementText.color = AnimeTheme.Element(hero.Data.element);
            }
            _activeHeroHpFill.rectTransform.sizeDelta = new Vector2(426f * hero.HP.Ratio, 22f);
            _activeHeroHpFill.color = hero.HP.Ratio > 0.5f ? new Color(0.15f, 0.85f, 0.3f)
                : hero.HP.Ratio > 0.25f ? new Color(0.95f, 0.75f, 0.2f) : new Color(0.95f, 0.3f, 0.25f);

            float expRatio = Mathf.Clamp01(hero.Stats.Exp / CharacterStats.NeededExp(hero.Stats.Level));
            _activeHeroExpFill.rectTransform.sizeDelta = new Vector2(426f * expRatio, 8f);

            if (_ultChargeFill != null)
            {
                _ultChargeFill.rectTransform.sizeDelta = new Vector2(196f, 196f * (1f - Mathf.Clamp01(hero.Caster.UltCharge / 100f)));
            }
        }

        private void UpdateSkillButtons()
        {
            var hero = _team != null ? _team.ActiveHero : null;
            if (hero == null) return;

            if (_skillLabels != null && hero.Caster.Skills != null)
            {
                for (int i = 0; i < _skillLabels.Length && i < hero.Caster.Skills.Length; i++)
                {
                    var skill = hero.Caster.Skills[i];
                    if (_skillLabels[i] != null && skill != null)
                        _skillLabels[i].text = skill.IsUltimate ? "ULT\n" + skill.displayName : skill.displayName;
                    if (_skillButtons != null && i < _skillButtons.Length && _skillButtons[i] != null && skill != null)
                        _skillButtons[i].interactable = hero.Caster.GetCooldownLeft(i) <= 0.01f
                            && (!skill.IsUltimate || hero.Caster.UltCharge >= skill.ultimateChargeCost);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                float ratio = hero.Caster.GetCooldownRatio(i);
                if (_skillCooldownFills[i] != null)
                {
                    _skillCooldownFills[i].fillAmount = ratio;
                    float left = hero.Caster.GetCooldownLeft(i);
                    _skillCooldownTexts[i].text = left > 0f ? Mathf.CeilToInt(left).ToString() : "";
                }
            }
            if (_ultChargeFill != null)
            {
                bool ready = hero.Caster.UltCharge >= 100f;
                if (ready)
                {
                    // subtle ready pulse on the ult label
                    var lbl = _ultChargeFill.transform.parent.GetComponentInChildren<Text>();
                    if (lbl != null) lbl.color = Mathf.PingPong(Time.time * 2f, 1f) > 0.5f
                        ? new Color(1f, 0.95f, 0.6f) : Color.white;
                }
            }
        }

        private void UpdatePortraits()
        {
            if (_team == null) return;
            for (int i = 0; i < GameConfig.TeamSize && i < _team.Heroes.Count; i++)
            {
                var h = _team.Heroes[i];
                if (h == null) continue;

                if (_portraitFills[i] != null)
                {
                    float ratio = h.HP.IsDead ? 0f : h.HP.Ratio;
                    _portraitFills[i].rectTransform.sizeDelta = new Vector2(84f * ratio, 12f);
                    _portraitFills[i].color = h.HP.IsDead ? new Color(0.4f, 0.4f, 0.4f)
                        : ratio > 0.5f ? new Color(0.2f, 0.9f, 0.3f)
                        : ratio > 0.25f ? new Color(0.95f, 0.75f, 0.2f) : new Color(0.95f, 0.3f, 0.25f);
                }
                if (_portraitFrames[i] != null)
                {
                    _portraitFrames[i].color = i == _team.ActiveIndex
                        ? AnimeTheme.Gold
                        : (i < _team.Heroes.Count && _team.Heroes[i] != null
                            ? AnimeTheme.Rarity(_team.Heroes[i].Data.rarity)
                            : AnimeTheme.PanelRaised);
                }
            }
        }

        private void UpdateNotification()
        {
            if (_notificationText == null) return;
            if (_notificationTimer > 0f)
            {
                _notificationTimer -= Time.deltaTime;
                if (_notificationTimer <= 0f) _notificationText.text = "";
            }
        }

        private void OnNotification(string msg)
        {
            if (_notificationText == null) return;
            _notificationText.text = msg;
            _notificationTimer = 2.4f;
        }

        private void OnDamageDealt(DamageInfo info)
        {
            if (info.target == null) return;
            var pos = info.target.transform.position;

            Color c = info.isCrit ? new Color(1f, 0.35f, 0.2f) : Color.white;
            string text = Mathf.RoundToInt(info.amount).ToString() + (info.isCrit ? "!" : "");
            DamagePopup.Spawn(pos, text, c, info.isCrit);
        }

        private void OnCharacterDied(GameObject go)
        {
            if (go == null) return;
            bool isHero = go.GetComponent<HeroCharacter>() != null;
            DamagePopup.Spawn(go.transform.position, isHero ? "DOWN" : "SLAIN",
                isHero ? new Color(1f, 0.4f, 0.35f) : new Color(0.8f, 0.8f, 0.85f), true);
        }

        private void OnHeroLevelUp(int slot)
        {
            // camera micro-shake for juice
            if (_camera != null) _camera.Shake(0.25f, 0.12f);
        }

        public void SetVisible(bool visible)
        {
            if (_canvasGo != null) _canvasGo.SetActive(visible);
        }

        public void ShowFloorClearedPanel(bool show)
        {
            if (_floorClearedPanel != null) _floorClearedPanel.gameObject.SetActive(show);
        }

        // ---- attack button hold ----
        private class AttackHoldButton : MonoBehaviour,
            UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
        {
            public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
            {
                AttackButtonHeld = true;
            }

            public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
            {
                AttackButtonHeld = false;
            }

            private void OnDisable()
            {
                AttackButtonHeld = false;
            }
        }
    }
}
