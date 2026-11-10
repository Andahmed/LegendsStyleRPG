using UnityEngine;
using LegendsRPG.Combat;
using LegendsRPG.Core;
using LegendsRPG.Vfx;
using LegendsRPG.Art;

namespace LegendsRPG.Characters
{
    /// <summary>
    /// v1.1 (Legends Reborn style): a fully autonomous hero. Movement, basic
    /// attacks and (while AUTO is on) skills are driven by TeamAIBrain — there
    /// is no manual control and no active-hero switching anymore. The player
    /// only fires an Ultimate by tapping the hero's HUD portrait when its Rage
    /// is full. Owns stats, HP, skills and the procedural body — all created
    /// in code, zero manual scene setup.
    /// </summary>
    public class HeroCharacter : MonoBehaviour
    {
        public HeroData Data { get; private set; }
        public CharacterStats Stats { get; private set; }
        public Health HP { get; private set; }
        public CharacterMotor Motor { get; private set; }
        public CharacterAnimator Anim { get; private set; }
        public SkillCaster Caster { get; private set; }
        public Transform WeaponTip { get; private set; }

        private QuaterniusClipDriver _modelAnimator;

        public int Slot { get; private set; }

        private float _attackCooldown;

        public void Setup(HeroData data, int level, int slot, Vector3 spawnPos)
        {
            Data = data;
            Slot = slot;
            name = "Hero_" + data.displayName;

            var cc = gameObject.AddComponent<CharacterController>();
            cc.height = 2.2f;
            cc.radius = 0.42f;
            cc.center = new Vector3(0f, 1.1f, 0f);
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.4f;

            Motor = gameObject.AddComponent<CharacterMotor>();
            Stats = gameObject.AddComponent<CharacterStats>();
            HP = gameObject.AddComponent<Health>();
            Anim = gameObject.AddComponent<CharacterAnimator>();
            Caster = gameObject.AddComponent<SkillCaster>();

            var body = CharacterBodyFactory.Build(gameObject, data.element, true, 1f, data.primaryColor, data.secondaryColor);
            Anim.Torso = body.Torso;
            Anim.Head = body.Head;
            Anim.ArmL = body.ArmL;
            Anim.ArmR = body.ArmR;
            Anim.LegL = body.LegL;
            Anim.LegR = body.LegR;
            Anim.MainRenderer = body.MainRenderer;
            WeaponTip = body.WeaponTip;

            // Use the imported, rigged Quaternius hero model when available.
            // Keep the procedural body as a fallback for missing/unimported assets.
            var importedModel = QuaterniusAssetLibrary.InstantiateHero(data.heroClass, transform, 2.35f);
            if (importedModel != null)
            {
                QuaterniusAssetLibrary.HideProceduralBody(body);
                _modelAnimator = importedModel.GetComponent<QuaterniusClipDriver>();
            }

            // v1.0: per-hero progression (own level + ascension stars)
            if (Core.HeroProgress.LevelOf(data.heroId) > 1) level = Core.HeroProgress.LevelOf(data.heroId);
            float starMult = Core.HeroProgress.StarMult(Core.HeroProgress.StarsOf(data.heroId));

            float hp = (data.baseHP + data.hpPerLevel * (level - 1)) * starMult;
            float atk = (data.baseAttack + data.atkPerLevel * (level - 1)) * starMult;
            float def = (data.baseDefense + data.defPerLevel * (level - 1)) * starMult;
            Stats.Init(hp, atk, def, data.baseMoveSpeed, data.baseAttackSpeed, data.baseAttackRange, data.element, level);
            Core.TechSystem.ApplyToHero(Stats);
            World.BeastSystem.ApplyToHero(Stats);
            Stats.OnLevelUp += HandleLevelUp;

            Caster.Bind(this, data.skill1, data.skill2, data.ultimate);

            // v1.1: small HP bar above every unit (billboarded, code-drawn)
            gameObject.AddComponent<Combat.UnitHealthBar>().Bind(HP, 0.95f, 0.14f, 2.75f, true);

            transform.position = spawnPos;
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }

        private void HandleLevelUp(int newLevel)
        {
            Stats.ApplyLevelGrowth(Data.hpPerLevel, Data.atkPerLevel, Data.defPerLevel);
            HP.Current = HP.Max; // full heal on level up
            DamagePopup.Spawn(transform.position, "LEVEL " + newLevel, new Color(1f, 0.85f, 0.3f), true);
            VFXFactory.Spawn("levelup", transform.position, new Color(1f, 0.85f, 0.3f), 1.2f);
            Audio.AudioSystem.Play("LevelUp");
            CombatEvents.RaiseLevelUp(Slot);
        }

        /// <summary>Called every frame by TeamAIBrain (every hero, always).</summary>
        public void AutoTick(Vector3 destination, bool engage)
        {
            if (HP == null || HP.IsDead) return;

            Vector3 toDest = destination - transform.position;
            toDest.y = 0f;
            bool moving = toDest.magnitude > 0.4f;
            if (moving)
            {
                Motor.Move(toDest.normalized, Stats.MoveSpeed * 0.92f);
            }
            else
            {
                Motor.Stop();
            }
            Anim.SetMoving(moving);
            if (_modelAnimator != null) _modelAnimator.SetMoving(moving);

            if (engage)
            {
                TryBasicAttack();
            }
        }

        /// <summary>Stick-driven movement + optional basic attack (used by AutoBattleController).</summary>
        public void ManualTick(Vector2 stick, bool attack)
        {
            if (HP == null || HP.IsDead) return;
            bool moving = stick.sqrMagnitude > 0.01f;
            if (moving) Motor.Move(new Vector3(stick.x, 0f, stick.y).normalized, Stats.MoveSpeed);
            else Motor.Stop();
            Anim.SetMoving(moving);
            if (_modelAnimator != null) _modelAnimator.SetMoving(moving);
            if (attack) TryBasicAttack();
        }

        public bool TryBasicAttack()
        {
            if (_attackCooldown > 0f) return false;

            var m = CombatUtil.FindNearestMonster(transform.position, Stats.AttackRange + 0.5f);
            if (m == null) return false;

            Motor.FaceTowards(m.transform.position);
            Anim.PlayAttack(0.32f);
            if (_modelAnimator != null) _modelAnimator.PlayAttack();

            float dmg = Stats.Attack;
            CombatUtil.DealDamage(Stats, m.Stats, dmg, Data.element, false, gameObject);
            VFXFactory.Spawn("slash", m.transform.position + Vector3.up * 1.2f,
                Data.primaryColor, 0.8f);
            Audio.AudioSystem.Play("Hit", 0.6f);

            _attackCooldown = 1f / Mathf.Max(0.2f, Stats.AttackSpeed);
            return true;
        }

        private void Update()
        {
            if (_attackCooldown > 0f) _attackCooldown -= Time.deltaTime;
        }
    }
}
