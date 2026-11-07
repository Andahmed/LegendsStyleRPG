using UnityEngine;
using LegendsRPG.Core;

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

        public void Move(Vector3 direction, float speed)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 1f) direction.Normalize();

            CurrentSpeed = direction.magnitude * speed;

            if (direction.sqrMagnitude > 0.0001f)
            {
                _facing = Vector3.RotateTowards(_facing, direction.normalized, 720f * Mathf.Deg2Rad * Time.deltaTime, 0f);
            }

            if (_cc.isGrounded) _verticalVelocity = -1f;
            else _verticalVelocity += Physics.gravity.y * 2f * Time.deltaTime;

            var motion = direction * speed * Time.deltaTime;
            motion.y = _verticalVelocity * Time.deltaTime;
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
            _cc.enabled = false;
            transform.position = position;
            _cc.enabled = true;
        }

        private void ClampToArena()
        {
            Vector3 p = transform.position;
            float r = new Vector2(p.x, p.z).magnitude;
            if (r > GameConfig.ArenaRadius - 1.2f)
            {
                Vector2 xz = new Vector2(p.x, p.z).normalized * (GameConfig.ArenaRadius - 1.2f);
                p.x = xz.x; p.z = xz.y;
                _cc.enabled = false;
                transform.position = p;
                _cc.enabled = true;
            }
        }
    }
}
