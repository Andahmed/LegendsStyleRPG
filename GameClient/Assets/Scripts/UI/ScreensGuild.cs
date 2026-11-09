using System;
using UnityEngine;
using UnityEngine.UI;
using LegendsRPG.Core;
using LegendsRPG.Social;
using LegendsRPG.World;

namespace LegendsRPG.UI
{
    // ====================================================================
    // GUILD — join/create flow, then 7 tabs: Info, Hunt, Caravan, War,
    // Tech, Gifts, Wish Wall. Also enters the real 3D guild hall.
    // ====================================================================
    public class GuildScreen : ScreenBase
    {
        private int _tab;   // 0 info 1 hunt 2 caravan 3 war 4 tech 5 gifts 6 wish
        private InputField _createInput;
        private InputField _wishInput;

        private static readonly string[] TabNames = { "INFO", "HUNT", "CARAVAN", "WAR", "TECH", "GIFTS", "WISHES" };

        public void RequestTab(string tab)
        {
            switch (tab)
            {
                case "hunt": _tab = 1; break;
                case "caravan": _tab = 2; break;
                case "war": _tab = 3; break;
                case "tech": _tab = 4; break;
                case "gifts": _tab = 5; break;
                case "wish": _tab = 6; break;
                default: _tab = 0; break;
            }
        }

        public override void Build(UIManager manager, Transform canvasParent, string title)
        {
            base.Build(manager, canvasParent, title);

            // guild creation input (only shown when guildless)
            var createRow = UIBuilder.CreateFreeRect(Root, "CreateRow", new Vector2(1000f, 110f),
                new Vector2(0.5f, 0f), new Vector2(0f, 130f));

            var inputGo = new GameObject("CreateInput");
            var irt = inputGo.AddComponent<RectTransform>();
            irt.SetParent(createRow, false);
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = new Vector2(0f, 1f);
            irt.offsetMin = Vector2.zero;
            irt.offsetMax = new Vector2(-280f, 0f);

            var bg = inputGo.AddComponent<Image>();
            bg.sprite = UIBuilder.SolidSprite;
            bg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f);

            _createInput = inputGo.AddComponent<InputField>();
            _createInput.placeholder = CreateText(inputGo, "Ph", "Guild name (3-16 chars)...", new Color(0.5f, 0.5f, 0.55f));
            _createInput.textComponent = CreateText(inputGo, "Txt", "", Color.white);

            UIBuilder.CreateButton(createRow, "CreateBtn", "FOUND GUILD", new Vector2(260f, 100f),
                new Vector2(1f, 0f), new Vector2(-265f, 0f), new Color(0.6f, 0.45f, 0.12f, 0.95f), Color.white, 28,
                () =>
                {
                    if (_createInput == null || string.IsNullOrWhiteSpace(_createInput.text)) return;
                    GuildSystem.Create(_createInput.text);
                    _createInput.text = "";
                    Rebuild();
                });

            // wish input (wish tab)
            var wishRow = UIBuilder.CreateFreeRect(Root, "WishRow", new Vector2(1000f, 110f),
                new Vector2(0.5f, 0f), new Vector2(0f, 130f));

            var wishGo = new GameObject("WishInput");
            var wrt = wishGo.AddComponent<RectTransform>();
            wrt.SetParent(wishRow, false);
            wrt.anchorMin = Vector2.zero;
            wrt.anchorMax = new Vector2(0f, 1f);
            wrt.offsetMin = Vector2.zero;
            wrt.offsetMax = new Vector2(-280f, 0f);

            var wbg = wishGo.AddComponent<Image>();
            wbg.sprite = UIBuilder.SolidSprite;
            wbg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f);

            _wishInput = wishGo.AddComponent<InputField>();
            _wishInput.placeholder = CreateText(wishGo, "Ph", "Wish for an item name...", new Color(0.5f, 0.5f, 0.55f));
            _wishInput.textComponent = CreateText(wishGo, "Txt", "", Color.white);

