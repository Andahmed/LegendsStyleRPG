#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using LegendsRPG.Art;

namespace LegendsRPG.EditorTools
{
    /// <summary>
    /// Legends RPG > Art > Art Slots
    /// Assign imported art prefabs (Asset Store packs, FBX, ...) to the slots
    /// the game requests. "Auto-match" scans Assets/GameArt and fills slots by
    /// name keywords; "Save" writes Assets/Resources/ArtSlotMap.asset which is
    /// picked up automatically by every build (local or GitHub Actions).
    /// </summary>
    public class ArtSlotsWindow : EditorWindow
    {
        private static readonly string[] MonsterSlots =
        {
            "goblin", "wolf", "shaman", "golem", "wraith", "boss_titan", "boss_lich"
        };

        private static readonly string[] PropSlots =
        {
            ArtSlots.PropChest, ArtSlots.PropAltar
        };

        private readonly Dictionary<string, GameObject> _assign = new Dictionary<string, GameObject>();
        private Vector2 _scroll;
        private string _status = "";

        [MenuItem("Legends RPG/Art/Art Slots", priority = 40)]
        private static void Open()
        {
            var w = GetWindow<ArtSlotsWindow>("Art Slots");
            w.minSize = new Vector2(560f, 640f);
        }

        private void OnEnable()
        {
            LoadFromMap();
        }

