using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Core;
using LegendsRPG.Social;
using LegendsRPG.World;

namespace LegendsRPG.UI
{
    // ====================================================================
    // DAILY QUESTS
    // ====================================================================
    public class QuestScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            var head = UIX.CreateRow(Content, "Head", 90f);
            UIX.AddChildText(head, "T", "DAILY QUESTS — reset at 00:00 UTC", 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            foreach (var q in QuestSystem.Daily)
            {
                int prog = QuestSystem.ProgressOf(q.id);
                bool claimable = QuestSystem.IsClaimable(q);
                bool claimed = prog >= q.target && !claimable;

                var row = UIX.CreateRow(Content, "Q_" + q.id, 120f);
                UIX.AddChildText(row, "Title", q.title, 28, Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "Reward",
                    "+" + Economy.Format(q.goldReward) + "g  +" + q.diamondReward + "d",
                    26, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleRight, true, 240f);

                var bar = UIX.CreateLayoutBar(row, "Bar", claimable ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.3f, 0.6f, 0.9f), 120f, 16f);
                bar.rectTransform.sizeDelta = new Vector2(120f * Mathf.Clamp01((float)prog / q.target), 0f);

                var state = claimed ? "DONE" : claimable ? "CLAIM" : prog + "/" + q.target;
                var btnColor = claimed ? new Color(0.3f, 0.32f, 0.38f) : claimable ? new Color(0.2f, 0.65f, 0.3f) : new Color(0.25f, 0.28f, 0.4f);
                var btn = UIX.CreateRowButton(row, state,
                    () =>
                    {
                        if (QuestSystem.TryClaim(q)) { Rebuild(); }
                        else if (!claimed) { Manager.Notify("Progress " + prog + "/" + q.target); }
                    },
                    btnColor, 76f, 200f, 26);
                btn.interactable = claimable;
            }
        }
    }

    // ====================================================================
    // SHOP — daily deals / merchant link / free diamonds.
    // ====================================================================
    public class ShopScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            ShopSystem.EnsureFresh();

            var head = UIX.CreateRow(Content, "Head", 110f);
            UIX.AddChildText(head, "T", "SHOP — refreshes in " + UIX.TimeShort(ShopSystem.TimeToRefresh()), 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            foreach (var slot in ShopSystem.Slots)
            {
                var row = UIX.CreateRow(Content, "Slot_" + slot.key, 130f);
                string name;
                if (!string.IsNullOrEmpty(slot.itemId))
                {
                    var item = Items.ItemDatabase.Get(slot.itemId);
                    name = (item != null ? item.displayName : slot.itemId) + (slot.discountPct > 0 ? "  (-" + slot.discountPct + "%)" : "");
                }
                else if (slot.heroShardsFor > 0)
                {
                    name = "Hero Shards x" + slot.heroShardsFor + (slot.discountPct > 0 ? "  (-" + slot.discountPct + "%)" : "");
                }
                else
                {
                    name = "Gold Bag " + Economy.Format(slot.goldCost) + (slot.discountPct > 0 ? "  (-" + slot.discountPct + "%)" : "");
                }

                UIX.AddChildText(row, "Name", name, 28, Color.white, TextAnchor.MiddleLeft, true);
                string cost = slot.diamondCost > 0 ? slot.diamondCost + " Dia" : Economy.Format(slot.goldCost) + " gold";
                UIX.CreateRowButton(row, slot.bought ? "SOLD" : "BUY\n" + cost,
                    () => { if (ShopSystem.TryBuy(slot)) Rebuild(); },
                    slot.bought ? new Color(0.3f, 0.32f, 0.38f) : new Color(0.2f, 0.55f, 0.3f), 84f, 240f, 26)
                    .interactable = !slot.bought;
            }

            // free diamonds timer
            var freeRow = UIX.CreateRow(Content, "FreeDia", 140f);
            var fImg = freeRow.GetComponent<Image>();
            fImg.color = new Color(0.1f, 0.2f, 0.28f, 0.98f);
            string freeLabel;
            bool freeReady = IsFreeDiamondReady(out freeLabel);
            UIX.AddChildText(freeRow, "F", "FREE DIAMONDS\n" + freeLabel, 28,
                freeReady ? new Color(0.4f, 1f, 0.5f) : Color.white, TextAnchor.MiddleCenter, true);
            UIX.CreateRowButton(freeRow, "CLAIM", () =>
                {
                    if (ClaimFreeDiamonds()) Rebuild();
                },
                freeReady ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.3f, 0.32f, 0.38f), 84f, 220f, 28)
                .interactable = freeReady;

            var merchantLink = UIX.CreateRow(Content, "MerchLink", 100f, 20, 20, 10, 10);
            UIX.CreateRowButton(merchantLink, "VISIT THE WANDERING MERCHANT (better deals, 6h rotation)",
                () => Manager.Open("merchant"), new Color(0.4f, 0.55f, 0.25f), 80f, 940f, 28);
        }

        private bool IsFreeDiamondReady(out string label)
        {
            long last = SaveSystem.Current.freeDiamondClaimUtc;
            if (last <= 0)
            {
                label = GameConfig.FreeDiamondAmount + " diamonds waiting!";
                return true;
            }
            var next = new DateTime(last, DateTimeKind.Utc).AddHours(GameConfig.FreeDiamondIntervalHours);
            if (DateTime.UtcNow >= next)
            {
                label = GameConfig.FreeDiamondAmount + " diamonds waiting!";
                return true;
            }
            label = "Next gift in " + UIX.TimeShort(next - DateTime.UtcNow);
            return false;
        }

        private bool ClaimFreeDiamonds()
        {
            string label;
            if (!IsFreeDiamondReady(out label)) return false;
            SaveSystem.Current.freeDiamondClaimUtc = DateTime.UtcNow.Ticks;
            Economy.AddDiamonds(GameConfig.FreeDiamondAmount, "Free gift");
            Audio.AudioSystem.Play("Coin");
            return true;
        }
    }

    // ====================================================================
    // SIGN-IN — 8 day cycle.
    // ====================================================================
    public class SignInScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            var head = UIX.CreateRow(Content, "Head", 110f);
            UIX.AddChildText(head, "T", "SIGN-IN CYCLE " + SignInSystem.Cycle + " — Day " + (SignInSystem.CurrentDayIndex() + 1) + " of 8", 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            for (int day = 0; day < SignInSystem.DaysPerCycle; day += 2)
            {
                var row = UIX.CreateRow(Content, "SignRow" + day, 130f, 10, 10, 6, 6);
                for (int j = 0; j < 2; j++)
                {
                    int d = day + j;
                    bool claimed = SignInSystem.IsClaimed(d);
                    bool today = d == SignInSystem.CurrentDayIndex() && SignInSystem.CanClaimToday();

                    var cell = new GameObject("Day" + d);
                    var rt = cell.AddComponent<RectTransform>();
                    rt.SetParent(row, false);
                    var le = cell.AddComponent<LayoutElement>();
                    le.flexibleWidth = 1f;

                    var img = cell.AddComponent<Image>();
                    img.sprite = UIBuilder.SolidSprite;
                    img.color = claimed ? new Color(0.15f, 0.4f, 0.2f, 0.97f)
                        : today ? new Color(0.75f, 0.55f, 0.1f, 0.97f)
                        : new Color(0.2f, 0.22f, 0.3f, 0.97f);

                    var labelGo = new GameObject("Label");
                    var labelRt = labelGo.AddComponent<RectTransform>();
                    labelRt.SetParent(cell.transform, false);
                    labelRt.anchorMin = Vector2.zero;
                    labelRt.anchorMax = Vector2.one;
                    labelRt.offsetMin = Vector2.zero;
                    labelRt.offsetMax = Vector2.zero;

                    var txt = labelGo.AddComponent<Text>();
                    txt.font = UIBuilder.BuiltinFont;
                    txt.text = SignInSystem.DayTitle(d) + "\n" + (claimed ? "CLAIMED" : today ? "TAP TO CLAIM!" : "locked");
                    txt.fontSize = 24;
                    txt.color = Color.white;
                    txt.alignment = TextAnchor.MiddleCenter;
                    txt.raycastTarget = false;

                    var btn = cell.AddComponent<Button>();
                    btn.targetGraphic = img;
                    btn.interactable = today;
                    btn.onClick.AddListener(() =>
                    {
                        if (SignInSystem.TryClaimToday()) Rebuild();
                    });
                }
            }

            var note = UIX.CreateRow(Content, "Note", 90f);
            UIX.AddChildText(note, "N", "Complete all 8 days for a bonus 200 diamonds and a fresh cycle!", 26,
                new Color(0.7f, 0.8f, 0.9f), TextAnchor.MiddleCenter, true);
        }
    }

    // ====================================================================
    // LIMITED EVENT — points from battles + milestone rewards.
    // ====================================================================
    public class EventsScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            var head = UIX.CreateRow(Content, "Head", 180f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.1f, 0.16f, 0.3f, 0.98f);
            UIX.AddChildText(head, "Name", LimitedEvent.EventName, 48, new Color(0.5f, 0.85f, 1f), TextAnchor.MiddleCenter, true);
            UIX.AddChildText(head, "Timer", "Ends in " + UIX.TimeShort(LimitedEvent.TimeLeft) + "   •   Points: " + LimitedEvent.Points,
                30, Color.white, TextAnchor.MiddleCenter, true);

            var how = UIX.CreateRow(Content, "How", 90f);
            UIX.AddChildText(how, "H", "Win any battle: +50 event points", 28, new Color(0.7f, 0.8f, 0.9f), TextAnchor.MiddleCenter, true);

            for (int i = 0; i < LimitedEvent.Milestones.Length; i++)
            {
                int idx = i;
                int target = LimitedEvent.Milestones[i];
                bool claimable = LimitedEvent.IsMilestoneClaimable(i);

                string reward = i switch
                {
                    0 => "+3,000 gold",
                    1 => "+80 diamonds",
                    2 => "Hero shards x20",
                    3 => "Random equipment",
                    4 => "+150 diamonds",
                    _ => "LEGENDARY HERO"
                };

                var row = UIX.CreateRow(Content, "Ms" + i, 110f);
                UIX.AddChildText(row, "T", target + " points", 30, Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "R", reward, 28, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleLeft, true);
                var bar = UIX.CreateLayoutBar(row, "Bar", claimable ? new Color(0.4f, 0.9f, 0.5f) : new Color(0.3f, 0.5f, 0.9f), 110f, 14f);
                bar.rectTransform.sizeDelta = new Vector2(110f * Mathf.Clamp01((float)LimitedEvent.Points / target), 0f);
                UIX.CreateRowButton(row, claimable ? "CLAIM" : (LimitedEvent.Points >= target ? "DONE" : "LOCKED"),
                    () => { if (LimitedEvent.TryClaimMilestone(idx)) Rebuild(); },
                    claimable ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.28f, 0.3f, 0.4f), 72f, 190f, 26)
                    .interactable = claimable;
            }
        }
    }

    // ====================================================================
    // OFFERS — value packs purchasable with in-game currency.
    // ====================================================================
    public class OffersScreen : ScreenBase
    {
        private string _starterDay = "";
        private bool _starterBoughtToday;

        public override void Rebuild()
        {
            ClearContent();

            var head = UIX.CreateRow(Content, "Head", 110f);
            UIX.AddChildText(head, "T", "SPECIAL OFFERS — daily value packs", 30, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            // starter pack
            CheckStarterDay();
            var starter = UIX.CreateRow(Content, "Starter", 230f);
            var sImg = starter.GetComponent<Image>();
            sImg.color = new Color(0.28f, 0.14f, 0.05f, 0.98f);
            UIX.AddChildText(starter, "T", "DAILY STARTER PACK", 36, new Color(1f, 0.8f, 0.3f), TextAnchor.MiddleCenter, true);
            UIX.AddChildText(starter, "D", "8,000 gold + random equipment + 15 hero shards\nCost: 120 diamonds  •  once per day", 28,
                Color.white, TextAnchor.MiddleCenter, true);
            UIX.CreateRowButton(starter, _starterBoughtToday ? "COME BACK TOMORROW" : "BUY (120 Dia)",
                () =>
                {
                    if (_starterBoughtToday) return;
                    if (!Economy.TrySpendDiamonds(120)) return;
                    Economy.AddGold(8000, "Starter pack");
                    Items.InventorySystem.Grant(Items.ItemDatabase.RollRandomItemId());
                    HeroProgress.AddShards(SaveSystem.Current.unlockedHeroIds[0], 15);
                    _starterBoughtToday = true;
                    Audio.AudioSystem.Play("Coin");
                    Rebuild();
                },
                _starterBoughtToday ? new Color(0.3f, 0.32f, 0.38f) : new Color(0.7f, 0.45f, 0.1f), 84f, 420f, 28)
                .interactable = !_starterBoughtToday;

            // shard packs
            var roster = LegendsRPG.Characters.HeroDatabase.CreateDefaultRoster();
            roster.Sort((a, b) => b.rarity.CompareTo(a.rarity));

            for (int i = 0; i < 4; i++)
            {
                var hero = roster[i * 7];
                int price = 40 + (int)hero.rarity * 25;
                var row = UIX.CreateRow(Content, "Shard" + i, 110f);
                UIX.AddChildText(row, "H", hero.displayName + "\n" + hero.rarity + " " + hero.heroClass, 28, Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "G", "Shards x30", 28, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleLeft, true, 220f);
                UIX.CreateRowButton(row, "BUY\n" + price + " Dia",
                    () =>
                    {
                        if (Economy.TrySpendDiamonds(price))
                        {
                            HeroProgress.AddShards(hero.heroId, 30);
                            Audio.AudioSystem.Play("Coin");
                            Rebuild();
                        }
                    },
                    new Color(0.45f, 0.3f, 0.6f), 80f, 220f, 26);
            }
        }

        private void CheckStarterDay()
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            if (_starterDay != today)
            {
                _starterDay = today;
                _starterBoughtToday = false;
            }
        }
    }

    // ====================================================================
    // MERCHANT — 6h rotating deep-discount offers.
    // ====================================================================
    public class MerchantScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();
            MerchantSystem.EnsureFresh();

            var head = UIX.CreateRow(Content, "Head", 110f);
            UIX.AddChildText(head, "T", "WANDERING MERCHANT — leaves in " + UIX.TimeShort(MerchantSystem.TimeToRefresh()), 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            foreach (var o in MerchantSystem.Offers)
            {
                var row = UIX.CreateRow(Content, "Offer_" + o.label, 110f);
                UIX.AddChildText(row, "Name", o.label + "\n" + o.detail, 28, Color.white, TextAnchor.MiddleLeft, true);
                UIX.CreateRowButton(row, o.bought ? "SOLD" : "BUY\n" + o.diamondCost + " Dia",
                    () => { if (MerchantSystem.TryBuy(o)) Rebuild(); },
                    o.bought ? new Color(0.3f, 0.32f, 0.38f) : new Color(0.35f, 0.5f, 0.2f), 80f, 230f, 26)
                    .interactable = !o.bought;
            }
        }
    }

    // ====================================================================
    // CHAT — world & guild channels.
    // ====================================================================
    public class ChatScreen : ScreenBase
    {
        private int _channel;   // 0 world 1 guild
        private InputField _input;
        private RectTransform _listContent;
        private float _refreshTimer;
        private int _lastMsgCount;

        public override void Build(UIManager manager, Transform canvasParent, string title)
        {
            base.Build(manager, canvasParent, title);

            // tabs + input live outside the scroll
            var tabRow = UIBuilder.CreateFreeRect(Root, "Tabs", new Vector2(500f, 100f),
                new Vector2(0f, 1f), new Vector2(40f, -190f));
            UIBuilder.CreateButton(tabRow, "WorldTab", "WORLD", new Vector2(240f, 90f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Color(0.25f, 0.4f, 0.6f, 0.95f), Color.white, 30,
                () => { _channel = 0; RefreshList(true); });
            UIBuilder.CreateButton(tabRow, "GuildTab", "GUILD", new Vector2(240f, 90f),
                new Vector2(0f, 0f), new Vector2(255f, 0f), new Color(0.4f, 0.6f, 0.3f, 0.95f), Color.white, 30,
                () => { _channel = 1; RefreshList(true); });

            var inputRow = UIBuilder.CreateFreeRect(Root, "InputRow", new Vector2(1000f, 110f),
                new Vector2(0.5f, 0f), new Vector2(0f, 130f));

            var inputGo = new GameObject("Input");
            var irt = inputGo.AddComponent<RectTransform>();
            irt.SetParent(inputRow, false);
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = new Vector2(0f, 1f);
            irt.offsetMin = new Vector2(0f, 0f);
            irt.offsetMax = new Vector2(-270f, 0f);

            var inputBg = inputGo.AddComponent<Image>();
            inputBg.sprite = UIBuilder.SolidSprite;
            inputBg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f);

            _input = inputGo.AddComponent<InputField>();
            _input.placeholder = CreateInputText(inputGo, "Placeholder", "Say something...", new Color(0.5f, 0.5f, 0.55f));
            _input.textComponent = CreateInputText(inputGo, "Text", "", Color.white);
            _input.text = "";

            UIBuilder.CreateButton(inputRow, "SendBtn", "SEND", new Vector2(240f, 100f),
                new Vector2(1f, 0f), new Vector2(-255f, 0f), new Color(0.2f, 0.5f, 0.35f, 0.95f), Color.white, 30, SendCurrent);

            // message list occupies the middle
            _listContent = UIX.CreateScrollView(Root, "MsgScroll",
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(30f, 260f), new Vector2(-30f, -310f));
        }

        private static Text CreateInputText(GameObject parent, string name, string value, Color color)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(16f, 8f);
            rt.offsetMax = new Vector2(-16f, -8f);
            var t = go.AddComponent<Text>();
            t.font = UIBuilder.BuiltinFont;
            t.text = value;
            t.fontSize = 30;
            t.color = color;
            t.alignment = TextAnchor.MiddleLeft;
            t.raycastTarget = false;
            return t;
        }

        public override void Open()
        {
            base.Open();
            RefreshList(true);
        }

        public override void Tick()
        {
            _refreshTimer -= Time.unscaledDeltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = 1.5f;
                var channel = _channel == 0 ? ChatSystem.World : ChatSystem.Guild;
                if (channel.Count != _lastMsgCount) RefreshList(false);
            }
        }

        private void SendCurrent()
        {
            if (_input == null || string.IsNullOrWhiteSpace(_input.text)) return;
            if (_channel == 0) ChatSystem.SendWorld(_input.text);
            else ChatSystem.SendGuild(_input.text);
            _input.text = "";
            _input.DeactivateInputField();
            RefreshList(true);
        }

        private void RefreshList(bool clear)
        {
            if (_listContent == null) return;
            for (int i = _listContent.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(_listContent.GetChild(i).gameObject);
            }

            var channel = _channel == 0 ? ChatSystem.World : ChatSystem.Guild;
            _lastMsgCount = channel.Count;

            foreach (var msg in channel)
            {
                var row = UIX.CreateRow(_listContent, "Msg", 90f, 14, 14, 6, 6);
                Color c = msg.isPlayer ? new Color(0.55f, 0.85f, 1f) : new Color(0.9f, 0.9f, 0.95f);
                UIX.AddChildText(row, "Author", msg.author, 26, msg.color, TextAnchor.MiddleLeft, true, 200f);
                UIX.AddChildText(row, "Text", msg.text, 26, c, TextAnchor.MiddleLeft, true);
            }
        }

        public override void Rebuild()
        {
            // content is managed by RefreshList
            ChatSystem.SeedWelcome();
        }
    }

    // ====================================================================
    // TECH — 6 account-wide upgrade branches.
    // ====================================================================
    public class TechScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            var head = UIX.CreateRow(Content, "Head", 90f);
            UIX.AddChildText(head, "T", "TECHNOLOGIES — permanent account upgrades", 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            for (int i = 0; i < TechSystem.BranchNames.Length; i++)
            {
                int branch = i;
                int lv = TechSystem.LevelOf(branch);
                long cost = TechSystem.CostOf(branch, lv);
                bool maxed = lv >= TechSystem.MaxLevel;

                var row = UIX.CreateRow(Content, "Tech" + i, 120f);
                UIX.AddChildText(row, "Name", TechSystem.BranchNames[i] + "\n" + TechSystem.BranchDesc[i], 28,
                    Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "Lv", "Lv " + lv + " / " + TechSystem.MaxLevel, 28,
                    new Color(0.6f, 0.85f, 1f), TextAnchor.MiddleRight, true, 190f);
                UIX.CreateRowButton(row, maxed ? "MAX" : "UPGRADE\n" + Economy.Format(cost) + " gold",
                    () => { if (TechSystem.TryUpgrade(branch)) Rebuild(); },
                    maxed ? new Color(0.3f, 0.32f, 0.38f) : new Color(0.5f, 0.35f, 0.65f), 84f, 250f, 26)
                    .interactable = !maxed;
            }
        }
    }

    // ====================================================================
    // FORGE — upgrade owned equipment.
    // ====================================================================
    public class ForgeScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            var head = UIX.CreateRow(Content, "Head", 90f);
            UIX.AddChildText(head, "T", "FORGE — +12% item stats per level", 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            var equipped = SaveSystem.Current.equipped;
            var equippedIds = new HashSet<string>();
            foreach (var eq in equipped) if (eq != null && !string.IsNullOrEmpty(eq.itemId)) equippedIds.Add(eq.itemId);

            var owned = new List<string>();
            foreach (var id in SaveSystem.Current.ownedItemIds) owned.Add(id);
            owned.Sort((a, b) =>
            {
                int ea = equippedIds.Contains(a) ? 1 : 0;
                int eb = equippedIds.Contains(b) ? 1 : 0;
                if (ea != eb) return eb - ea;
                return string.Compare(a, b, StringComparison.Ordinal);
            });

            if (owned.Count == 0)
            {
                var empty = UIX.CreateRow(Content, "Empty", 110f);
                UIX.AddChildText(empty, "E", "No equipment yet — win battles and open chests!", 28,
                    new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);
                return;
            }

            foreach (var id in owned)
            {
                var item = Items.ItemDatabase.Get(id);
                if (item == null) continue;

                int lv = ForgeSystem.LevelOf(id);
                long cost = ForgeSystem.CostOf(id, lv + 1);
                bool maxed = lv >= ForgeSystem.MaxForgeLevel;

                var row = UIX.CreateRow(Content, "F_" + id, 120f);
                var dot = new GameObject("Dot");
                var drt = dot.AddComponent<RectTransform>();
                drt.SetParent(row, false);
                var dle = dot.AddComponent<LayoutElement>();
                dle.preferredWidth = 40f;
                var dimg = dot.AddComponent<Image>();
                dimg.sprite = UIBuilder.SolidSprite;
                dimg.color = SummonScreen.RarityColor(item.rarity);

                UIX.AddChildText(row, "Name", item.displayName + (equippedIds.Contains(id) ? "\n[equipped]" : "") +
                    "\n" + item.StatSummary(), 26, Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "Lv", "+" + lv, 34, new Color(1f, 0.8f, 0.3f), TextAnchor.MiddleRight, true, 130f);
                UIX.CreateRowButton(row, maxed ? "MAX" : "UPGRADE\n" + Economy.Format(cost) + " gold",
                    () => { if (ForgeSystem.TryUpgrade(id)) Rebuild(); },
                    maxed ? new Color(0.3f, 0.32f, 0.38f) : new Color(0.65f, 0.35f, 0.15f), 84f, 250f, 26)
                    .interactable = !maxed;
            }
        }
    }

    // ====================================================================
    // INVENTORY — equip / sell items.
    // ====================================================================
    public class InventoryScreen : ScreenBase
    {
        private int _equipTargetSlot = 0;
        private string _equipTargetHero = "";

        public override void Rebuild()
        {
            ClearContent();

            // hero + slot selector
            var team = Team.TeamManager.InstanceRef;
            var head = UIX.CreateRow(Content, "Head", 120f);
            string heroLabel = string.IsNullOrEmpty(_equipTargetHero)
                ? "Tap EQUIP on an item, then pick the hero here"
                : "Equipping to: " + _equipTargetHero + " / slot " + _equipTargetSlot;
            UIX.AddChildText(head, "H", heroLabel, 28, new Color(0.7f, 0.9f, 1f), TextAnchor.MiddleLeft, true);

            if (!string.IsNullOrEmpty(_equipTargetHero) && team != null)
            {
                var slotsRow = UIX.CreateRow(Content, "SlotsRow", 100f, 8, 8, 8, 8);
                string[] slotNames = { "WPN", "ARM", "AML", "WNG", "PET", "MNT" };
                for (int s = 0; s < Items.InventorySystem.SlotCount; s++)
                {
                    int slot = s;
                    UIX.CreateRowButton(slotsRow, slotNames[s],
                        () =>
                        {
                            // re-equip pending item into this slot
                            string itemId = _pendingItem;
                            if (!string.IsNullOrEmpty(itemId))
                            {
                                Items.InventorySystem.Equip(_equipTargetHero, slot, itemId);
                                _pendingItem = null;
                                _equipTargetHero = "";
                                Audio.AudioSystem.Play("Equip");
                                Manager.Notify("Equipped!");
                            }
                            Rebuild();
                        },
                        _equipTargetSlot == s ? new Color(0.7f, 0.45f, 0.1f) : new Color(0.25f, 0.28f, 0.38f),
                        76f, 140f, 24);
                }
            }

            // owned items
            var owned = new List<string>(SaveSystem.Current.ownedItemIds);
            if (owned.Count == 0)
            {
                var empty = UIX.CreateRow(Content, "Empty", 110f);
                UIX.AddChildText(empty, "E", "Bag is empty — win battles and claim loot!", 28,
                    new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);
                return;
            }

            foreach (var id in owned)
            {
                var item = Items.ItemDatabase.Get(id);
                if (item == null) continue;

                var row = UIX.CreateRow(Content, "I_" + id, 140f);
                var dot = new GameObject("Dot");
                var drt = dot.AddComponent<RectTransform>();
                drt.SetParent(row, false);
                var dle = dot.AddComponent<LayoutElement>();
                dle.preferredWidth = 44f;
                var dimg = dot.AddComponent<Image>();
                dimg.sprite = UIBuilder.SolidSprite;
                dimg.color = SummonScreen.RarityColor(item.rarity);

                UIX.AddChildText(row, "Info", item.displayName + "  (" + item.category + ")\n" +
                    item.StatSummary() + "  •  " + item.RarityName + (ForgeSystem.LevelOf(id) > 0 ? "  +Forge " + ForgeSystem.LevelOf(id) : ""),
                    26, Color.white, TextAnchor.MiddleLeft, true);

                var btnCol = new GameObject("Btns");
                var brt = btnCol.AddComponent<RectTransform>();
                brt.SetParent(row, false);
                var bl = btnCol.AddComponent<HorizontalLayoutGroup>();
                bl.childAlignment = TextAnchor.MiddleCenter;
                bl.childForceExpandWidth = false;
                bl.childControlWidth = false;
                bl.childForceExpandHeight = false;
                bl.childControlHeight = false;
                bl.spacing = 8f;
                var ble = btnCol.AddComponent<LayoutElement>();
                ble.minWidth = 278f;
                ble.preferredWidth = 278f;
                ble.flexibleWidth = 0f;

                UIX.CreateRowButton(btnCol.transform, "EQUIP", () =>
                    {
                        _pendingItem = id;
                        if (string.IsNullOrEmpty(_equipTargetHero) && team != null && team.Heroes.Count > 0)
                        {
                            _equipTargetHero = team.Heroes[0].Data.heroId;
                        }
                        Manager.Notify("Pick slot: WPN/ARM/AML/WNG/PET/MNT");
                        Rebuild();
                    },
                    new Color(0.2f, 0.5f, 0.35f), 76f, 132f, 21);
                UIX.CreateRowButton(btnCol.transform, "SELL +" + Economy.Format(200 + item.RarityIndex * 350) + "g", () =>
                    {
                        SellItem(id);
                        Rebuild();
                    },
                    new Color(0.45f, 0.3f, 0.2f), 76f, 138f, 19);
            }
        }

        private string _pendingItem;

        private void SellItem(string itemId)
        {
            var item = Items.ItemDatabase.Get(itemId);
            if (item == null) return;

            // cannot sell an equipped item
            foreach (var eq in SaveSystem.Current.equipped)
            {
                if (eq != null && eq.itemId == itemId)
                {
                    Manager.Notify("Unequip it first!");
                    Audio.AudioSystem.Play("Error");
                    return;
                }
            }

            Economy.AddGold(200 + item.RarityIndex * 350, "Sold " + item.displayName);
            var owned = SaveSystem.Current.ownedItemIds;
            owned.Remove(itemId);
            Audio.AudioSystem.Play("Coin");
        }
    }

    // ====================================================================
    // MINE — idle gold production.
    // ====================================================================
    public class MineScreen : ScreenBase
    {
        public override void Tick()
        {
            if (Content == null || !IsOpen) return;
            // live update of the pending amount every second
            _tickTimer -= Time.unscaledDeltaTime;
            if (_tickTimer <= 0f)
            {
                _tickTimer = 1f;
                Rebuild();
            }
        }
        private float _tickTimer = 1f;

        public override void Rebuild()
        {
            ClearContent();

            var pending = MineSystem.Pending();

            var head = UIX.CreateRow(Content, "Head", 170f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.2f, 0.16f, 0.08f, 0.98f);
            UIX.AddChildText(head, "T", "GOLD MINE  —  Level " + MineSystem.Level + " / " + MineSystem.MaxLevel + "\n" +
                "Production: " + Economy.Format(MineSystem.PerHour) + " gold / hour", 32,
                new Color(1f, 0.85f, 0.35f), TextAnchor.MiddleCenter, true);

            var bar = UIX.CreateRow(Content, "Bar", 100f);
            UIX.AddChildText(bar, "P", "Stored: " + Economy.Format(pending.gold) + " / " + Economy.Format(MineSystem.Capacity), 28,
                Color.white, TextAnchor.MiddleLeft, true);
            var fill = UIX.CreateLayoutBar(bar, "Fill", new Color(1f, 0.8f, 0.3f), 200f, 20f);
            fill.rectTransform.sizeDelta = new Vector2(200f * pending.capRatio, 0f);

            var act = UIX.CreateRow(Content, "Act", 130f, 20, 20, 12, 12);
            UIX.CreateRowButton(act, "COLLECT\n" + Economy.Format(pending.gold) + " gold",
                () => { MineSystem.Claim(); Rebuild(); },
                new Color(0.7f, 0.55f, 0.12f), 100f, 440f, 28);
            UIX.CreateRowButton(act, MineSystem.Level >= MineSystem.MaxLevel ? "MAX LEVEL" :
                    "UPGRADE\n" + Economy.Format(MineSystem.UpgradeCost) + " gold",
                () => { if (MineSystem.TryUpgrade()) Rebuild(); },
                new Color(0.3f, 0.45f, 0.6f), 100f, 440f, 28)
                .interactable = MineSystem.Level < MineSystem.MaxLevel;

            var note = UIX.CreateRow(Content, "Note", 90f);
            UIX.AddChildText(note, "N", "The mine keeps producing while you are offline (8h capacity).", 26,
                new Color(0.65f, 0.65f, 0.75f), TextAnchor.MiddleCenter, true);
        }
    }

    // ====================================================================
    // BEAST DEN — pets with team-wide passives.
    // ====================================================================
    public class BeastScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            var bonus = BeastSystem.ActiveBonus();
            var head = UIX.CreateRow(Content, "Head", 120f);
            UIX.AddChildText(head, "T", "BEASTS — team bonus: +" + (bonus.atkPct * 100f).ToString("0.0") + "% ATK   +" +
                (bonus.hpPct * 100f).ToString("0.0") + "% HP\nSnacks: " + BeastSystem.Snacks, 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            foreach (var b in BeastSystem.Bestiary)
            {
                bool unlocked = BeastSystem.IsUnlocked(b.id);
                int lv = BeastSystem.LevelOf(b.id);

                var row = UIX.CreateRow(Content, "B_" + b.id, 130f);
                var dot = new GameObject("Dot");
                var drt = dot.AddComponent<RectTransform>();
                drt.SetParent(row, false);
                var dle = dot.AddComponent<LayoutElement>();
                dle.preferredWidth = 44f;
                var dimg = dot.AddComponent<Image>();
                dimg.sprite = UIBuilder.SolidSprite;
                dimg.color = unlocked ? ElementalColor(b.element) : new Color(0.2f, 0.2f, 0.24f);

                UIX.AddChildText(row, "Name", (unlocked ? b.name : "???") + "  Lv." + lv + "\n" + b.desc, 28,
                    unlocked ? Color.white : new Color(0.5f, 0.5f, 0.55f), TextAnchor.MiddleLeft, true);

                if (unlocked)
                {
                    UIX.CreateRowButton(row, "FEED\n" + Economy.Format(BeastSystem.FeedCost(b.id)) + "g +1 snack",
                        () => { if (BeastSystem.TryFeed(b.id)) Rebuild(); },
                        new Color(0.3f, 0.55f, 0.25f), 84f, 260f, 24);
                }
                else
                {
                    UIX.CreateRowButton(row, "UNLOCK\n" + b.unlockDiamonds + " Dia",
                        () => { if (BeastSystem.TryUnlock(b)) Rebuild(); },
                        new Color(0.5f, 0.35f, 0.6f), 84f, 260f, 24);
                }
            }

            var note = UIX.CreateRow(Content, "Note", 90f);
            UIX.AddChildText(note, "N", "Snacks come from guild gifts, sign-in day 6 and events.", 26,
                new Color(0.65f, 0.65f, 0.75f), TextAnchor.MiddleCenter, true);
        }

        private static Color ElementalColor(Combat.ElementType el)
        {
            return Combat.ElementalMatrix.GetElementColor(el);
        }
    }

    // ====================================================================
    // PROFILE — identity, titles, statistics.
    // ====================================================================
    public class ProfileScreen : ScreenBase
    {
        private InputField _renameInput;

        public override void Build(UIManager manager, Transform canvasParent, string title)
        {
            // Rename controls are created in Rebuild as a normal content row so they
            // participate in the same vertical scroll/layout as the rest of Profile.
            base.Build(manager, canvasParent, title);
        }

        private static Text CreateText(GameObject parent, string name, string value, Color color)
        {
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(16f, 8f);
            rt.offsetMax = new Vector2(-16f, -8f);
            var t = go.AddComponent<Text>();
            t.font = UIBuilder.BuiltinFont;
            t.text = value;
            t.fontSize = 30;
            t.color = color;
            t.alignment = TextAnchor.MiddleLeft;
            t.raycastTarget = false;
            return t;
        }

        private void DoRename()
        {
            if (_renameInput == null || string.IsNullOrWhiteSpace(_renameInput.text)) return;
            if (!Economy.TrySpendDiamonds(500)) return;
            if (PlayerProfile.Rename(_renameInput.text))
            {
                Manager.Notify("Renamed to " + PlayerProfile.Name);
                _renameInput.text = "";
                Rebuild();
            }
            else
            {
                Economy.AddDiamonds(500, "Rename refund");
                Manager.Notify("Name must be 2-14 characters");
            }
        }

        public override void Rebuild()
        {
            ClearContent();

            var head = UIX.CreateRow(Content, "Head", 200f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.14f, 0.18f, 0.3f, 0.98f);
            UIX.AddChildText(head, "N", PlayerProfile.Name + "   Lv." + PlayerProfile.Level, 40, Color.white, TextAnchor.MiddleLeft, true);
            UIX.AddChildText(head, "I",
                "ID: " + PlayerProfile.PlayerId + "\nVIP " + Economy.VipLevel + "  •  " +
                (string.IsNullOrEmpty(PlayerProfile.ActiveTitle) ? "No title" : "\"" + PlayerProfile.ActiveTitle + "\""),
                28, new Color(0.8f, 0.85f, 0.95f), TextAnchor.MiddleRight, true);

            var renameRow = UIX.CreateRow(Content, "RenameRow", 112f, 12, 12, 8, 8);
            var inputGo = new GameObject("RenameInput", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var irt = inputGo.GetComponent<RectTransform>();
            irt.SetParent(renameRow, false);
            var inputLayout = inputGo.GetComponent<LayoutElement>();
            inputLayout.minWidth = 320f;
            inputLayout.preferredHeight = 76f;
            inputLayout.flexibleWidth = 1f;
            var inputBg = inputGo.GetComponent<Image>();
            inputBg.sprite = UIBuilder.SolidSprite;
            inputBg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f);
            _renameInput = inputGo.AddComponent<InputField>();
            _renameInput.placeholder = CreateText(inputGo, "Ph", "New name (2-14 chars)", new Color(0.5f, 0.5f, 0.55f));
            _renameInput.textComponent = CreateText(inputGo, "Txt", "", Color.white);
            UIX.CreateRowButton(renameRow, "RENAME (500 Dia)", DoRename,
                new Color(0.35f, 0.4f, 0.6f), 76f, 300f, 22);

            // statistics
            AddStatRow("Player EXP", Mathf.RoundToInt(PlayerProfile.Exp) + " / " + Mathf.RoundToInt(PlayerProfile.ExpNeeded(PlayerProfile.Level)));
            AddStatRow("Total power", Economy.Format(HeroProgress.TeamPower(TeamHeroes())));
            AddStatRow("Heroes owned", SaveSystem.Current.unlockedHeroIds.Count + " / 62");
            AddStatRow("Campaign stars", CampaignSystem.TotalStars() + " / " + (CampaignSystem.Chapters * CampaignSystem.StagesPerChapter * 3));
            AddStatRow("Total summons", SummonSystem.TotalPulls.ToString());
            AddStatRow("Arena record", ArenaSystem.Wins + "W / " + ArenaSystem.Losses + "L  (Rank " + ArenaSystem.Rank + ")");
            AddStatRow("Guild", GuildSystem.InGuild ? GuildSystem.GuildName + " (Lv." + GuildSystem.GuildLevel + ")" : "No guild");
            AddStatRow("Lifetime gold", Economy.Format(Economy.LifetimeGoldEarned));

            // titles
            var titleHeader = UIX.CreateRow(Content, "TitleHeader", 64f);
            UIX.AddChildText(titleHeader, "T", "TITLES (tap to wear)", 28, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            foreach (var title in PlayerProfile.Titles)
            {
                string t = title;
                var row = UIX.CreateRow(Content, "Title_" + t, 80f);
                bool active = PlayerProfile.ActiveTitle == t;
                UIX.AddChildText(row, "T", "\"" + t + "\"", 28, active ? new Color(1f, 0.85f, 0.3f) : Color.white, TextAnchor.MiddleLeft, true);
                UIX.CreateRowButton(row, active ? "WEARING" : "WEAR",
                    () => { PlayerProfile.SetTitle(t); Rebuild(); },
                    active ? new Color(0.5f, 0.45f, 0.15f) : new Color(0.3f, 0.35f, 0.5f), 60f, 190f, 24)
                    .interactable = !active;
            }
        }

        private void AddStatRow(string label, string value)
        {
            var row = UIX.CreateRow(Content, "S_" + label, 76f);
            UIX.AddChildText(row, "L", label, 28, new Color(0.75f, 0.78f, 0.88f), TextAnchor.MiddleLeft, true);
            UIX.AddChildText(row, "V", value, 28, Color.white, TextAnchor.MiddleRight, true);
        }

        private static List<LegendsRPG.Characters.HeroData> TeamHeroes()
        {
            var result = new List<LegendsRPG.Characters.HeroData>();
            var team = Team.TeamManager.InstanceRef;
            if (team == null) return result;
            var roster = LegendsRPG.Characters.HeroDatabase.CreateDefaultRoster();
            foreach (var h in team.Heroes)
            {
                if (h == null) continue;
                foreach (var r in roster)
                {
                    if (r.heroId == h.Data.heroId) { result.Add(r); break; }
                }
            }
            return result;
        }
    }

    // ====================================================================
    // SETTINGS — graphics tiers, volumes, save info, reset.
    // ====================================================================
    public class SettingsScreen : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            var gfxHeader = UIX.CreateRow(Content, "GfxHeader", 64f);
            UIX.AddChildText(gfxHeader, "T", "GRAPHICS QUALITY", 28, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            var gfxRow = UIX.CreateRow(Content, "GfxRow", 110f, 10, 10, 8, 8);
            for (int i = 0; i < 4; i++)
            {
                int lv = i;
                UIX.CreateRowButton(gfxRow, GfxSettings.Names[i],
                    () => { GfxSettings.Apply(lv); SaveSystem.SaveNow(); Rebuild(); },
                    GfxSettings.Level == lv ? new Color(0.7f, 0.45f, 0.1f) : new Color(0.25f, 0.28f, 0.38f),
                    90f, 215f, 30);
            }

            var gfxNote = UIX.CreateRow(Content, "GfxNote", 110f);
            UIX.AddChildText(gfxNote, "N",
                "Low: no shadows, fastest battery  •  Medium: hard shadows\n" +
                "High: soft shadows  •  Ultra: long shadows + extra lights",
                26, new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);

            // volumes
            AddVolumeRow("Music", Audio.AudioSystem.MusicVolume(), v =>
            {
                Audio.AudioSystem.SetMusicVolume(v);
                SaveSystem.Current.musicVolume = v;
            });
            AddVolumeRow("SFX", Audio.AudioSystem.SfxVolume(), v =>
            {
                Audio.AudioSystem.SetSfxVolume(v);
                SaveSystem.Current.sfxVolume = v;
            });

            // account
            AddStatRow("Player ID", PlayerProfile.PlayerId);
            AddStatRow("Save file", "legends_save_v1.json");
            AddStatRow("Schema", "v3 (cloud-sync ready)");

            var reset = UIX.CreateRow(Content, "Reset", 120f, 20, 20, 12, 12);
            UIX.CreateRowButton(reset, "RESET SAVE (delete all progress)",
                () =>
                {
                    string path = System.IO.Path.Combine(Application.persistentDataPath, "legends_save_v1.json");
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    Manager.Notify("Save deleted — restart the game to start fresh");
                },
                new Color(0.6f, 0.2f, 0.2f), 84f, 900f, 30);
        }

        private void AddVolumeRow(string label, float value, Action<float> onChange)
        {
            var row = UIX.CreateRow(Content, "Vol_" + label, 90f);
            UIX.AddChildText(row, "L", label, 28, Color.white, TextAnchor.MiddleLeft, true, 200f);

            var sliderGo = new GameObject("Slider_" + label);
            var srt = sliderGo.AddComponent<RectTransform>();
            srt.SetParent(row, false);
            var sle = sliderGo.AddComponent<LayoutElement>();
            sle.flexibleWidth = 1f;
            sle.preferredHeight = 40f;

            var sliderBg = sliderGo.AddComponent<Image>();
            sliderBg.sprite = UIBuilder.SolidSprite;
            sliderBg.color = new Color(0f, 0f, 0f, 0.6f);

            var slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;

            var fillArea = new GameObject("FillArea");
            var frt = fillArea.AddComponent<RectTransform>();
            frt.SetParent(srt, false);
            frt.anchorMin = new Vector2(0f, 0.25f);
            frt.anchorMax = new Vector2(1f, 0.75f);
            frt.offsetMin = new Vector2(8f, 0f);
            frt.offsetMax = new Vector2(-8f, 0f);

            var fillGo = new GameObject("Fill");
            var filrt = fillGo.AddComponent<RectTransform>();
            filrt.SetParent(frt, false);
            filrt.anchorMin = Vector2.zero;
            filrt.anchorMax = new Vector2(value, 1f);
            filrt.offsetMin = Vector2.zero;
            filrt.offsetMax = Vector2.zero;

            var fillImg = fillGo.AddComponent<Image>();
            fillImg.sprite = UIBuilder.SolidSprite;
            fillImg.color = new Color(0.3f, 0.7f, 0.4f);

            slider.fillRect = filrt;
            slider.targetGraphic = fillImg;
            slider.direction = Slider.Direction.LeftToRight;

            slider.onValueChanged.AddListener(v => onChange(v));
        }

        private void AddStatRow(string label, string value)
        {
            var row = UIX.CreateRow(Content, "S_" + label, 76f);
            UIX.AddChildText(row, "L", label, 26, new Color(0.75f, 0.78f, 0.88f), TextAnchor.MiddleLeft, true);
            UIX.AddChildText(row, "V", value, 26, Color.white, TextAnchor.MiddleRight, true);
        }
    }

    // ====================================================================
    // LEADERBOARD
    // ====================================================================
    public class RankScreen : ScreenBase
    {
        public override void Open()
        {
            RankSystem.Rebuild();
            base.Open();
        }

        public override void Rebuild()
        {
            ClearContent();

            var head = UIX.CreateRow(Content, "Head", 90f);
            UIX.AddChildText(head, "T", "LEADERBOARD — by team power", 30, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            int rank = 1;
            foreach (var e in RankSystem.Board)
            {
                var row = UIX.CreateRow(Content, "Rank" + rank, 84f);
                var img = row.GetComponent<Image>();
                img.color = e.isPlayer ? new Color(0.15f, 0.35f, 0.45f, 0.98f) : new Color(0.13f, 0.14f, 0.19f, 0.96f);

                string medal = rank == 1 ? "1st" : rank == 2 ? "2nd" : rank == 3 ? "3rd" : rank + "th";
                UIX.AddChildText(row, "R", medal, 28, rank <= 3 ? new Color(1f, 0.85f, 0.3f) : new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleLeft, true, 120f);
                UIX.AddChildText(row, "N", e.name + (e.isPlayer ? " (YOU)" : ""), 28, e.isPlayer ? new Color(0.5f, 0.9f, 1f) : Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "P", Economy.Format(e.power), 28, new Color(1f, 0.8f, 0.4f), TextAnchor.MiddleRight, true, 220f);
                UIX.AddChildText(row, "S", "ch." + e.stage, 24, new Color(0.6f, 0.7f, 0.85f), TextAnchor.MiddleRight, true, 140f);
                rank++;
            }
        }
    }

    // ====================================================================
    // AFK POPUP — claim the idle treasure.
    // ====================================================================
    public class AFKPopup : ScreenBase
    {
        public override void Rebuild()
        {
            ClearContent();

            var pending = AFKReward.Pending();

            var head = UIX.CreateRow(Content, "Head", 200f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.22f, 0.17f, 0.06f, 0.98f);
            UIX.AddChildText(head, "T", "TREASURE CHEST\nAFK rewards accumulate up to 12 hours", 34,
                new Color(1f, 0.85f, 0.35f), TextAnchor.MiddleCenter, true);

            var cap = UIX.CreateRow(Content, "Cap", 100f);
            UIX.AddChildText(cap, "C", "Chest fill: " + Mathf.RoundToInt(AFKReward.CapRatio() * 100f) + "%", 28,
                Color.white, TextAnchor.MiddleCenter, true);
            var fill = UIX.CreateLayoutBar(cap, "Fill", new Color(1f, 0.8f, 0.3f), 300f, 22f);
            fill.rectTransform.sizeDelta = new Vector2(300f * AFKReward.CapRatio(), 0f);

            var rewards = UIX.CreateRow(Content, "Rewards", 150f);
            UIX.AddChildText(rewards, "R",
                "+" + Economy.Format(pending.gold) + " Gold\n+" + Economy.Format(pending.exp) + " Player XP\n+" + pending.diamonds + " Diamonds",
                30, Color.white, TextAnchor.MiddleCenter, true);

            var rates = UIX.CreateRow(Content, "Rates", 90f);
            UIX.AddChildText(rates, "RT", "Rate: " + Economy.Format(AFKReward.GoldPerHour) + " gold/hour (grows with campaign progress)", 26,
                new Color(0.7f, 0.8f, 0.9f), TextAnchor.MiddleCenter, true);

            var act = UIX.CreateRow(Content, "Act", 130f, 20, 20, 12, 12);
            UIX.CreateRowButton(act, pending.gold <= 0 ? "CHEST EMPTY" : "CLAIM ALL",
                () =>
                {
                    var got = AFKReward.Claim();
                    if (got.gold > 0)
                    {
                        Manager.CloseScreen();
                    }
                    else
                    {
                        Manager.Notify("Come back later — the chest is filling!");
                    }
                },
                pending.gold <= 0 ? new Color(0.3f, 0.32f, 0.38f) : new Color(0.75f, 0.55f, 0.1f), 100f, 700f, 36)
                .interactable = pending.gold > 0;
        }
    }
}
