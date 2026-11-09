using System.Collections;
using UnityEngine;
using LegendsRPG.Combat;
using LegendsRPG.Core;
using LegendsRPG.Vfx;

namespace LegendsRPG.Characters
{
    /// <summary>
    /// Enemy with a real 3D state machine: Idle → Chase → Attack → Leash/Return.
    /// Registries feed hero targeting; death routes EXP to the team and notifies
    /// the WaveSpawner so floor progression works end to end.
    /// </summary>
    public class MonsterCharacter : MonoBehaviour
    {
        public static readonly System.Collections.Generic.List<MonsterCharacter> All =
            new System.Collections.Generic.List<MonsterCharacter>();

        public MonsterData Data { get; private set; }
        public CharacterStats Stats { get; private set; }
        public Health HP { get; private set; }
        public CharacterMotor Motor { get; private set; }
        public CharacterAnimator Anim { get; private set; }

        public event System.Action<MonsterCharacter> MonsterDied;

        private enum State { Idle, Chase, Attack, Return }
        private State _state = State.Idle;
        private Vector3 _spawnPoint;
        private float _attackCooldown;
        private HeroCharacter _target;
        private bool _registered;

        public void Setup(MonsterData data, float hpScale, float atkScale, float expScale, Vector3 spawnPos)
        {
            Data = data;
            name = "Monster_" + data.displayName + "_" + Random.Range(100, 999);
            _spawnPoint = spawnPos;

            var cc = gameObject.AddComponent<CharacterController>();
            cc.height = 2.1f * data.scale;
            cc.radius = 0.45f * data.scale;
            cc.center = new Vector3(0f, 1.05f * data.scale, 0f);

            Motor = gameObject.AddComponent<CharacterMotor>();
            Stats = gameObject.AddComponent<CharacterStats>();
            HP = gameObject.AddComponent<Health>();
            Anim = gameObject.AddComponent<CharacterAnimator>();

            var body = CharacterBodyFactory.Build(gameObject, data.element, false, data.scale,
                data.primaryColor, data.secondaryColor);
            Anim.Torso = body.Torso;
            Anim.Head = body.Head;
            Anim.ArmL = body.ArmL;
            Anim.ArmR = body.ArmR;
            Anim.LegL = body.LegL;
            Anim.LegR = body.LegR;

            Stats.Init(data.baseHP * hpScale, data.baseAttack * atkScale, data.baseDefense,
                data.moveSpeed, data.attackSpeed, data.attackRange, data.element, 1);

            // v1.1: small HP bar above every unit (billboarded, code-drawn)
            gameObject.AddComponent<Combat.UnitHealthBar>().Bind(HP, 1.15f * data.scale, 0.13f, 2.45f * data.scale, false);

            HP.Died += HandleDeath;
            transform.position = spawnPos;

            if (data.isBoss)
            {
                Audio.AudioSystem.Play("BossRoar");
                CombatEvents.RaiseNotification("BOSS: " + data.displayName + "!");
                VFXFactory.Spawn("explosion", spawnPos + Vector3.up, data.primaryColor, 2.2f);
            }
        }

        private void OnEnable()
        {
            if (!_registered)
            {
                All.Add(this);
                _registered = true;
            }
        }

        private void OnDisable()
        {
            if (_registered)
            {
                All.Remove(this);
                _registered = false;
            }
        }

        private void Update()
        {
            if (HP == null || HP.IsDead) return;
            if (_attackCooldown > 0f) _attackCooldown -= Time.deltaTime;

            switch (_state)
            {
                case State.Idle:
                    _target = CombatUtil.FindNearestHero(transform.position, Data.aggroRadius);
                    if (_target != null) _state = State.Chase;
                    Anim.SetMoving(false);
                    break;

                case State.Chase:
                    if (_target == null || _target.HP == null || _target.HP.IsDead)
                    {
                        _state = State.Return;
                        break;
                    }
                    float distToTarget = Vector3.Distance(transform.position, _target.transform.position);
                    float distFromSpawn = Vector3.Distance(transform.position, _spawnPoint);

                    if (distFromSpawn > Data.aggroRadius * 1.8f)
                    {
                        _state = State.Return;
                        break;
                    }

                    if (distToTarget <= Stats.AttackRange)
                    {
                        _state = State.Attack;
                    }
                    else
                    {
                        Motor.Move((_target.transform.position - transform.position).normalized, Stats.MoveSpeed);
                        Anim.SetMoving(true);
                    }
                    break;

                case State.Attack:
                    if (_target == null || _target.HP == null || _target.HP.IsDead)
                    {
                        _state = State.Return;
                        break;
                    }
                    float d = Vector3.Distance(transform.position, _target.transform.position);
                    if (d > Stats.AttackRange * 1.25f)
                    {
                        _state = State.Chase;
                        break;
                    }
                    Anim.SetMoving(false);
                    Motor.FaceTowards(_target.transform.position);
                    if (_attackCooldown <= 0f) PerformAttack();
                    break;

                case State.Return:
                    float homeDist = Vector3.Distance(transform.position, _spawnPoint);
                    if (homeDist < 0.5f)
                    {
                        _state = State.Idle;
                        Motor.Stop();
                        Anim.SetMoving(false);
                    }
                    else
                    {
                        Motor.Move((_spawnPoint - transform.position).normalized, Stats.MoveSpeed);
                        Anim.SetMoving(true);
                    }
                    break;
            }
        }

        private void PerformAttack()
        {
            _attackCooldown = 1f / Mathf.Max(0.2f, Stats.AttackSpeed);
            Anim.PlayAttack(0.4f);
            CombatUtil.DealDamage(Stats, _target.Stats, Stats.Attack, Data.element, false, gameObject);
            VFXFactory.Spawn(Data.attackVfxKey, _target.transform.position + Vector3.up * 1.1f,
                Data.primaryColor, Data.isBoss ? 1.6f : 0.9f);
        }

        private void HandleDeath(Health h)
        {
            VFXFactory.Spawn("death", transform.position + Vector3.up * 0.8f, Data.primaryColor, 1.1f);
            Audio.AudioSystem.Play("Death", 0.7f);

            var team = Team.TeamManager.InstanceRef;
            if (team != null)
            {
                float exp = Data.expReward * (World.TowerSystem.CurrentExpScale > 0f ? World.TowerSystem.CurrentExpScale : 1f);
                team.ShareEXP(exp);
                DamagePopup.Spawn(transform.position, "+" + Mathf.RoundToInt(exp) + " EXP", new Color(1f, 0.85f, 0.3f));
            }

            if (MonsterDied != null) MonsterDied(this);
            StartCoroutine(DespawnAfterDelay());
        }

        private IEnumerator DespawnAfterDelay()
        {
            yield return new WaitForSeconds(1.6f);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (HP != null) HP.Died -= HandleDeath;
        }
    }
}
