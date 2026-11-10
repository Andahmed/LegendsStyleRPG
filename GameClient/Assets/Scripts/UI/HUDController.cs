using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Cam;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;
using LegendsRPG.Team;

namespace LegendsRPG.UI
{
    /// <summary>
    /// v1.1 battle HUD — Legends Reborn style, fully autonomous combat:
    ///  - bottom hero portraits: stars, HP bar, Rage bar and an AUTO state chip;
    ///    when the Rage bar is full the portrait glows and tapping it fires
    ///    that hero's Ultimate,
    ///  - AUTO toggle (auto-casts skills + ultimates when on),
    ///  - battle speed x1/x2/x3, pause, battle timer on top,
    ///  - side camera buttons (angle / reset — no drag rotation),
    ///  - boss HP bar, floor/difficulty header, FPS meter, notifications,
    ///    flying damage numbers, kill-streak banner and the floor-cleared panel.
    /// There is no joystick, no manual attack and no hero switching anymore.
    /// Every visible button is functional.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        /// <summary>Battle speed selected in the HUD; restored by GameManager.Pause(false).</summary>
        public static float BattleTimeScale = 1f;

        private static readonly float[] Speeds = { 1f, 2f, 3f };
        private const int MaxPortraits = 8;

        private TeamManager _team;
        private IsometricCameraController _camera;

        private Canvas _canvas;
        private Text _floorText;
        private Text _difficultyText;
        private Text _fpsText;
        private Text _notificationText;
        private Text _killStreakText;
        private Text _timerText;
        private float _battleSeconds;
        private float _notificationTimer;
        private float _killStreakTimer;
        private float _fpsRefresh;

        // portraits
        private readonly Image[] _portraitFrames = new Image[MaxPortraits];
        private readonly Image[] _portraitFills = new Image[MaxPortraits];   // HP fill
        private readonly Image[] _portraitRage = new Image[MaxPortraits];    // rage fill
        private readonly Image[] _portraitGlows = new Image[MaxPortraits];   // glow behind frame when rage ready
        private readonly Text[] _portraitAuto = new Text[MaxPortraits];      // AUTO state chip

        private Text _autoLabel;
        private Text _speedLabel;
        private Text _pauseLabel;
        private int _speedIndex;

        private RectTransform _floorClearedPanel;
        private Text _floorClearedLabel;

        // boss bar
        private Image _bossHpFill;
        private Text _bossNameText;

        // kill streak
        private int _killStreak;
        private float _killStreakWindow;

        public void Build(TeamManager team, IsometricCameraController camera)
        {
            _team = team;
            _camera = camera;

            var canvas = UIBuilder.CreateCanvas("HUDCanvas");
            _canvas = canvas;
            var safeArea = UIBuilder.CreateSafeArea(canvas.transform, "BattleHUDSafeArea");

            BuildHeader(safeArea);
            BuildTeamPortraits(safeArea);
            BuildControlColumn(safeArea);
            BuildFloorClearedPanel(safeArea);
            BuildBattleExtras(safeArea);

            _notificationText = UIBuilder.CreateText(safeArea, "Notification", "", 34,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -320f),
                new Vector2(900f, 60f));

