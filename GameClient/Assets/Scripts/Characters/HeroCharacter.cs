using UnityEngine;
using LegendsRPG.Combat;
using LegendsRPG.Core;
using LegendsRPG.Vfx;

namespace LegendsRPG.Characters
{
    /// <summary>
    /// Playable hero. Controlled manually when active (touch joystick + buttons)
    /// and by TeamAIBrain / AutoBattle when not. Owns stats, HP, skills and the
    /// procedural body — all created in code, zero manual scene setup.
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
        private Transform _ring;

        public int Slot { get; private set; }
        public bool IsActive { get; private set; }

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

            var body = CharacterBodyFactory.Build(gameObject, data.element, true, 1f, data.primaryColor, data.secondaryColor,
                Art.ArtSlots.SlotForHeroClass(data.heroClass));
            Anim.Torso = body.Torso;
            Anim.Head = body.Head;
            Anim.ArmL = body.ArmL;
            Anim.ArmR = body.ArmR;
            Anim.LegL = body.LegL;
            Anim.LegR = body.LegR;
            Anim.MainRenderer = body.MainRenderer;
            WeaponTip = body.WeaponTip;
            _ring = body.Ring;

            float hp = data.baseHP + data.hpPerLevel * (level - 1);
            float atk = data.baseAttack + data.atkPerLevel * (level - 1);
            float def = data.baseDefense + data.defPerLevel * (level - 1);
            Stats.Init(hp, atk, def, data.baseMoveSpeed, data.baseAttackSpeed, data.baseAttackRange, data.element, level);
            Stats.OnLevelUp += HandleLevelUp;

            Caster.Bind(this, data.skill1, data.skill2, data.ultimate);

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

        public void SetActive(bool active)
        {
            IsActive = active;
            if (_ring != null) _ring.localScale = Vector3.one * (active ? 2.6f : 1.7f);
        }

        /// <summary>Called every frame for the manually-controlled hero.</summary>
        public void ManualTick(Vector2 stick, bool attackHeld)
        {
            if (HP == null || HP.IsDead) return;

            bool moving = stick.sqrMagnitude > 0.01f;
            if (moving)
            {
                Motor.Move(CameraRelative(stick), Stats.MoveSpeed);
            }
            else
            {
                Motor.Stop();
            }
            Anim.SetMoving(moving);

            if (attackHeld) TryBasicAttack();
        }

        /// <summary>Called every frame by TeamAIBrain for non-active heroes.</summary>
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

            if (engage)
            {
                TryBasicAttack();
                // opportunistic skill casts
                var target = CombatUtil.FindNearestMonster(transform.position, 9f);
                if (target != null)
                {
                    if (Caster.GetCooldownLeft(0) <= 0f) Caster.TryCast(0, target.transform);
                    else if (Caster.GetCooldownLeft(1) <= 0f) Caster.TryCast(1, target.transform);
                }
            }
        }

        public bool TryBasicAttack()
        {
            if (_attackCooldown > 0f) return false;

            var m = CombatUtil.FindNearestMonster(transform.position, Stats.AttackRange + 0.5f);
            if (m == null) return false;

            Motor.FaceTowards(m.transform.position);
            Anim.PlayAttack(0.32f);

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

        private Vector3 CameraRelative(Vector2 stick)
        {
            var cam = Camera.main;
            Vector3 fwd = Vector3.forward;
            Vector3 right = Vector3.right;
            if (cam != null)
            {
                fwd = cam.transform.forward; fwd.y = 0f; fwd.Normalize();
                right = cam.transform.right; right.y = 0f; right.Normalize();
            }
            return fwd * stick.y + right * stick.x;
        }
    }
}
