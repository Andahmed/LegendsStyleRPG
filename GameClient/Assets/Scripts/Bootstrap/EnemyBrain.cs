using System;
using UnityEngine;

namespace LegendsReborn.Rebuild
{
    public sealed class EnemyBrain : MonoBehaviour
    {
        public bool IsAlive { get; private set; } = true;

        private PlayerMotor target;
        private Action<EnemyBrain> defeated;
        private float health = 90f;
        private float nextAttackTime;
        private float moveSpeed = 1.9f;

        public void Initialize(PlayerMotor player, Action<EnemyBrain> onDefeated)
        {
            target = player;
            defeated = onDefeated;
        }

        private void Update()
        {
            if (!IsAlive || target == null) return;

            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;

            if (distance > 1.55f)
            {
                direction.Normalize();
                transform.position += direction * moveSpeed * Time.deltaTime;
                if (direction.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(direction), 8f * Time.deltaTime);
            }
            else if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + 1.35f;
                target.TakeDamage(6f);
            }
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;

            health -= amount;
            transform.localScale = Vector3.one * 1.04f;
            Invoke(nameof(ResetScale), 0.08f);

            if (health <= 0f)
            {
                IsAlive = false;
                if (defeated != null) defeated(this);
                Destroy(gameObject, 0.15f);
            }
        }

        private void ResetScale()
        {
            if (this != null) transform.localScale = Vector3.one;
        }
    }
}
