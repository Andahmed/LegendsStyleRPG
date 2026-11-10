using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using LegendsRPG.Characters;

namespace LegendsRPG.Art
{
    /// <summary>
    /// Runtime bridge for the Quaternius FBX packs stored below Resources.
    /// The project remains playable when Unity has not imported the new assets:
    /// every method is null-safe and callers keep their procedural fallback.
    /// </summary>
    public static class QuaterniusAssetLibrary
    {
        private const string MonstersRoot = "GameArt/Free3D/Quaternius_AnimatedMonsters/FBX/";
        private const string NatureRoot = "GameArt/Free3D/Quaternius_Nature/FBX/";
        private const string DungeonRoot = "GameArt/Free3D/Quaternius_ModularDungeon/FBX/";
        private const string HeroesRoot = "GameArt/Free3D/Quaternius_RPGCharacters/FBX/";

        private static readonly Dictionary<HeroClass, string> HeroModels =
            new Dictionary<HeroClass, string>
            {
                { HeroClass.Warrior, "Warrior" },
                { HeroClass.Mage, "Wizard" },
                { HeroClass.Archer, "Ranger" },
                { HeroClass.Paladin, "Cleric" },
                { HeroClass.Assassin, "Rogue" },
                { HeroClass.Healer, "Monk" }
            };

