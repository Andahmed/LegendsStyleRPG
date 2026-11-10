using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Characters;
using LegendsRPG.Combat;
using LegendsRPG.Core;
using LegendsRPG.Team;
using LegendsRPG.Art;

namespace LegendsRPG.UI
{
    // ====================================================================
    // SUMMON SCREEN — banners, rates, pity, single & 10x pulls with a
    // card-reveal animation and a Skip button.
    // ====================================================================
    public class SummonScreen : ScreenBase
    {
        private List<SummonSystem.PullResult> _pendingResults;
        private RectTransform _cardLayer;
        private Text _statusText;
        private float _cardTimer;
        private int _cardsShown;
        private bool _revealing;
        private readonly List<RectTransform> _liveCards = new List<RectTransform>();

        private string[] _rateLines;

        public override void Build(UIManager manager, Transform canvasParent, string title)
        {
            base.Build(manager, canvasParent, title);

            // card reveal layer sits above the scroll
            _cardLayer = UIBuilder.CreatePanel(Root, "CardLayer", new Color(0.02f, 0.02f, 0.05f, 0.96f),
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _cardLayer.SetAsLastSibling();
            _cardLayer.gameObject.SetActive(false);

            _statusText = UIBuilder.CreateText(_cardLayer, "Status", "", 34, new Color(1f, 0.9f, 0.5f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -240f), new Vector2(900f, 60f));

            UIBuilder.CreateButton(_cardLayer, "SkipBtn", "SKIP >>", new Vector2(240f, 90f),
                new Vector2(1f, 1f), new Vector2(-140f, -160f), new Color(0.35f, 0.3f, 0.45f, 0.95f),
                Color.white, 32, SkipReveal);

            UIBuilder.CreateButton(_cardLayer, "DoneBtn", "TAP TO CONTINUE", new Vector2(560f, 100f),
                new Vector2(0.5f, 0f), new Vector2(0f, 160f), new Color(0.16f, 0.45f, 0.3f, 0.95f),
                Color.white, 34, () =>
                {
                    _cardLayer.gameObject.SetActive(false);
                    ClearLiveCards();
                    Rebuild();
                });

            _rateLines = new[]
            {
                "Rates:  Mythic 1.5%   Legendary 7.5%   Epic 17.5%   Rare 29.5%   Common 44%",
                "10x pull guarantees Epic or better. 90 pulls guarantee Legendary.",
                "Duplicate heroes convert into 10 ascension shards each."
            };
        }

        public override void Rebuild()
        {
            ClearContent();

            // banner
            var banner = UIX.CreateRow(Content, "Banner", 300f);
            var bgImg = banner.GetComponent<Image>();
            bgImg.color = new Color(0.16f, 0.1f, 0.28f, 1f);
            UIX.AddChildText(banner, "BannerTitle", "LEGENDS BANNER", 52, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleCenter);
            UIX.AddChildText(banner, "BannerSub", "Call heroes from beyond the veil", 30, new Color(0.8f, 0.8f, 0.9f), TextAnchor.MiddleCenter);

            // pity info
            var pity = UIX.CreateRow(Content, "Pity", 150f);
            UIX.AddChildText(pity, "P1", "Epic pity: " + SummonSystem.EpicPityLeft + " pulls", 28, Color.white, TextAnchor.MiddleLeft, true);
            UIX.AddChildText(pity, "P2", "Legendary pity: " + SummonSystem.LegendaryPityLeft, 28, Color.white, TextAnchor.MiddleLeft, true);
            UIX.AddChildText(pity, "P3", "Total pulls: " + SummonSystem.TotalPulls, 28, new Color(0.7f, 0.8f, 1f), TextAnchor.MiddleRight, true);

            // rates text
            var rates = UIX.CreateRow(Content, "Rates", 190f);
            var rt = UIX.AddChildText(rates, "RateText", string.Join("\n", _rateLines), 26, new Color(0.85f, 0.85f, 0.95f), TextAnchor.UpperLeft, true);
            rt.alignment = TextAnchor.UpperLeft;

            // pull buttons
            var btnRow = UIX.CreateRow(Content, "Buttons", 130f, 20, 20, 16, 16);
            UIX.CreateRowButton(btnRow, "Summon x1  (" + SummonSystem.CostSingle + " Dia)",
                () => DoPull(false), new Color(0.2f, 0.45f, 0.75f), 100f, 440f, 32);
            UIX.CreateRowButton(btnRow, "Summon x10  (" + SummonSystem.CostTen + ")",
                () => DoPull(true), new Color(0.72f, 0.4f, 0.15f), 100f, 440f, 32);

            var hint = UIX.CreateRow(Content, "Hint", 80f);
            UIX.AddChildText(hint, "HintText", "Diamonds come from quests, AFK chests, arena, campaign and events.", 26,
                new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);
        }

        private void DoPull(bool ten)
        {
            if (_revealing) return;

            if (ten)
            {
                var results = SummonSystem.PullTen();
                if (results == null) return;
                _pendingResults = results;
            }
            else
            {
                var r = SummonSystem.PullOnce();
                if (r == null) return;
                _pendingResults = new List<SummonSystem.PullResult> { r };
            }

            // hero detail roster may need refresh
            StartReveal();
        }

        private void StartReveal()
        {
            _revealing = true;
            _cardsShown = 0;
            _cardTimer = 0f;
            ClearLiveCards();
            _cardLayer.gameObject.SetActive(true);
            _cardLayer.SetAsLastSibling();
            _statusText.text = _pendingResults.Count > 1 ? "Summoning 10 heroes..." : "Summoning...";
            Audio.AudioSystem.Play("Summon");
        }

        private void SkipReveal()
        {
            if (!_revealing) return;
            while (_cardsShown < _pendingResults.Count) ShowNextCard();
            FinishReveal();
        }

        private void FinishReveal()
        {
            _revealing = false;
            _statusText.text = "Tap to continue";
            Audio.AudioSystem.Play("Fanfare", 0.7f);
        }

        private void ShowNextCard()
        {
            if (_cardsShown >= _pendingResults.Count) return;
            var r = _pendingResults[_cardsShown];
            _cardsShown++;

            int count = _pendingResults.Count;
            const int perRow = 5;
            const float cardW = 176f, cardH = 240f, gapX = 16f, gapY = 22f;

            int row = (_cardsShown - 1) / perRow;
            int col = (_cardsShown - 1) % perRow;
            int rowCount = Mathf.Min(perRow, count - row * perRow);
            float rowWidth = rowCount * cardW + (rowCount - 1) * gapX;
            float startX = -rowWidth / 2f + cardW / 2f;

            var card = UIBuilder.CreateFreeRect(_cardLayer, "Card" + _cardsShown, new Vector2(cardW, cardH),
                new Vector2(0.5f, 0.5f),
                new Vector2(startX + col * (cardW + gapX) + (row == 1 && count > perRow ? 0 : 0), 140f - row * (cardH + gapY)));

            Color rc = RarityColor(r.hero.rarity);
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = UIBuilder.SolidSprite;
            bg.color = rc;

            // star row
            int stars = HeroProgress.StarsOf(r.hero.heroId);
            string starStr = new string('*', Mathf.Clamp(stars, 1, 6));
            var st = UIBuilder.CreateText(card, "Stars", starStr, 30, new Color(1f, 0.9f, 0.3f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(170f, 44f));

            // hero name + class
            var name = UIBuilder.CreateText(card, "Name", r.hero.displayName, 26, Color.white,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(170f, 40f));
            var cls = UIBuilder.CreateText(card, "Class", r.hero.heroClass + "  •  " + r.hero.element, 22,
                new Color(0.9f, 0.9f, 1f), TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, -102f), new Vector2(170f, 36f));

            // NEW badge or shard gain
            string footer = r.isNew ? "NEW HERO!" : "+" + r.shardsGranted + " shards";
            var foot = UIBuilder.CreateText(card, "Footer", footer, 24,
                r.isNew ? new Color(0.4f, 1f, 0.5f) : new Color(0.8f, 0.8f, 0.9f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(170f, 36f));

            // pop-in animation
            card.localScale = Vector3.zero;
            _liveCards.Add(card);

            _statusText.text = _cardsShown < _pendingResults.Count
                ? "Revealing " + _cardsShown + " / " + _pendingResults.Count
                : "";
            Audio.AudioSystem.Play(r.hero.rarity >= Rarity.Epic ? "Fanfare" : "UI", 0.8f);
        }

        private void ClearLiveCards()
        {
            foreach (var c in _liveCards)
            {
                if (c != null) Object.Destroy(c.gameObject);
            }
            _liveCards.Clear();
        }

        public override void Tick()
        {
            if (!_revealing) return;

            _cardTimer -= Time.unscaledDeltaTime;
            if (_cardTimer <= 0f)
            {
                _cardTimer = 0.35f;
                if (_cardsShown < _pendingResults.Count)
                {
                    ShowNextCard();
                }
                else
                {
                    FinishReveal();
                }
            }

            // pop animation on newest card
            if (_liveCards.Count > 0)
            {
                var newest = _liveCards[_liveCards.Count - 1];
                if (newest != null)
                {
                    newest.localScale = Vector3.Lerp(newest.localScale, Vector3.one, Time.unscaledDeltaTime * 10f);
                }
            }
        }

        public static Color RarityColor(Rarity r)
        {
            switch (r)
            {
                case Rarity.Common: return new Color(0.45f, 0.48f, 0.55f, 0.97f);
                case Rarity.Rare: return new Color(0.2f, 0.5f, 0.85f, 0.97f);
                case Rarity.Epic: return new Color(0.55f, 0.25f, 0.85f, 0.97f);
                case Rarity.Legendary: return new Color(0.85f, 0.6f, 0.1f, 0.97f);
                default: return new Color(0.85f, 0.25f, 0.35f, 0.97f);
            }
        }
    }

    // ====================================================================
    // HERO SCREEN — roster grid + 6-slot formation editor.
    // ====================================================================
    public class HeroScreen : ScreenBase
    {
        private List<HeroData> _roster;

        public override void Rebuild()
        {
            ClearContent();
            if (_roster == null) _roster = HeroDatabase.CreateDefaultRoster();

            var unlocked = SaveSystem.Current.unlockedHeroIds;
            var teamIds = SaveSystem.Current.teamHeroIds;

            // formation row
            var formHeader = UIX.CreateRow(Content, "FormHeader", 70f);
            UIX.AddChildText(formHeader, "T", "BATTLE FORMATION (tap a filled slot to remove)", 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            var formRow = UIX.CreateRow(Content, "Formation", 130f, 12, 12, 8, 8);
            for (int i = 0; i < GameConfig.TeamSize; i++)
            {
                string heroId = i < teamIds.Count ? teamIds[i] : null;
                HeroData data = null;
                if (heroId != null)
                {
                    foreach (var h in _roster) if (h.heroId == heroId) { data = h; break; }
                }

                var slot = new GameObject("Slot" + i);
                var rt = slot.AddComponent<RectTransform>();
                rt.SetParent(formRow, false);
                var le = slot.AddComponent<LayoutElement>();
                le.preferredWidth = 140f;

                var img = slot.AddComponent<Image>();
                img.sprite = UIBuilder.SolidSprite;
                img.color = data != null ? SummonScreen.RarityColor(data.rarity) : new Color(0.2f, 0.2f, 0.25f, 0.9f);
                var btn = slot.AddComponent<Button>();
                btn.targetGraphic = img;

                var labelGo = new GameObject("Label");
                var labelRt = labelGo.AddComponent<RectTransform>();
                labelRt.SetParent(slot.transform, false);
                labelRt.anchorMin = Vector2.zero;
                labelRt.anchorMax = Vector2.one;
                labelRt.offsetMin = Vector2.zero;
                labelRt.offsetMax = Vector2.zero;
                var lbl = labelGo.AddComponent<Text>();
                lbl.font = UIBuilder.BuiltinFont;
                lbl.text = data != null ? data.displayName : "Empty";
                lbl.fontSize = 24;
                lbl.color = Color.white;
                lbl.alignment = TextAnchor.MiddleCenter;
                lbl.raycastTarget = false;

                int index = i;
                btn.onClick.AddListener(() =>
                {
                    if (teamIds.Count > index)
                    {
                        // filled → remove (keep at least 1 hero)
                        if (teamIds.Count <= 1)
                        {
                            Manager.Notify("The team needs at least one hero!");
                            return;
                        }
                        teamIds.RemoveAt(index);
                    }
                    else
                    {
                        // empty → open picker (roster screen in pick mode)
                        Manager.Notify("Tap a hero below to add them to slot " + (index + 1));
                        _pickSlot = index;
                        Rebuild();
                        return;
                    }
                    Audio.AudioSystem.Play("UI");
                    Rebuild();
                });
            }

            // roster
            var rosterHeader = UIX.CreateRow(Content, "RosterHeader", 70f);
            UIX.AddChildText(rosterHeader, "T", "ROSTER (" + unlocked.Count + " heroes owned) — tap for details, + to add to team", 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            var owned = _roster.FindAll(h => unlocked.Contains(h.heroId));
            owned.Sort((a, b) =>
            {
                int pa = a.rarity.CompareTo(b.rarity);
                if (pa != 0) return -pa;
                return HeroProgress.LevelOf(b.heroId).CompareTo(HeroProgress.LevelOf(a.heroId));
            });

            int perRow = 3;
            for (int i = 0; i < owned.Count; i += perRow)
            {
                var row = UIX.CreateRow(Content, "HeroRow" + i, 120f, 10, 10, 6, 6);
                for (int j = 0; j < perRow && i + j < owned.Count; j++)
                {
                    var hero = owned[i + j];
                    var cell = new GameObject("Cell_" + hero.displayName);
                    var rt = cell.AddComponent<RectTransform>();
                    rt.SetParent(row, false);
                    var le = cell.AddComponent<LayoutElement>();
                    le.flexibleWidth = 1f;

                    var img = cell.AddComponent<Image>();
                    img.sprite = UIBuilder.SolidSprite;
                    img.color = new Color(0.15f, 0.16f, 0.22f, 0.98f);

                    var labelGo = new GameObject("Label");
                    var labelRt = labelGo.AddComponent<RectTransform>();
                    labelRt.SetParent(cell.transform, false);
                    labelRt.anchorMin = Vector2.zero;
                    labelRt.anchorMax = Vector2.one;
                    labelRt.offsetMin = Vector2.zero;
                    labelRt.offsetMax = Vector2.zero;
                    var txt = labelGo.AddComponent<Text>();
                    txt.font = UIBuilder.BuiltinFont;
                    int stars = HeroProgress.StarsOf(hero.heroId);
                    txt.text = hero.displayName + "\nLv." + HeroProgress.LevelOf(hero.heroId) + " " +
                               new string('*', Mathf.Clamp(stars, 1, 6)) + "\n" +
                               hero.rarity + " " + hero.element;
                    txt.fontSize = 24;
                    txt.color = SummonScreen.RarityColor(hero.rarity);
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.raycastTarget = false;

                    var detail = cell.AddComponent<Button>();
                    detail.targetGraphic = img;
                    detail.onClick.AddListener(() =>
                    {
                        // if a slot is being picked, fill it
                        if (_pickSlot >= 0)
                        {
                            if (!teamIds.Contains(hero.heroId))
                            {
                                if (_pickSlot < teamIds.Count) teamIds[_pickSlot] = hero.heroId;
                                else teamIds.Add(hero.heroId);
                                _pickSlot = -1;
                                Audio.AudioSystem.Play("Equip");
                                Manager.CloseScreen();
                                // apply formation immediately
                                TeamManager.InstanceRef.CreateTeam(_roster, TeamManager.InstanceRef.BasePosition, false);
                                return;
                            }
                            _pickSlot = -1;
                        }
                        Manager.OpenHeroDetail(hero.heroId);
                    });
                }
            }
        }

        private int _pickSlot = -1;
    }

    // ====================================================================
    // HERO DETAIL — live 3D model on a rotating podium attached to the
    // camera, stats, level-up, ascension, equipment, skills, rune info.
    // ====================================================================
    public class HeroDetailScreen : ScreenBase
    {
        private string _heroId;
        private GameObject _display;
        private HeroData _hero;
        private Text _powerText;

        public void SetHero(string heroId)
        {
            _heroId = heroId;
        }

        public override void Open()
        {
            base.Open();
            SpawnDisplay();
        }

        public override void Close()
        {
            DestroyDisplay();
            base.Close();
        }

        private void SpawnDisplay()
        {
            DestroyDisplay();
            if (string.IsNullOrEmpty(_heroId)) return;

            foreach (var h in HeroDatabase.CreateDefaultRoster())
            {
                if (h.heroId == _heroId) { _hero = h; break; }
            }
            if (_hero == null) return;

            var cam = Camera.main;
            if (cam == null) return;

            _display = new GameObject("HeroDisplay");
            _display.transform.SetParent(cam.transform, false);
            _display.transform.localPosition = new Vector3(0.85f, -1.15f, 4.6f);
            _display.transform.localScale = Vector3.one * 0.95f;

            var body = CharacterBodyFactory.Build(_display, _hero.element, true, 1f,
                _hero.primaryColor, _hero.secondaryColor);

            // Use the same imported, rigged class model as the battle team.
            // The procedural model remains available as a safe fallback.
            var importedModel = QuaterniusAssetLibrary.InstantiateHero(_hero.heroClass, _display.transform, 2.35f);
            if (importedModel != null)
            {
                QuaterniusAssetLibrary.HideProceduralBody(body);
                _display.name = "HeroDisplay_" + _hero.heroClass;
            }
            else
            {
                Debug.LogWarning("[HeroDetail] Imported model unavailable for " + _hero.heroClass +
                    "; using procedural hero display.");
            }

            // podium disc under the model
            var podium = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(podium.GetComponent<Collider>());
            podium.transform.SetParent(_display.transform, false);
            podium.transform.localPosition = new Vector3(0f, -1.15f, 0f);
            podium.transform.localScale = new Vector3(1.7f, 0.1f, 1.7f);
            podium.GetComponent<MeshRenderer>().material =
                Vfx.MaterialLibrary.Emissive(SummonScreen.RarityColor(_hero.rarity), 1.2f);
        }

        private void DestroyDisplay()
        {
            if (_display != null)
            {
                Object.Destroy(_display);
                _display = null;
            }
        }

        public override void Tick()
        {
            if (_display != null)
            {
                _display.transform.localRotation *= Quaternion.Euler(0f, 40f * Time.unscaledDeltaTime, 0f);
            }
            if (_powerText != null && _hero != null)
            {
                _powerText.text = "Power " + Economy.Format(HeroProgress.PowerOf(_hero)) +
                                  "   •   Team bonus active";
            }
        }

        public override void Rebuild()
        {
            ClearContent();
            if (string.IsNullOrEmpty(_heroId) || _hero == null)
            {
                foreach (var h in HeroDatabase.CreateDefaultRoster())
                {
                    if (h.heroId == _heroId) { _hero = h; break; }
                }
            }
            if (_hero == null)
            {
                UIX.AddChildText(Content, "None", "No hero selected.", 32, Color.white, TextAnchor.MiddleCenter, true);
                return;
            }

            int level = HeroProgress.LevelOf(_heroId);
            int stars = HeroProgress.StarsOf(_heroId);
            int shards = HeroProgress.ShardsOf(_heroId);

            // header card
            var head = UIX.CreateRow(Content, "Head", 190f);
            var headImg = head.GetComponent<Image>();
            headImg.color = SummonScreen.RarityColor(_hero.rarity) * new Color(1f, 1f, 1f, 0.35f);
            UIX.AddChildText(head, "Name", _hero.displayName, 46, Color.white, TextAnchor.MiddleLeft, true);
            var infoCol = UIX.AddChildText(head, "Info",
                _hero.heroClass + "  •  " + _hero.element + "  •  " + _hero.rarity + "\n" +
                "Level " + level + "   " + new string('*', Mathf.Clamp(stars, 1, 6)) + "\n" +
                "Shards: " + shards + " / " + HeroProgress.ShardsNeededForNextStar(stars),
                28, new Color(0.9f, 0.9f, 1f), TextAnchor.MiddleRight, true);

            _powerText = UIX.AddChildText(head, "Power", "", 34, new Color(0.6f, 0.95f, 1f), TextAnchor.MiddleRight, true);

            // stats
            var stats = UIX.CreateRow(Content, "Stats", 120f);
            var st = UIX.AddChildText(stats, "ST",
                "HP " + Mathf.RoundToInt((_hero.baseHP + _hero.hpPerLevel * (level - 1)) * HeroProgress.StarMult(stars)) +
                "   ATK " + Mathf.RoundToInt((_hero.baseAttack + _hero.atkPerLevel * (level - 1)) * HeroProgress.StarMult(stars)) +
                "   DEF " + Mathf.RoundToInt((_hero.baseDefense + _hero.defPerLevel * (level - 1)) * HeroProgress.StarMult(stars)),
                28, Color.white, TextAnchor.MiddleLeft, true);

            // action buttons
            var act = UIX.CreateRow(Content, "Actions", 130f, 16, 16, 10, 10);
            UIX.CreateRowButton(act, "LEVEL UP\n" + Economy.Format(HeroProgress.LevelUpCost(_hero)) + " gold",
                () => { if (HeroProgress.TryLevelUp(_hero)) Rebuild(); },
                new Color(0.2f, 0.5f, 0.3f), 110f, 300f, 26);
            UIX.CreateRowButton(act, stars >= HeroProgress.MaxStar ? "MAX STAR" :
                    "ASCEND\n" + HeroProgress.ShardsNeededForNextStar(stars) + " shards + " +
                    Economy.Format(HeroProgress.StarUpGoldCost(stars, _hero.rarity)) + " gold",
                () => { if (HeroProgress.TryStarUp(_hero)) { Rebuild(); SpawnDisplay(); } },
                new Color(0.75f, 0.5f, 0.15f), 110f, 300f, 26);

            // equipment
            var eqHeader = UIX.CreateRow(Content, "EqHeader", 64f);
            UIX.AddChildText(eqHeader, "T", "EQUIPMENT (tap to open inventory & equip)", 28,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            string[] slotNames = { "Weapon", "Armor", "Amulet", "Wings", "Pet", "Mount" };
            for (int s = 0; s < Items.InventorySystem.SlotCount; s++)
            {
                int slot = s;
                string itemName = Items.InventorySystem.GetEquippedName(_heroId, slot);
                var row = UIX.CreateRow(Content, "Eq" + slot, 78f);
                UIX.AddChildText(row, "Slot", slotNames[slot], 26, new Color(0.7f, 0.75f, 0.85f), TextAnchor.MiddleLeft, true, 200f);
                UIX.AddChildText(row, "Item", string.IsNullOrEmpty(itemName) ? "— empty —" : itemName + " (+" + ForgeSystem.LevelOf(itemName) + ")",
                    26, string.IsNullOrEmpty(itemName) ? new Color(0.5f, 0.5f, 0.55f) : Color.white, TextAnchor.MiddleLeft, true);
                UIX.CreateRowButton(row, "CHANGE", () => Manager.Open("inventory"), new Color(0.25f, 0.35f, 0.55f), 60f, 170f, 24);
            }

            // skills
            var skHeader = UIX.CreateRow(Content, "SkHeader", 64f);
            UIX.AddChildText(skHeader, "T", "SKILLS", 28, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            AddSkillRow("Skill 1", _hero.skill1);
            AddSkillRow("Skill 2", _hero.skill2);
            AddSkillRow("ULTIMATE", _hero.ultimate);

            // runes (star-unlocked, real stat effects via StarMult)
            var runeHeader = UIX.CreateRow(Content, "RuneHeader", 64f);
            UIX.AddChildText(runeHeader, "T", "RUNE SOCKETS (unlocked by ascension)", 28,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            string[] runeNames = { "Rune of Ember", "Rune of Tides", "Rune of Gale", "Rune of Dawn", "Rune of Void", "Rune of Light" };
            for (int i = 0; i < 6; i++)
            {
                var row = UIX.CreateRow(Content, "Rune" + i, 70f);
                bool socketed = stars > i;
                UIX.AddChildText(row, "R", runeNames[i], 26, socketed ? new Color(0.6f, 1f, 0.6f) : new Color(0.45f, 0.45f, 0.5f),
                    TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "V", socketed ? "+14% all stats (ascension " + (i + 1) + "★)" : "Locked — ascend to " + (i + 1) + "★",
                    24, socketed ? Color.white : new Color(0.5f, 0.5f, 0.55f), TextAnchor.MiddleRight, true);
            }

            var backRow = UIX.CreateRow(Content, "Back", 100f, 16, 16, 10, 10);
            UIX.CreateRowButton(backRow, "BACK TO ROSTER", () => Manager.Open("heroes"), new Color(0.3f, 0.35f, 0.5f), 80f, 500f, 30);
        }

        private void AddSkillRow(string label, SkillData skill)
        {
            if (skill == null) return;
            var row = UIX.CreateRow(Content, "Skill_" + skill.skillId, 96f);
            UIX.AddChildText(row, "Kind", label, 26, new Color(0.7f, 0.75f, 0.85f), TextAnchor.MiddleLeft, true, 190f);
            UIX.AddChildText(row, "Name", skill.displayName + "\n" +
                skill.powerMultiplier.ToString("0.0") + "x ATK  •  " + skill.cooldownSeconds.ToString("0.#") + "s CD  •  " +
                (skill.IsUltimate ? "Ultimate" : skill.kind),
                26, Color.white, TextAnchor.MiddleLeft, true);
            var dot = new GameObject("ColorDot");
            var rt = dot.AddComponent<RectTransform>();
            rt.SetParent(row, false);
            var le = dot.AddComponent<LayoutElement>();
            le.preferredWidth = 44f;
            var img = dot.AddComponent<Image>();
            img.sprite = UIBuilder.SolidSprite;
            img.color = skill.vfxColor;
        }
    }
}
