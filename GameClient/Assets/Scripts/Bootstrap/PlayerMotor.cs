using System.Collections;
using UnityEngine;

namespace LegendsReborn.Rebuild
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        public float MaxHealth { get; private set; } = 100f;
        public float Health { get; private set; } = 100f;
        public float MaxMana { get; private set; } = 100f;
        public float Mana { get; private set; } = 100f;

        private CharacterController controller;
        private Transform cameraTransform;
        private MobileJoystick joystick;
        private Vector3 lastMoveDirection = Vector3.forward;
        private float moveSpeed = 5.2f;
        private bool dashing;

        public void Initialize(Transform cameraReference)
        {
            controller = GetComponent<CharacterController>();
            cameraTransform = cameraReference;
        }

        public void SetJoystick(MobileJoystick value)
        {
            joystick = value;
        }

        private void Update()
        {
            if (controller == null) controller = GetComponent<CharacterController>();

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (joystick != null && joystick.Direction.sqrMagnitude > 0.02f)
                input = joystick.Direction;

            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            Vector3 movement = forward * input.y + right * input.x;

            if (movement.sqrMagnitude > 0.001f)
            {
                movement.Normalize();
                lastMoveDirection = movement;
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(movement), 14f * Time.deltaTime);
            }

            if (!dashing && controller != null)
                controller.SimpleMove(movement * moveSpeed);

            if (Input.GetKeyDown(KeyCode.Space) && LegendsBootstrap.Instance != null)
                LegendsBootstrap.Instance.SendMessage("BasicAttack", SendMessageOptions.DontRequireReceiver);
        }

        public void TakeDamage(float amount)
        {
            Health = Mathf.Max(0f, Health - amount);
            if (Health <= 0f)
            {
                Health = MaxHealth;
                transform.position = new Vector3(0f, 0.05f, 0f);
            }
        }

        public bool SpendMana(float amount)
        {
            if (Mana < amount) return false;
            Mana -= amount;
            return true;
        }

        public void Dash()
        {
            if (!dashing && gameObject.activeInHierarchy)
                StartCoroutine(DashRoutine());
        }

        private IEnumerator DashRoutine()
        {
            dashing = true;
            float elapsed = 0f;
            while (elapsed < 0.22f)
            {
                if (controller != null)
                    controller.Move(lastMoveDirection * 11f * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            dashing = false;
        }
    }
}