            UIBuilder.CreateButton(wishRow, "WishBtn", "POST WISH", new Vector2(260f, 100f),
                new Vector2(1f, 0f), new Vector2(-265f, 0f), new Color(0.45f, 0.3f, 0.6f, 0.95f), Color.white, 28,
                () =>
                {
                    if (_wishInput == null || string.IsNullOrWhiteSpace(_wishInput.text)) return;
                    GuildSystem.PostWish(_wishInput.text);
                    _wishInput.text = "";
                    Rebuild();
                });

            // tab bar
            var tabRow = UIBuilder.CreateFreeRect(Root, "TabBar", new Vector2(1010f, 96f),
                new Vector2(0.5f, 1f), new Vector2(0f, -180f));
            for (int i = 0; i < TabNames.Length; i++)
            {
                int t = i;
                UIBuilder.CreateButton(tabRow, "Tab" + TabNames[i], TabNames[i], new Vector2(138f, 90f),
                    new Vector2(0f, 0f), new Vector2(4f + i * 143f, 0f),
                    new Color(0.22f, 0.24f, 0.32f, 0.95f), Color.white, 22,
                    () => { _tab = t; Rebuild(); });
            }
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

        public override void Rebuild()
        {
            ClearContent();

            bool inGuild = GuildSystem.InGuild;
            if (Root != null)
            {
                var createRow = Root.Find("CreateRow") as RectTransform;
                if (createRow != null) createRow.gameObject.SetActive(!inGuild);
                var wishRow = Root.Find("WishRow") as RectTransform;
                if (wishRow != null) wishRow.gameObject.SetActive(inGuild && _tab == 6);
            }

            if (!inGuild)
            {
                RebuildJoinFlow();
                return;
            }

            switch (_tab)
            {
                case 1: RebuildHunt(); break;
                case 2: RebuildCaravan(); break;
                case 3: RebuildWar(); break;
                case 4: RebuildTech(); break;
                case 5: RebuildGifts(); break;
                case 6: RebuildWishes(); break;
                default: RebuildInfo(); break;
            }
        }