        private void LoadFromMap()
        {
            _assign.Clear();
            var map = AssetDatabase.LoadAssetAtPath<ArtSlotMap>("Assets/Resources/ArtSlotMap.asset");
            if (map == null) return;
            foreach (var e in map.entries)
            {
                if (e.prefab != null && !string.IsNullOrEmpty(e.slot)) _assign[e.slot] = e.prefab;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(
                "1)  Import your art package (Assets > Import Package, or drag the .unitypackage file).\n" +
                "2)  Keep (or move) the prefabs under  Assets/GameArt/  .\n" +
                "3)  Press AUTO-MATCH — or drag prefabs onto the slots below.\n" +
                "4)  Press SAVE. Build the game — the models appear instead of the primitive bodies.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("AUTO-MATCH  (scan Assets/GameArt)", GUILayout.Height(34f)))
            {
                AutoMatch();
            }
            if (GUILayout.Button("SAVE", GUILayout.Height(34f), GUILayout.Width(110f)))
            {
                Save();
            }
            if (GUILayout.Button("Clear", GUILayout.Height(34f), GUILayout.Width(70f)))
            {
                _assign.Clear();
                _status = "Cleared (not saved yet).";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show GameArt folder"))
            {
                if (!Directory.Exists("Assets/GameArt")) Directory.CreateDirectory("Assets/GameArt");
                EditorUtility.RevealInFinder("Assets/GameArt");
            }
            if (GUILayout.Button("Reload saved map")) LoadFromMap();
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_status))
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.HelpBox(_status, MessageType.None);
            }

            EditorGUILayout.Space(6f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("HERO SLOTS  (one model per hero class)", EditorStyles.boldLabel);
            DrawSlotRow(ArtSlots.HeroWarrior, "Warrior  —  Kael, Iskra, Korrag, Bram");
            DrawSlotRow(ArtSlots.HeroMage, "Mage  —  Sylra, Wren, Maro, Morvath");
            DrawSlotRow(ArtSlots.HeroArcher, "Archer  —  Rania, Juno, Vashta, Zeph");
            DrawSlotRow(ArtSlots.HeroPaladin, "Paladin  —  Doran, Odo, Lyra");
            DrawSlotRow(ArtSlots.HeroAssassin, "Assassin  —  Nyx, Tamsin, Seraphel");
            DrawSlotRow(ArtSlots.HeroHealer, "Healer  —  Verda, Pell, Fenn");

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("MONSTER SLOTS  (slot name = monster id)", EditorStyles.boldLabel);
            DrawSlotRow("goblin", "Ember Goblin");
            DrawSlotRow("wolf", "Frost Wolf");
            DrawSlotRow("shaman", "Tide Shaman");
            DrawSlotRow("golem", "Moss Golem");
            DrawSlotRow("wraith", "Night Wraith");
            DrawSlotRow("boss_titan", "Ember Titan (boss)");
            DrawSlotRow("boss_lich", "Ancient Lich (boss)");

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("PROP SLOTS  (optional decorations)", EditorStyles.boldLabel);
            DrawSlotRow(ArtSlots.PropChest, "AFK / reward chest prop");
            DrawSlotRow(ArtSlots.PropAltar, "Summon altar prop");

            EditorGUILayout.EndScrollView();
        }

        private void DrawSlotRow(string slot, string hint)
        {
            EditorGUILayout.BeginHorizontal();

            var label = new GUIContent(slot, hint);
            EditorGUILayout.LabelField(label, GUILayout.Width(150f));

            GameObject current;
            _assign.TryGetValue(slot, out current);
            var next = (GameObject)EditorGUILayout.ObjectField(current, typeof(GameObject), false);
            if (next != current)
            {
                if (next == null) _assign.Remove(slot);
                else _assign[slot] = next;
            }

            GUILayout.Label(current != null ? "OK" : "-", GUILayout.Width(28f));

            EditorGUILayout.EndHorizontal();
        }

        // ---------------- actions ----------------

        private void AutoMatch()
        {
            if (!Directory.Exists("Assets/GameArt"))
            {
                _status = "Assets/GameArt does not exist yet — import your art package first.";
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/GameArt" });
            if (guids.Length == 0)
            {
                _status = "No prefabs found under Assets/GameArt. Imported prefabs may live in another folder — " +
                          "move them (or the whole pack folder) under Assets/GameArt, or assign them manually below.";
                return;
            }

            var used = new HashSet<string>();
            int found = 0;
            foreach (var pair in ArtSlots.AutoMatch)
            {
                if (_assign.ContainsKey(pair.Key)) continue;
                foreach (var g in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(g);
                    string lower = path.ToLowerInvariant();
                    bool match = false;
                    foreach (var kw in pair.Value)
                    {
                        if (lower.Contains(kw)) { match = true; break; }
                    }
                    if (!match) continue;
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null) continue;
                    if (!used.Add(path)) continue;

                    _assign[pair.Key] = prefab;
                    found++;
                    Debug.Log("[ArtSlots] " + pair.Key + "  <-  " + path);
                    break;
                }
            }

            int total = _assign.Count;
            _status = found > 0
                ? "Auto-match finished: " + found + " new slot(s) filled, " + total + " assigned in total. Review the list and press SAVE."
                : "Auto-match found no name matches (" + guids.Length + " prefabs scanned). " +
                  "Assign the prefabs manually with the object fields, then press SAVE.";
            Debug.Log("[ArtSlots] " + _status);
        }

        private void Save()
        {
            if (!Directory.Exists("Assets/Resources")) Directory.CreateDirectory("Assets/Resources");
            var map = AssetDatabase.LoadAssetAtPath<ArtSlotMap>("Assets/Resources/ArtSlotMap.asset");
            if (map == null)
            {
                map = ScriptableObject.CreateInstance<ArtSlotMap>();
                AssetDatabase.CreateAsset(map, "Assets/Resources/ArtSlotMap.asset");
            }

            map.entries.Clear();
            foreach (var kv in _assign)
            {
                if (kv.Value == null) continue;
                map.entries.Add(new ArtSlotMap.Entry { slot = kv.Key, prefab = kv.Value });
            }

            EditorUtility.SetDirty(map);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _status = "Saved " + map.entries.Count + " slot(s) to Assets/Resources/ArtSlotMap.asset — " +
                      "they will be included in every build from now on.";
            Debug.Log("[ArtSlots] " + _status);
        }
    }
}
#endif
