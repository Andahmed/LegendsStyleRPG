using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LegendsReborn.Rebuild
{
    public sealed class LegendsBootstrap : MonoBehaviour
    {
        public static LegendsBootstrap Instance { get; private set; }

        private readonly List<EnemyBrain> enemies = new List<EnemyBrain>();
        private PlayerMotor player;
        private Text statusText;
        private Text goldText;
        private Image healthFill;
        private Image manaFill;
        private float gold;
        private int level = 1;
        private int xp;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntime()
        {
            if (FindObjectOfType<LegendsBootstrap>() == null)
            {
                new GameObject("Legends Reborn Runtime").AddComponent<LegendsBootstrap>();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            ConfigureEnvironment();
            BuildWorld();
            BuildPlayer();
            BuildEnemies();
            BuildHud();
            RefreshHud();
            InvokeRepeating(nameof(RefreshHud), 0.1f, 0.1f);
        }

        private void ConfigureEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.12f, 0.17f, 0.24f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.43f, 0.52f, 0.66f);
            RenderSettings.ambientEquatorColor = new Color(0.27f, 0.32f, 0.37f);
            RenderSettings.ambientGroundColor = new Color(0.1f, 0.12f, 0.1f);
            QualitySettings.shadowDistance = 65f;

            GameObject sun = new GameObject("World Sun");
            Light sunlight = sun.AddComponent<Light>();
            sunlight.type = LightType.Directional;
            sunlight.intensity = 1.15f;
            sunlight.color = new Color(1f, 0.88f, 0.72f);
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }

        private void BuildWorld()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Training Grounds";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(12f, 1f, 12f);
            ground.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.19f, 0.27f, 0.2f), 0f);

            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Arch",
                new Vector3(0f, 0f, 23f), null, 5.8f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Column",
                new Vector3(-5f, 0f, 20f), null, 4.2f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Column",
                new Vector3(5f, 0f, 20f), null, 4.2f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Banner",
                new Vector3(-3.2f, 0f, 20f), null, 3.2f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Banner",
                new Vector3(3.2f, 0f, 20f), null, 3.2f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Torch",
                new Vector3(-2.7f, 0f, 7f), null, 2f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Torch",
                new Vector3(2.7f, 0f, 7f), null, 2f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Chest_Gold",
                new Vector3(7f, 0f, 2.5f), null, 1.35f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Barrel",
                new Vector3(-6f, 0f, 4f), null, 1.15f);
            SpawnVisual("GameArt/Free3D/Quaternius_ModularDungeon/FBX/Crate",
                new Vector3(-7.2f, 0f, 4.3f), null, 1.1f);

            string tree = "GameArt/Free3D/Quaternius_Nature/FBX/CommonTree_1";
            string pine = "GameArt/Free3D/Quaternius_Nature/FBX/PineTree_1";
            Vector3[] treeSpots =
            {
                new Vector3(-18f,0f,-5f), new Vector3(-22f,0f,8f), new Vector3(-17f,0f,18f),
                new Vector3(18f,0f,-5f), new Vector3(22f,0f,8f), new Vector3(17f,0f,18f),
                new Vector3(-11f,0f,27f), new Vector3(11f,0f,27f),
                new Vector3(-24f,0f,-20f), new Vector3(24f,0f,-20f)
            };
            for (int i = 0; i < treeSpots.Length; i++)
            {
                SpawnVisual(i % 2 == 0 ? tree : pine, treeSpots[i], null, 4.8f);
            }
        }

        private void BuildPlayer()
        {
            GameObject hero = new GameObject("Player - Warrior");
            hero.transform.position = new Vector3(0f, 0.05f, 0f);
            CharacterController controller = hero.AddComponent<CharacterController>();
            controller.height = 1.85f;
            controller.radius = 0.36f;
            controller.center = new Vector3(0f, 0.92f, 0f);
            controller.skinWidth = 0.08f;

            player = hero.AddComponent<PlayerMotor>();
            SpawnVisual("GameArt/Free3D/Quaternius_RPGCharacters/FBX/Warrior",
                hero.transform.position, hero.transform, 1.8f);

            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cam = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            CameraFollow follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.Target = hero.transform;
            follow.Offset = new Vector3(0f, 3.6f, -6.8f);
            player.Initialize(cam.transform);
        }

        private void BuildEnemies()
        {
            SpawnEnemy("GameArt/Free3D/Quaternius_AnimatedMonsters/FBX/Slime",
                new Vector3(-3.5f, 0f, 10f), "Moss Slime", 1.25f);
            SpawnEnemy("GameArt/Free3D/Quaternius_AnimatedMonsters/FBX/Skeleton",
                new Vector3(3.4f, 0f, 12f), "Forgotten Skeleton", 1.85f);
            SpawnEnemy("GameArt/Free3D/Quaternius_AnimatedMonsters/FBX/Bat",
                new Vector3(0f, 0f, 15f), "Cave Bat", 1.35f);
        }

        private void SpawnEnemy(string assetPath, Vector3 position, string enemyName, float height)
        {
            GameObject enemy = new GameObject(enemyName);
            enemy.transform.position = position;
            SpawnVisual(assetPath, position, enemy.transform, height);
            EnemyBrain brain = enemy.AddComponent<EnemyBrain>();
            brain.Initialize(player, OnEnemyDefeated);
            enemies.Add(brain);
        }

        private GameObject SpawnVisual(string resourcePath, Vector3 position, Transform parent, float targetHeight)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogWarning("Missing retained graphics asset: " + resourcePath);
                return null;
            }

            GameObject visual = Instantiate(prefab, position, Quaternion.identity, parent);
            visual.name = prefab.name;
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return visual;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float factor = targetHeight / Mathf.Max(bounds.size.y, 0.01f);
            Vector3 originalPosition = visual.transform.position;
            float bottomOffset = bounds.min.y - originalPosition.y;
            visual.transform.localScale = visual.transform.localScale * factor;
            visual.transform.position = originalPosition + Vector3.up * (-bottomOffset * factor);
            return visual;
        }

        private void BuildHud()
        {
            if (EventSystem.current == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            GameObject canvasObject = new GameObject("Gameplay HUD");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            CreateText(canvas.transform, "Game Title", "LEGENDS REBORN",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -22f),
                new Vector2(450f, 52f), 34, TextAnchor.MiddleLeft, new Color(1f, 0.82f, 0.48f));
            CreateText(canvas.transform, "Class Label", "WARRIOR  •  LEVEL 01",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -76f),
                new Vector2(380f, 34f), 20, TextAnchor.MiddleLeft, Color.white);

            Image healthBack = CreateImage(canvas.transform, "Health Bar Back",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -122f),
                new Vector2(360f, 24f), new Color(0.05f, 0.07f, 0.1f, 0.9f));
            healthFill = CreateImage(healthBack.transform, "Health Fill",
                new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero,
                new Vector2(360f, 24f), new Color(0.72f, 0.12f, 0.17f, 1f));
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;

            Image manaBack = CreateImage(canvas.transform, "Mana Bar Back",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -154f),
                new Vector2(280f, 13f), new Color(0.05f, 0.07f, 0.1f, 0.9f));
            manaFill = CreateImage(manaBack.transform, "Mana Fill",
                new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero,
                new Vector2(280f, 13f), new Color(0.1f, 0.55f, 0.92f, 1f));
            manaFill.type = Image.Type.Filled;
            manaFill.fillMethod = Image.FillMethod.Horizontal;
            manaFill.fillOrigin = (int)Image.OriginHorizontal.Left;

            goldText = CreateText(canvas.transform, "Gold Counter", "GOLD  0",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f),
                new Vector2(260f, 45f), 24, TextAnchor.MiddleRight, new Color(1f, 0.82f, 0.38f));
            statusText = CreateText(canvas.transform, "Status Message", "Defeat the creatures near the gate",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -38f),
                new Vector2(700f, 44f), 22, TextAnchor.MiddleCenter, Color.white);

            Image joystickBack = CreateImage(canvas.transform, "Movement Joystick",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(142f, 145f),
                new Vector2(190f, 190f), new Color(0.05f, 0.08f, 0.14f, 0.48f));
            joystickBack.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            joystickBack.gameObject.AddComponent<MobileJoystick>();
            Image joystickHandle = CreateImage(joystickBack.transform, "Joystick Handle",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(76f, 76f), new Color(0.55f, 0.72f, 0.88f, 0.7f));
            joystickBack.GetComponent<MobileJoystick>().Initialize(joystickHandle.rectTransform);
            player.SetJoystick(joystickBack.GetComponent<MobileJoystick>());

            CreateButton(canvas.transform, "Attack Button", "ATTACK",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-150f, 145f),
                new Vector2(138f, 138f), new Color(0.62f, 0.17f, 0.16f, 0.94f), BasicAttack);
            CreateButton(canvas.transform, "Skill Button", "SKILL",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-305f, 82f),
                new Vector2(112f, 112f), new Color(0.14f, 0.34f, 0.7f, 0.94f), CastSkill);
            CreateButton(canvas.transform, "Dash Button", "DASH",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-65f, 285f),
                new Vector2(92f, 92f), new Color(0.16f, 0.42f, 0.38f, 0.94f), () => player.Dash());
        }

        public void BasicAttack()
        {
            EnemyBrain target = FindClosestEnemy(3.8f);
            if (target == null)
            {
                SetMessage("Move closer to an enemy");
                return;
            }

            target.TakeDamage(34f);
            SetMessage("Warrior strike!");
        }

        private void CastSkill()
        {
            if (!player.SpendMana(25f))
            {
                SetMessage("Not enough mana");
                return;
            }

            int hits = 0;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyBrain enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                if (Vector3.Distance(player.transform.position, enemy.transform.position) <= 6f)
                {
                    enemy.TakeDamage(58f);
                    hits++;
                }
            }
            SetMessage(hits > 0 ? "Arcane shockwave!" : "No enemies in skill range");
            RefreshHud();
        }

        private EnemyBrain FindClosestEnemy(float maxDistance)
        {
            EnemyBrain closest = null;
            float best = maxDistance;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyBrain enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
                if (distance < best)
                {
                    best = distance;
                    closest = enemy;
                }
            }
            return closest;
        }

        private void OnEnemyDefeated(EnemyBrain enemy)
        {
            gold += 25f;
            xp += 40;
            if (xp >= 100)
            {
                xp -= 100;
                level++;
                SetMessage("LEVEL UP!  Level " + level);
            }
            else
            {
                SetMessage("Enemy defeated  +25 gold  +40 XP");
            }
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (player == null) return;
            if (healthFill != null) healthFill.fillAmount = player.Health / player.MaxHealth;
            if (manaFill != null) manaFill.fillAmount = player.Mana / player.MaxMana;
            if (goldText != null) goldText.text = "GOLD  " + Mathf.RoundToInt(gold);
        }

        private void SetMessage(string message)
        {
            if (statusText != null) statusText.text = message;
        }

        private Image CreateImage(Transform parent, string objectName, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(Transform parent, string objectName, string value,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size,
            int fontSize, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(anchorMin.x, anchorMin.y);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private void CreateButton(Transform parent, string objectName, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size,
            Color color, UnityEngine.Events.UnityAction action)
        {
            Image image = CreateImage(parent, objectName, anchorMin, anchorMax, position, size, color);
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            Text text = CreateText(image.transform, objectName + " Label", label,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                22, TextAnchor.MiddleCenter, Color.white);
            text.fontStyle = FontStyle.Bold;
        }
    }
}