        // ---------------- join / create ----------------
        private void RebuildJoinFlow()
        {
            var head = UIX.CreateRow(Content, "Head", 130f);
            UIX.AddChildText(head, "T", "Join a guild to unlock the hall, hunts,\ncaravans, war, tech, gifts and the wish wall.", 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            var suggestions = UIX.CreateRow(Content, "SuggestHead", 64f);
            UIX.AddChildText(suggestions, "T", "OPEN GUILDS", 28, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            string[] names = { "Legends United", "Dragon Covenant", "Order of Dawn", "Shadow Legion", "Storm Vanguard" };
            for (int i = 0; i < names.Length; i++)
            {
                string g = names[i];
                var row = UIX.CreateRow(Content, "G_" + g, 100f);
                UIX.AddChildText(row, "N", g + "\nLv." + UnityEngine.Random.Range(2, 7) + " • 12 members", 28, Color.white, TextAnchor.MiddleLeft, true);
                UIX.CreateRowButton(row, "JOIN", () =>
                    {
                        GuildSystem.Join(g);
                        Rebuild();
                    },
                    new Color(0.25f, 0.5f, 0.3f), 72f, 220f, 28);
            }

            var note = UIX.CreateRow(Content, "Note", 90f);
            UIX.AddChildText(note, "N", "...or type a name above and FOUND your own guild!", 26,
                new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);
        }

        // ---------------- info tab ----------------
        private void RebuildInfo()
        {
            var head = UIX.CreateRow(Content, "Head", 150f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.22f, 0.17f, 0.08f, 0.98f);
            UIX.AddChildText(head, "N", GuildSystem.GuildName + "   Lv." + GuildSystem.GuildLevel, 38,
                new Color(1f, 0.85f, 0.35f), TextAnchor.MiddleLeft, true);
            UIX.AddChildText(head, "F", "Funds: " + Economy.Format(GuildSystem.GuildFunds) + "\nMembers: " + GuildSystem.Members.Count,
                28, Color.white, TextAnchor.MiddleRight, true);

            var donate = UIX.CreateRow(Content, "Donate", 120f);
            UIX.AddChildText(donate, "D", "Donations level the guild up\n(unlocks bigger hunts & rewards)", 28,
                Color.white, TextAnchor.MiddleLeft, true);
            UIX.CreateRowButton(donate, "DONATE\n" + Economy.Format(GuildSystem.DonationCost) + " gold",
                () => { if (GuildSystem.Donate()) Rebuild(); },
                new Color(0.6f, 0.45f, 0.12f), 84f, 280f, 26);

            var hall = UIX.CreateRow(Content, "Hall", 120f, 20, 20, 10, 10);
            UIX.CreateRowButton(hall, "ENTER THE 3D GUILD HALL",
                () => { Manager.CloseScreen(); GuildHall3D.Enter(); },
                new Color(0.35f, 0.28f, 0.55f), 92f, 900f, 32);

            var memberHeader = UIX.CreateRow(Content, "MemHeader", 64f);
            UIX.AddChildText(memberHeader, "T", "MEMBERS", 28, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleLeft, true);

            foreach (var m in GuildSystem.Members)
            {
                var row = UIX.CreateRow(Content, "M_" + m.name, 76f);
                UIX.AddChildText(row, "Role", m.role, 24, m.role == "Leader" ? new Color(1f, 0.8f, 0.3f) : new Color(0.7f, 0.75f, 0.85f), TextAnchor.MiddleLeft, true, 170f);
                UIX.AddChildText(row, "Name", m.name + (m.isPlayer ? " (YOU)" : ""), 28, m.isPlayer ? new Color(0.5f, 0.9f, 1f) : Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "Power", Economy.Format(m.power), 26, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleRight, true, 220f);
            }

            var leave = UIX.CreateRow(Content, "Leave", 110f, 20, 20, 12, 12);
            UIX.CreateRowButton(leave, "LEAVE GUILD", () => { GuildSystem.Leave(); Rebuild(); },
                new Color(0.55f, 0.2f, 0.2f), 76f, 500f, 28);
        }

        // ---------------- hunt tab ----------------
        private void RebuildHunt()
        {
            float ratio = GuildSystem.HuntBossMaxHp > 0 ? (float)GuildSystem.HuntBossHp / GuildSystem.HuntBossMaxHp : 0f;

            var head = UIX.CreateRow(Content, "Head", 200f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.3f, 0.1f, 0.08f, 0.98f);
            UIX.AddChildText(head, "N", GuildSystem.HuntBossName, 40, new Color(1f, 0.6f, 0.4f), TextAnchor.MiddleCenter, true);
            UIX.AddChildText(head, "HP", Economy.Format(GuildSystem.HuntBossHp) + " / " + Economy.Format(GuildSystem.HuntBossMaxHp) + " HP\nShared boss — whole guild chips it down",
                28, Color.white, TextAnchor.MiddleCenter, true);

            var bar = UIX.CreateRow(Content, "BossBar", 90f);
            var fill = UIX.CreateLayoutBar(bar, "Fill", new Color(0.9f, 0.3f, 0.2f), 400f, 26f);
            fill.rectTransform.sizeDelta = new Vector2(400f * ratio, 0f);

            var act = UIX.CreateRow(Content, "Act", 130f, 20, 20, 12, 12);
            UIX.CreateRowButton(act, GuildSystem.HuntBossHp <= 0 ? "BOSS SLAIN — WEEKLY RESET" : "FIGHT THE BOSS\n(damage counts for everyone)",
                () => { Manager.CloseScreen(); GuildSystem.StartHunt(); },
                GuildSystem.HuntBossHp <= 0 ? new Color(0.3f, 0.32f, 0.38f) : new Color(0.7f, 0.25f, 0.15f), 100f, 900f, 30)
                .interactable = GuildSystem.HuntBossHp > 0;

            var note = UIX.CreateRow(Content, "Note", 90f);
            UIX.AddChildText(note, "N", "Every point of damage you deal reduces the shared HP — even a lost fight counts!", 26,
                new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);
        }

        // ---------------- caravan tab ----------------
        private void RebuildCaravan()
        {
            var head = UIX.CreateRow(Content, "Head", 170f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.2f, 0.2f, 0.1f, 0.98f);

            if (GuildSystem.CaravanActive)
            {
                var left = GuildSystem.CaravanEndUtc - DateTime.UtcNow;
                UIX.AddChildText(head, "T", "CARAVAN ON THE ROAD\nReturns in " + UIX.TimeShort(left) + "\nReward: " + Economy.Format(GuildSystem.CaravanReward) + " gold",
                    30, new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);
            }
            else if (GuildSystem.CaravanReady)
            {
                UIX.AddChildText(head, "T", "CARAVAN RETURNED!\nYour reward is waiting", 32, new Color(0.5f, 1f, 0.5f), TextAnchor.MiddleCenter, true);
            }
            else
            {
                UIX.AddChildText(head, "T", "GUILD CARAVAN\nSend the caravan trading across the realm", 30, Color.white, TextAnchor.MiddleCenter, true);
            }

            var options = UIX.CreateRow(Content, "Options", 130f, 12, 12, 10, 10);
            UIX.CreateRowButton(options, "15 min\n+750g xLv", () => { GuildSystem.DispatchCaravan(15); Rebuild(); },
                new Color(0.35f, 0.45f, 0.25f), 100f, 290f, 26);
            UIX.CreateRowButton(options, "60 min\n+3000g xLv", () => { GuildSystem.DispatchCaravan(60); Rebuild(); },
                new Color(0.4f, 0.5f, 0.25f), 100f, 290f, 26);
            UIX.CreateRowButton(options, "120 min\n+6000g xLv", () => { GuildSystem.DispatchCaravan(120); Rebuild(); },
                new Color(0.45f, 0.55f, 0.25f), 100f, 290f, 26);

            if (GuildSystem.CaravanReady)
            {
                var collect = UIX.CreateRow(Content, "Collect", 130f, 20, 20, 12, 12);
                UIX.CreateRowButton(collect, "COLLECT " + Economy.Format(GuildSystem.CaravanReward) + " GOLD",
                    () => { if (GuildSystem.CollectCaravan()) Rebuild(); },
                    new Color(0.7f, 0.55f, 0.1f), 100f, 900f, 32);
            }
        }

        // ---------------- war tab ----------------
        private void RebuildWar()
        {
            var head = UIX.CreateRow(Content, "Head", 170f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.28f, 0.1f, 0.14f, 0.98f);
            UIX.AddChildText(head, "W", "GUILD WAR — Season standing\nWar points: " + GuildSystem.WarPoints + "   •   Victories: " + GuildSystem.WarWins,
                30, new Color(1f, 0.7f, 0.5f), TextAnchor.MiddleCenter, true);

            var act = UIX.CreateRow(Content, "Act", 130f, 20, 20, 12, 12);
            UIX.CreateRowButton(act, "ATTACK THE RIVAL VANGUARD\n(4 enemy champions)",
                () => { Manager.CloseScreen(); GuildSystem.StartWar(); },
                new Color(0.7f, 0.2f, 0.3f), 100f, 900f, 30);

            var note = UIX.CreateRow(Content, "Note", 110f);
            UIX.AddChildText(note, "N", "Win 3 wars to unlock the \"Warlord\" title.\nWar rewards: 60 diamonds + 2,000 gold per victory.", 26,
                new Color(0.75f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);
        }

        // ---------------- tech tab ----------------
        private void RebuildTech()
        {
            var head = UIX.CreateRow(Content, "Head", 110f);
            UIX.AddChildText(head, "T", "GUILD TECH — paid with guild funds\n(everyone benefits)", 30,
                new Color(1f, 0.9f, 0.55f), TextAnchor.MiddleCenter, true);

            string[] names = { "Hunter's Might (+3% hunt dmg)", "Guard's Ward (+3% defense)", "Life Spring (+3% HP)" };
            int[] levels = { GuildSystem.TechAttack, GuildSystem.TechDefense, GuildSystem.TechHp };
            for (int i = 0; i < 3; i++)
            {
                int branch = i;
                var row = UIX.CreateRow(Content, "GT" + i, 110f);
                UIX.AddChildText(row, "N", names[i], 28, Color.white, TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "L", "Lv " + levels[i], 28, new Color(0.6f, 0.85f, 1f), TextAnchor.MiddleRight, true, 160f);
                UIX.CreateRowButton(row, "UPGRADE\n" + Economy.Format(GuildSystem.TechCost(i)) + " funds",
                    () => { if (GuildSystem.BuyTech(branch)) Rebuild(); },
                    new Color(0.3f, 0.4f, 0.6f), 80f, 300f, 26);
            }
        }

        // ---------------- gifts tab ----------------
        private void RebuildGifts()
        {
            var head = UIX.CreateRow(Content, "Head", 170f);
            var hImg = head.GetComponent<Image>();
            hImg.color = new Color(0.12f, 0.22f, 0.14f, 0.98f);
            UIX.AddChildText(head, "G", "DAILY GUILD GIFT\nGuildmates send supplies every day", 30,
                new Color(0.6f, 1f, 0.6f), TextAnchor.MiddleCenter, true);

            var contents = UIX.CreateRow(Content, "Contents", 120f);
            UIX.AddChildText(contents, "C", "+800 gold   •   +15 diamonds   •   +1 beast snack", 28,
                Color.white, TextAnchor.MiddleCenter, true);

            var act = UIX.CreateRow(Content, "Act", 130f, 20, 20, 12, 12);
            UIX.CreateRowButton(act, GuildSystem.GiftAvailable ? "CLAIM TODAY'S GIFT" : "ALREADY CLAIMED — COME BACK TOMORROW",
                () => { if (GuildSystem.ClaimGift()) Rebuild(); },
                GuildSystem.GiftAvailable ? new Color(0.25f, 0.6f, 0.3f) : new Color(0.3f, 0.32f, 0.38f), 100f, 900f, 30)
                .interactable = GuildSystem.GiftAvailable;
        }

        // ---------------- wish wall tab ----------------
        private void RebuildWishes()
        {
            var head = UIX.CreateRow(Content, "Head", 110f);
            UIX.AddChildText(head, "W", "WISH WALL — post a wish; guildmates grant it\n(yours arrive in ~2 minutes)", 28,
                new Color(0.8f, 0.6f, 1f), TextAnchor.MiddleCenter, true);

            foreach (var w in GuildSystem.Wishes)
            {
                var row = UIX.CreateRow(Content, "W_" + w.owner + w.itemName, 90f);
                UIX.AddChildText(row, "O", w.owner, 28, w.owner == PlayerProfile.Name ? new Color(0.5f, 0.9f, 1f) : Color.white, TextAnchor.MiddleLeft, true, 260f);
                UIX.AddChildText(row, "I", w.itemName, 28, new Color(1f, 0.85f, 0.4f), TextAnchor.MiddleLeft, true);
                UIX.AddChildText(row, "S", w.fulfilled ? "GRANTED" : "wishing...", 24,
                    w.fulfilled ? new Color(0.5f, 1f, 0.5f) : new Color(0.6f, 0.6f, 0.7f), TextAnchor.MiddleRight, true, 200f);
            }

            if (GuildSystem.Wishes.Count == 0)
            {
                var empty = UIX.CreateRow(Content, "Empty", 100f);
                UIX.AddChildText(empty, "E", "The wall is empty — post the first wish!", 28,
                    new Color(0.7f, 0.7f, 0.8f), TextAnchor.MiddleCenter, true);
            }

            var tick = UIX.CreateRow(Content, "Tick", 90f);
            UIX.AddChildText(tick, "T", "Tip: use the input below the title to post your own wish.", 26,
                new Color(0.65f, 0.65f, 0.75f), TextAnchor.MiddleCenter, true);
        }
    }
}