            _killStreakText = UIBuilder.CreateText(safeArea, "KillStreak", "", 56,
                new Color(1f, 0.45f, 0.25f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0f, 190f),
                new Vector2(800f, 80f));

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
            var bar = UIBuilder.CreatePanel(parent, "TopBar", new Color(0f, 0f, 0f, 0.55f),
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -110f), new Vector2(0f, 0f));

            _difficultyText = UIBuilder.CreateText(bar, "Difficulty", "EASY", 30,
                new Color(1f, 0.8f, 0.35f), TextAnchor.MiddleLeft, new Vector2(0f, 0.5f),
                new Vector2(28f, 0f), new Vector2(360f, 44f));

            // v1.1: battle timer lives on top center
            _timerText = UIBuilder.CreateText(bar, "BattleTimer", "0:00", 40,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(280f, 50f));

            _fpsText = UIBuilder.CreateText(bar, "FPS", "60 FPS", 24,
                new Color(0.6f, 1f, 0.6f), TextAnchor.MiddleRight, new Vector2(1f, 0.5f),
                new Vector2(-28f, 0f), new Vector2(160f, 40f));

            _floorText = UIBuilder.CreateText(bar, "Floor", "Floor 1", 26,
                new Color(0.85f, 0.87f, 0.95f), TextAnchor.MiddleRight, new Vector2(1f, 0.5f),
                new Vector2(-200f, 0f), new Vector2(260f, 40f));
        }

        private void BuildTeamPortraits(Transform parent)
        {
            int n = Mathf.Clamp(GameConfig.TeamSize, 1, MaxPortraits);
            var row = UIBuilder.CreateFreeRect(parent, "TeamRow", new Vector2(n * 118f + 10f, 130f),
                new Vector2(0f, 0f), new Vector2(28f, 130f));

            for (int i = 0; i < n; i++)
            {
                Vector2 anchor = new Vector2(0f, 0.5f);
                Vector2 pos = new Vector2(58f + i * 118f, 0f);

                // glow behind the portrait (invisible until rage is full)
                var glowRt = UIBuilder.CreateFreeRect(row, "Glow" + i, new Vector2(112f, 112f), anchor, pos);
                var glowImg = glowRt.gameObject.AddComponent<Image>();
                glowImg.sprite = UIBuilder.SolidSprite;
                glowImg.color = new Color(1f, 0.8f, 0.25f, 0f);
                glowImg.raycastTarget = false;
                _portraitGlows[i] = glowImg;

                var slot = UIBuilder.CreateFreeRect(row, "Portrait" + i, new Vector2(100f, 100f), anchor, pos);

                var frame = slot.gameObject.AddComponent<Image>();
                frame.sprite = UIBuilder.SolidSprite;
                frame.color = new Color(0.16f, 0.18f, 0.24f, 0.92f);
                _portraitFrames[i] = frame;

                // ascension stars (rarity + 1), drawn as small gold diamonds
                if (_team != null && i < _team.Heroes.Count && _team.Heroes[i].Data != null)
                {
                    int stars = Mathf.Clamp((int)_team.Heroes[i].Data.rarity + 1, 1, 5);
                    for (int s = 0; s < stars; s++)
                    {
                        var st = UIBuilder.CreateFreeRect(slot, "Star" + s, new Vector2(13f, 13f),
                            new Vector2(0.5f, 1f), new Vector2((s - (stars - 1) * 0.5f) * 17f, -9f));
                        var stImg = st.gameObject.AddComponent<Image>();
                        stImg.sprite = UIBuilder.SolidSprite;
                        stImg.color = new Color(1f, 0.82f, 0.3f, 0.95f);
                        stImg.raycastTarget = false;
                        st.localRotation = Quaternion.Euler(0f, 0f, 45f);
                    }
                }

                // HP bar (upper) + Rage bar (lower)
                Image hpFill, rageFill;
                BuildBar(slot, "Hp", new Vector2(0f, 8f), 86f, 10f, new Color(0.15f, 0.85f, 0.3f), out hpFill);
                BuildBar(slot, "Rage", new Vector2(0f, -6f), 86f, 8f, new Color(0.95f, 0.5f, 0.15f), out rageFill);
                _portraitFills[i] = hpFill;
                _portraitRage[i] = rageFill;

                // AUTO state chip (top-right of the portrait)
                var autoTxt = UIBuilder.CreateText(slot, "AutoState", "A", 22,
                    new Color(0.6f, 0.65f, 0.7f, 0.7f), TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-3f, -2f),
                    new Vector2(30f, 30f));
                _portraitAuto[i] = autoTxt;

                int index = i;
                var btn = slot.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => OnPortraitTapped(index));
            }
        }

        private void BuildBar(Transform parent, string name, Vector2 pos, float w, float h,
            Color color, out Image fillImg)
        {
            var bg = UIBuilder.CreateFreeRect(parent, name + "Bg", new Vector2(w, h),
                new Vector2(0.5f, 0.5f), pos);
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.sprite = UIBuilder.SolidSprite;
            bgImg.color = new Color(0f, 0f, 0f, 0.75f);
            bgImg.raycastTarget = false;

            var fillGo = new GameObject(name + "Fill");
            var frt = fillGo.AddComponent<RectTransform>();
            frt.SetParent(bg, false);
            frt.anchorMin = new Vector2(0f, 0.5f);
            frt.anchorMax = new Vector2(0f, 0.5f);
            frt.pivot = new Vector2(0f, 0.5f);       // left-edge pivot: bars anchored to edges never clip off-screen
            frt.sizeDelta = new Vector2(w - 4f, h - 4f);
            frt.anchoredPosition = new Vector2(2f, 0f);
            fillImg = fillGo.AddComponent<Image>();
            fillImg.sprite = UIBuilder.SolidSprite;
            fillImg.color = color;
            fillImg.raycastTarget = false;
        }

        /// <summary>Right control column: AUTO, speed, pause, camera buttons, retreat.</summary>
        private void BuildControlColumn(Transform parent)
        {
            var autoBtn = UIBuilder.CreateButton(parent, "AutoBtn", "AUTO\nOFF", new Vector2(170f, 120f),
                new Vector2(1f, 0f), new Vector2(-110f, 170f), new Color(0.2f, 0.25f, 0.3f, 0.95f),
                Color.white, 30, ToggleAuto);
            _autoLabel = autoBtn.GetComponentInChildren<Text>();

            var speedBtn = UIBuilder.CreateButton(parent, "SpeedBtn", "x1", new Vector2(120f, 100f),
                new Vector2(1f, 0f), new Vector2(-110f, 305f), new Color(0.15f, 0.45f, 0.5f, 0.95f),
                Color.white, 40, CycleSpeed);
            _speedLabel = speedBtn.GetComponentInChildren<Text>();

            var pauseBtn = UIBuilder.CreateButton(parent, "PauseBtn", "PAUSE", new Vector2(120f, 100f),
                new Vector2(1f, 0f), new Vector2(-110f, 425f), new Color(0.25f, 0.28f, 0.34f, 0.95f),
                Color.white, 30, TogglePause);
            _pauseLabel = pauseBtn.GetComponentInChildren<Text>();

            // v1.1 camera buttons (no drag rotation anymore)
            UIBuilder.CreateButton(parent, "CamAngleBtn", "ANGLE", new Vector2(120f, 100f),
                new Vector2(1f, 0f), new Vector2(-250f, 425f), new Color(0.3f, 0.3f, 0.45f, 0.95f),
                Color.white, 28, () =>
                {
                    if (_camera != null) _camera.CycleAngle();
                });
            UIBuilder.CreateButton(parent, "CamResetBtn", "RESET", new Vector2(120f, 100f),
                new Vector2(1f, 0f), new Vector2(-250f, 305f), new Color(0.3f, 0.3f, 0.45f, 0.95f),
                Color.white, 28, () =>
                {
                    if (_camera != null) _camera.ResetView();
                });

            UIBuilder.CreateButton(parent, "RetreatBtn", "RETREAT", new Vector2(170f, 90f),
                new Vector2(1f, 0f), new Vector2(-110f, 545f), new Color(0.5f, 0.22f, 0.2f, 0.95f),
                Color.white, 28, () => Core.BattleSession.Retreat());
        }

        private void BuildFloorClearedPanel(Transform parent)
        {
            _floorClearedPanel = UIBuilder.CreatePanel(parent, "FloorCleared", new Color(0f, 0f, 0f, 0.75f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _floorClearedPanel.gameObject.SetActive(false);

            _floorClearedLabel = UIBuilder.CreateText(_floorClearedPanel, "Title", "FLOOR CLEARED!", 64,
                new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 160f), new Vector2(900f, 90f));

            UIBuilder.CreateText(_floorClearedPanel, "Sub", "Loot & EXP collected", 34,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 60f), new Vector2(700f, 50f));

            UIBuilder.CreateButton(_floorClearedPanel, "NextBtn", "NEXT FLOOR", new Vector2(420f, 120f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Color(0.15f, 0.6f, 0.3f, 0.98f),
                Color.white, 42, () =>
                {
                    if (GameManager.Instance != null) GameManager.Instance.StartNextFloor();
                });
        }

        private void BuildBattleExtras(Transform parent)
        {
            // battle title under the top bar
            UIBuilder.CreateText(parent, "BattleTitle", "", 30,
                new Color(1f, 0.9f, 0.6f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f),
                new Vector2(0f, -150f), new Vector2(1000f, 44f));

            // boss HP bar (top center, below title)
            var bossBg = UIBuilder.CreateFreeRect(parent, "BossBar", new Vector2(640f, 54f),
                new Vector2(0.5f, 1f), new Vector2(0f, -220f));
            var bgImg = bossBg.gameObject.AddComponent<Image>();
            bgImg.sprite = UIBuilder.SolidSprite;
            bgImg.color = new Color(0, 0, 0, 0.6f);
            _bossNameText = UIBuilder.CreateText(bossBg, "BossName", "", 24, new Color(1f, 0.75f, 0.5f),
                TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(10f, -26f), new Vector2(620f, 26f));

            var fillGo = new GameObject("BossFill");
            var frt = fillGo.AddComponent<RectTransform>();
            frt.SetParent(bossBg, false);
            frt.anchorMin = new Vector2(0f, 0f);
            frt.anchorMax = new Vector2(0f, 0f);
            frt.pivot = new Vector2(0f, 0f);
            frt.sizeDelta = new Vector2(636f, 10f);
            frt.anchoredPosition = new Vector2(2f, 2f);
            _bossHpFill = fillGo.AddComponent<Image>();
            _bossHpFill.sprite = UIBuilder.SolidSprite;
            _bossHpFill.color = new Color(0.9f, 0.25f, 0.2f);
            _bossHpFill.raycastTarget = false;

            bossBg.gameObject.SetActive(false);
        }

        // ================= behavior =================
        /// <summary>v1.1: tapping a portrait fires THAT hero's Ultimate when its Rage is full.</summary>
        private void OnPortraitTapped(int index)
        {
            if (_team == null || index < 0 || index >= _team.Heroes.Count) return;
            var hero = _team.Heroes[index];
            if (hero == null || hero.HP == null || hero.HP.IsDead || hero.Caster == null) return;

            if (hero.Caster.UltCharge < 100f)
            {
                Toast(hero.Data.displayName + ": Rage not ready");
                Audio.AudioSystem.Play("UI");
                return;
            }

            if (hero.Caster.TryCast(2, null))
            {
                Audio.AudioSystem.Play("UI");
            }
        }

        private void ToggleAuto()
        {
            if (AutoSkills.Instance != null)
            {
                AutoSkills.Instance.Toggle();
                if (_autoLabel != null)
                {
                    _autoLabel.text = AutoSkills.Instance.Enabled ? "AUTO\nON" : "AUTO\nOFF";
                }
            }
        }

        private void CycleSpeed()
        {
            _speedIndex = (_speedIndex + 1) % Speeds.Length;
            BattleTimeScale = Speeds[_speedIndex];
            // while paused the game stays frozen; the speed applies on resume
            if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Paused)
            {
                Time.timeScale = BattleTimeScale;
            }
            if (_speedLabel != null) _speedLabel.text = "x" + (_speedIndex + 1);
            Audio.AudioSystem.Play("UI");
        }

        private void TogglePause()
        {
            if (GameManager.Instance == null) return;
            bool paused = GameManager.Instance.State == GameManager.GameState.Paused;
            GameManager.Instance.Pause(!paused);
            if (_pauseLabel != null) _pauseLabel.text = paused ? "PAUSE" : "RESUME";
        }

        /// <summary>Shows/hides the whole HUD canvas (used by the menu screens).</summary>
        public void SetVisible(bool visible)
        {
            if (_canvas != null) _canvas.gameObject.SetActive(visible);
        }

        /// <summary>City mode hides the whole battle HUD; battle mode shows it (and resets the timer).</summary>
        public void SetBattleHudVisible(bool visible, string title)
        {
            if (_canvas != null) _canvas.gameObject.SetActive(visible);
            if (visible)
            {
                _battleSeconds = 0f;
                if (_timerText != null) _timerText.text = "0:00";
            }
            else
            {
                BattleTimeScale = 1f;
                Time.timeScale = 1f;
                if (_speedLabel != null) _speedLabel.text = "x1";
                _speedIndex = 0;
            }
        }

        private void Update()
        {
            UpdateHeader();
            UpdateTimer();
            UpdatePortraits();
            UpdateNotification();
            UpdateKillStreak();
            UpdateBossBar();
        }

        private void UpdateHeader()
        {
            if (GameManager.Instance == null || GameManager.Instance.Tower == null) return;
            var tower = GameManager.Instance.Tower;
            if (_floorText != null) _floorText.text = "Floor " + tower.CurrentFloor;
            if (_difficultyText != null) _difficultyText.text = World.TowerSystem.DifficultyName(tower.Difficulty).ToUpper();

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

        // v1.1: battle timer — scaled time, so it freezes while paused and
        // counts at the selected battle speed (x2 counts twice as fast).
        private void UpdateTimer()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Battle) return;
            _battleSeconds += Time.deltaTime;
            if (_timerText != null)
            {
                int m = (int)(_battleSeconds / 60f);
                int s = (int)(_battleSeconds % 60f);
                _timerText.text = m + ":" + s.ToString("00");
            }
        }

        private void UpdatePortraits()
        {
            if (_team == null) return;
            bool autoOn = AutoSkills.Instance != null && AutoSkills.Instance.Enabled;

            for (int i = 0; i < GameConfig.TeamSize && i < _team.Heroes.Count && i < MaxPortraits; i++)
            {
                var h = _team.Heroes[i];
                if (h == null || h.HP == null) continue;

                float ratio = h.HP.IsDead ? 0f : h.HP.Ratio;
                if (_portraitFills[i] != null)
                {
                    _portraitFills[i].rectTransform.sizeDelta = new Vector2(82f * ratio, 6f);
                    _portraitFills[i].color = h.HP.IsDead ? new Color(0.4f, 0.4f, 0.4f)
                        : ratio > 0.5f ? new Color(0.15f, 0.85f, 0.3f)
                        : ratio > 0.25f ? new Color(0.95f, 0.75f, 0.2f) : new Color(0.95f, 0.3f, 0.25f);
                }

                float rage = (h.Caster != null) ? Mathf.Clamp01(h.Caster.UltCharge / 100f) : 0f;
                bool ready = rage >= 1f && !h.HP.IsDead;
                if (_portraitRage[i] != null)
                {
                    _portraitRage[i].rectTransform.sizeDelta = new Vector2(82f * rage, 4f);
                }
                if (_portraitGlows[i] != null)
                {
                    float a = ready ? 0.32f + 0.28f * Mathf.PingPong(Time.unscaledTime * 3f, 1f) : 0f;
                    var c = _portraitGlows[i].color;
                    c.a = a;
                    _portraitGlows[i].color = c;
                }
                if (_portraitFrames[i] != null)
                {
                    _portraitFrames[i].color = ready ? new Color(0.62f, 0.48f, 0.14f, 0.98f)
                        : h.HP.IsDead ? new Color(0.18f, 0.18f, 0.2f, 0.8f)
                        : new Color(0.16f, 0.18f, 0.24f, 0.92f);
                }
                if (_portraitAuto[i] != null)
                {
                    _portraitAuto[i].color = autoOn ? new Color(0.4f, 1f, 0.5f, 0.95f)
                        : new Color(0.6f, 0.65f, 0.7f, 0.55f);
                }
            }
        }

        private void UpdateNotification()
        {
            if (_notificationText == null) return;
            if (_notificationTimer > 0f)
            {
                _notificationTimer -= Time.unscaledDeltaTime;
                if (_notificationTimer <= 0f) _notificationText.text = "";
            }
        }

        // v1.1: kill streak banner (2KILL / 3KILL / ... like the reference game)
        private void UpdateKillStreak()
        {
            if (_killStreakWindow > 0f)
            {
                _killStreakWindow -= Time.unscaledDeltaTime;
                if (_killStreakWindow <= 0f) _killStreak = 0;
            }
            if (_killStreakTimer > 0f)
            {
                _killStreakTimer -= Time.unscaledDeltaTime;
                if (_killStreakTimer <= 0f && _killStreakText != null) _killStreakText.text = "";
            }
        }

        private void ShowKillStreak(int streak)
        {
            if (_killStreakText == null) return;
            string label;
            if (streak == 2) label = "2 KILL!";
            else if (streak == 3) label = "3 KILL!";
            else if (streak == 4) label = "4 KILL!";
            else if (streak == 5) label = "5 KILL!";
            else label = "RAMPAGE x" + streak;
            _killStreakText.text = label;
            _killStreakTimer = 1.4f;
        }

        private void UpdateBossBar()
        {
            if (_bossHpFill == null) return;

            MonsterCharacter boss = null;
            foreach (var m in MonsterCharacter.All)
            {
                if (m != null && m.Data != null && m.Data.isBoss && !m.HP.IsDead) { boss = m; break; }
            }

            var bar = _bossHpFill.transform.parent.gameObject;
            if (boss == null)
            {
                if (bar.activeSelf) bar.SetActive(false);
                return;
            }

            if (!bar.activeSelf) bar.SetActive(true);
            if (_bossNameText != null) _bossNameText.text = boss.Data.displayName;
            _bossHpFill.rectTransform.sizeDelta = new Vector2(636f * boss.HP.Ratio, 10f);
        }

        private void Toast(string msg)
        {
            if (_notificationText == null) return;
            _notificationText.text = msg;
            _notificationTimer = 1.6f;
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

            if (isHero)
            {
                _killStreak = 0;   // a hero death breaks the streak
            }
            else
            {
                _killStreak++;
                _killStreakWindow = 4f;
                if (_killStreak >= 2) ShowKillStreak(_killStreak);
            }
        }

        private void OnHeroLevelUp(int slot)
        {
            // camera micro-shake for juice
            if (_camera != null) _camera.Shake(0.25f, 0.12f);
        }

        public void ShowFloorClearedPanel(bool show)
        {
            if (_floorClearedPanel != null) _floorClearedPanel.gameObject.SetActive(show);
            if (_floorClearedLabel != null) _floorClearedLabel.text = "FLOOR CLEARED!";
        }
    }
}
