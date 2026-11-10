using UnityEngine;
using LegendsRPG.Core;
using LegendsRPG.World;

namespace LegendsRPG.Characters
{
    /// <summary>
    /// Movement using CharacterController with gravity, smooth facing and
    /// arena-bounds clamping. Used identically by heroes and monsters.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMotor : MonoBehaviour
    {
        private CharacterController _cc;
        private float _verticalVelocity;
        private Vector3 _facing = Vector3.forward;
        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_cc == null || !_cc.enabled) return;

            // Apply gravity even while the AI is idle; otherwise a character
            // teleported just above the floor can remain suspended in the arena.
            if (_cc.isGrounded) _verticalVelocity = -1f;
            else _verticalVelocity += Physics.gravity.y * 2f * Time.deltaTime;

            _cc.Move(Vector3.up * (_verticalVelocity * Time.deltaTime));
            ClampToArena();
        }

        public void Move(Vector3 direction, float speed)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 1f) direction.Normalize();

            CurrentSpeed = direction.magnitude * speed;

            if (direction.sqrMagnitude > 0.0001f)
            {
                _facing = Vector3.RotateTowards(_facing, direction.normalized, 720f * Mathf.Deg2Rad * Time.deltaTime, 0f);
            }

            // Horizontal locomotion is separate from the gravity step in Update().
            var motion = direction * speed * Time.deltaTime;
            _cc.Move(motion);

            ClampToArena();
            transform.rotation = Quaternion.LookRotation(_facing);
        }

        public void Stop()
        {
            CurrentSpeed = 0f;
        }

        public void FaceTowards(Vector3 worldPos)
        {
            Vector3 d = worldPos - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > 0.001f)
            {
                _facing = d.normalized;
                transform.rotation = Quaternion.LookRotation(_facing);
            }
        }

        public void Teleport(Vector3 position)
        {
            if (_cc == null) _cc = GetComponent<CharacterController>();
            if (_cc == null) return;

            _cc.enabled = false;
            transform.position = position;
            _verticalVelocity = 0f;
            CurrentSpeed = 0f;
            _cc.enabled = true;
        }

        private void ClampToArena()
        {
            // Only constrain movement during a battle. The legacy city/fallback lives at Z=0.
            // Clamp the character's offset from that center so movement cannot snap heroes
            // and monsters back toward (0,0,0) when they leave the arena radius.
            if (!BattleSession.Active) return;
            Vector3 center = BattleSession.ArenaCenter;
            Vector3 p = transform.position;
            Vector2 offset = new Vector2(p.x - center.x, p.z - center.z);
            float r = offset.magnitude;
            float maxRadius = GameConfig.ArenaRadius - 1.2f;
            if (r > maxRadius)
            {
                offset = offset.normalized * maxRadius;
                p.x = center.x + offset.x;
                p.z = center.z + offset.y;
                _cc.enabled = false;
                transform.position = p;
                _cc.enabled = true;
            }
        }
    }
}