        private static readonly Dictionary<string, string> MonsterModels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "goblin", "Slime" },
                { "wolf", "Slime" },
                { "shaman", "Skeleton" },
                { "golem", "Dragon" },
                { "wraith", "Skeleton" },
                { "storm_hawk", "Bat" },
                { "coral_crab", "Slime" },
                { "vine_spider", "Slime" },
                { "sun_eagle", "Bat" },
                { "boss_titan", "Dragon" },
                { "boss_lich", "Skeleton" },
                { "chapter_boss_01", "Dragon" }, { "chapter_boss_02", "Slime" },
                { "chapter_boss_03", "Slime" }, { "chapter_boss_04", "Dragon" },
                { "chapter_boss_05", "Dragon" }, { "chapter_boss_06", "Bat" },
                { "chapter_boss_07", "Dragon" }, { "chapter_boss_08", "Skeleton" },
                { "chapter_boss_09", "Bat" }, { "chapter_boss_10", "Skeleton" },
                { "chapter_boss_11", "Bat" }, { "chapter_boss_12", "Dragon" },
                { "world_boss_1", "Dragon" }, { "world_boss_2", "Dragon" },
                { "world_boss_3", "Dragon" }, { "world_boss_4", "Bat" },
                { "world_boss_5", "Skeleton" }
            };

        private static readonly string[] CityTrees =
        {
            "CommonTree_1", "CommonTree_3", "PineTree_2", "BirchTree_1",
            "Willow_1", "CommonTree_Autumn_2", "Bush_1", "BushBerries_1"
        };

        private static readonly string[] DungeonProps =
        {
            "Floor_Modular", "Column", "Torch", "Chest_Gold", "Barrel",
            "Skull", "Banner_wall", "Woodfire"
        };

        public static GameObject InstantiateMonster(string monsterId, Transform parent, float targetHeight)
        {
            string modelName;
            if (!MonsterModels.TryGetValue(monsterId ?? string.Empty, out modelName)) return null;
            return InstantiateModel(MonstersRoot + modelName, parent, targetHeight, true);
        }

        /// <summary>Applies an element/boss tint to imported FBX materials without replacing their textures.</summary>
        public static void TintModel(GameObject model, Color tint, float strength = 0.38f)
        {
            if (model == null) return;
            strength = Mathf.Clamp01(strength);
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var mats = renderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    var mat = mats[j];
                    if (mat == null) continue;
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color baseColor = mat.GetColor("_BaseColor");
                        mat.SetColor("_BaseColor", Color.Lerp(baseColor, tint, strength));
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        Color baseColor = mat.GetColor("_Color");
                        mat.SetColor("_Color", Color.Lerp(baseColor, tint, strength));
                    }
                }
            }
        }

        public static GameObject InstantiateHero(HeroClass heroClass, Transform parent, float targetHeight)
        {
            string modelName;
            if (!HeroModels.TryGetValue(heroClass, out modelName)) return null;
            return InstantiateModel(HeroesRoot + modelName, parent, targetHeight, true);
        }

        public static GameObject InstantiateNature(string modelName, Transform parent, Vector3 position,
            float targetHeight, float yaw)
        {
            var go = InstantiateModel(NatureRoot + modelName, parent, targetHeight, false);
            if (go == null) return null;
            go.name = "QuaterniusNature_" + modelName;
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            return go;
        }

        public static GameObject InstantiateDungeonProp(string modelName, Transform parent, Vector3 position,
            float targetHeight, float yaw)
        {
            var go = InstantiateModel(DungeonRoot + modelName, parent, targetHeight, false);
            if (go == null) return null;
            go.name = "QuaterniusDungeon_" + modelName;
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            return go;
        }

        /// <summary>Creates a light city nature dressing and reports whether at least one FBX loaded.</summary>
        public static bool BuildCityNature(Transform parent)
        {
            if (parent == null) return false;
            int placed = 0;
            for (int i = 0; i < 16; i++)
            {
                float a = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
                float r = UnityEngine.Random.Range(12f, 42f);
                if (r > 20f && r < 32f) { i--; continue; }

                string name = CityTrees[UnityEngine.Random.Range(0, CityTrees.Length)];
                float height = name.StartsWith("Bush", StringComparison.OrdinalIgnoreCase)
                    ? UnityEngine.Random.Range(1.0f, 1.8f)
                    : UnityEngine.Random.Range(3.0f, 5.2f);
                var go = InstantiateNature(name, parent,
                    new Vector3(Mathf.Sin(a) * r, 0f, Mathf.Cos(a) * r), height,
                    UnityEngine.Random.Range(0f, 360f));
                if (go != null) placed++;
            }
            return placed > 0;
        }

        /// <summary>Uses modular dungeon pieces in the arena when the FBX assets are imported.</summary>
        public static bool BuildDungeonProps(Transform parent)
        {
            if (parent == null) return false;
            int placed = 0;
            var positions = new[]
            {
                new Vector3(-10f, 0f, -10f), new Vector3(10f, 0f, -10f),
                new Vector3(-10f, 0f, 10f), new Vector3(10f, 0f, 10f),
                new Vector3(0f, 0f, 14f), new Vector3(0f, 0f, -14f),
                new Vector3(-14f, 0f, 0f), new Vector3(14f, 0f, 0f)
            };
            for (int i = 0; i < positions.Length; i++)
            {
                string name = DungeonProps[i % DungeonProps.Length];
                float h = name == "Floor_Modular" ? 1.0f : 2.0f;
                var go = InstantiateDungeonProp(name, parent, positions[i], h, i * 45f);
                if (go != null) placed++;
            }
            return placed > 0;
        }

        public static void HideProceduralBody(BodyRefs body)
        {
            var parts = new[] { body.Torso, body.Head, body.ArmL, body.ArmR, body.LegL, body.LegR };
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null) continue;
                var renderers = parts[i].GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < renderers.Length; j++) renderers[j].enabled = false;
            }
        }

        private static GameObject InstantiateModel(string resourcePath, Transform parent, float targetHeight,
            bool addAnimationDriver)
        {
            if (parent == null) return null;
            GameObject prefab;
            try
            {
                prefab = Resources.Load<GameObject>(resourcePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Quaternius] Failed to load " + resourcePath + ": " + e.Message);
                return null;
            }
            if (prefab == null) return null;

            GameObject go;
            try
            {
                go = UnityEngine.Object.Instantiate(prefab, parent, false);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Quaternius] Failed to instantiate " + resourcePath + ": " + e.Message);
                return null;
            }

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var colliders = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) UnityEngine.Object.Destroy(colliders[i]);
            NormalizeAndGround(go, targetHeight);

            if (addAnimationDriver)
            {
                var animator = go.GetComponent<Animator>();
                if (animator == null) animator = go.AddComponent<Animator>();
                var driver = go.GetComponent<QuaterniusClipDriver>();
                if (driver == null) driver = go.AddComponent<QuaterniusClipDriver>();
                driver.Initialize(animator, Resources.LoadAll<AnimationClip>(resourcePath));
            }
            return go;
        }

        private static void NormalizeAndGround(GameObject go, float targetHeight)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            if (targetHeight > 0f && bounds.size.y > 0.0001f)
                go.transform.localScale *= targetHeight / bounds.size.y;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            go.transform.position += Vector3.up * (go.transform.position.y - bounds.min.y);
        }
    }

    /// <summary>
    /// Plays the animation clips embedded in a Quaternius FBX through a
    /// PlayableGraph, so no AnimatorController asset is required in the repo.
    /// </summary>
    public sealed class QuaterniusClipDriver : MonoBehaviour
    {
        private Animator _animator;
        private AnimationClip[] _clips = new AnimationClip[0];
        private PlayableGraph _graph;
        private string _state;

        public void Initialize(Animator animator, AnimationClip[] clips)
        {
            _animator = animator;
            _clips = clips ?? new AnimationClip[0];
            PlayIdle();
        }

        public void SetMoving(bool moving)
        {
            if (moving) PlayFirst("Walk", "Flying", "Idle");
            else PlayIdle();
        }

        public void PlayAttack()
        {
            PlayFirst("Attack", "Punch", "Hit");
        }

        public void PlaySkill()
        {
            PlayFirst("Cast", "Skill", "Attack", "Hit");
        }

        public void PlayDeath()
        {
            PlayFirst("Death");
        }

        private void PlayIdle()
        {
            PlayFirst("Idle", "Flying", "Walk");
        }

        private void PlayFirst(params string[] keywords)
        {
            AnimationClip clip = null;
            for (int k = 0; k < keywords.Length && clip == null; k++)
            {
                for (int i = 0; i < _clips.Length; i++)
                {
                    if (_clips[i] != null && _clips[i].name.IndexOf(keywords[k], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        clip = _clips[i];
                        break;
                    }
                }
            }
            if (clip == null || _animator == null || _state == clip.name) return;
            _state = clip.name;
            if (_graph.IsValid()) _graph.Destroy();
            _graph = PlayableGraph.Create(name + "Graph");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(_graph, "QuaterniusAnimation", _animator);
            var playable = AnimationClipPlayable.Create(_graph, clip);
            playable.SetDuration(clip.length);
            output.SetSourcePlayable(playable);
            _graph.Play();
        }

        private void OnDestroy()
        {
            if (_graph.IsValid()) _graph.Destroy();
        }
    }
}
