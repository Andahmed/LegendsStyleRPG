using UnityEngine;

namespace LegendsRPG.Characters
{
    /// <summary>
    /// Procedural animator for primitive-rig characters (Phase 0.1/0.2 visuals).
    /// Drives limb pivots directly: walk cycle, attack swing, cast raise,
    /// hit punch and a death fall. When production rigs (FBX/GLB) arrive in
    /// later prototypes, this class is replaced by Animator + clips behind
    /// the same PlayAttack/PlayCast/PlayDeath API.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        public Transform Torso;
        public Transform Head;
        public Transform ArmL;
        public Transform ArmR;
        public Transform LegL;
        public Transform LegR;
        public Renderer MainRenderer;

        private bool _moving;
        private float _walkPhase;
        private float _attackTimer, _attackDuration = 0.4f;
        private float _castTimer;
        private float _hitTimer;
        private bool _dead;
        private float _deathT;
        private Quaternion _torsoBaseRot;

        private void Start()
        {
            if (Torso != null) _torsoBaseRot = Torso.localRotation;
        }

        public void SetMoving(bool moving) { _moving = moving; }

        public void PlayAttack(float duration = 0.4f)
        {
            _attackTimer = duration;
            _attackDuration = duration;
        }

        public void PlayCast(float duration = 0.6f)
        {
            _castTimer = duration;
        }

        public void PlayHit()
        {
            _hitTimer = 0.18f;
        }

        public void PlayDeath()
        {
            _dead = true;
            _deathT = 0f;
        }

        public void ResetDeath()
        {
            _dead = false;
            _deathT = 0f;
            if (Torso != null) Torso.localRotation = _torsoBaseRot;
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_dead)
            {
                _deathT = Mathf.Min(1f, _deathT + dt * 2.2f);
                float angle = Mathf.SmoothStep(0f, 88f, _deathT);
                Vector3 e = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(angle, e.y, 0f);
                return;
            }

            // ---- walk cycle ----
            if (_moving)
            {
                _walkPhase += dt * 9f;
                float swing = Mathf.Sin(_walkPhase) * 38f;
                if (LegL != null) LegL.localRotation = Quaternion.Euler(swing, 0f, 0f);
                if (LegR != null) LegR.localRotation = Quaternion.Euler(-swing, 0f, 0f);
                if (_attackTimer <= 0f && _castTimer <= 0f)
                {
                    if (ArmL != null) ArmL.localRotation = Quaternion.Euler(-swing * 0.7f, 0f, 0f);
                    if (ArmR != null) ArmR.localRotation = Quaternion.Euler(swing * 0.7f, 0f, 0f);
                }
                if (Torso != null)
                {
                    Torso.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(_walkPhase)) * 0.06f, 0f);
                }
            }
            else
            {
                _walkPhase = 0f;
                if (LegL != null) LegL.localRotation = Quaternion.Slerp(LegL.localRotation, Quaternion.identity, dt * 10f);
                if (LegR != null) LegR.localRotation = Quaternion.Slerp(LegR.localRotation, Quaternion.identity, dt * 10f);
            }

            // ---- attack swing (right arm arc) ----
            if (_attackTimer > 0f)
            {
                _attackTimer -= dt;
                float t = 1f - Mathf.Clamp01(_attackTimer / Mathf.Max(0.01f, _attackDuration));
                float arc = Mathf.Sin(t * Mathf.PI) * -130f;
                if (ArmR != null) ArmR.localRotation = Quaternion.Euler(arc, 0f, 0f);
                if (ArmL != null) ArmL.localRotation = Quaternion.Euler(-arc * 0.25f, 0f, 0f);
            }
            else if (!_moving)
            {
                if (ArmR != null) ArmR.localRotation = Quaternion.Slerp(ArmR.localRotation, Quaternion.identity, dt * 10f);
                if (ArmL != null) ArmL.localRotation = Quaternion.Slerp(ArmL.localRotation, Quaternion.identity, dt * 10f);
            }

            // ---- cast (both arms raised) ----
            if (_castTimer > 0f)
            {
                _castTimer -= dt;
                float t = Mathf.Clamp01(_castTimer / 0.6f);
                float raise = Mathf.Sin(t * Mathf.PI) * -150f;
                if (ArmR != null) ArmR.localRotation = Quaternion.Euler(raise, 0f, 0f);
                if (ArmL != null) ArmL.localRotation = Quaternion.Euler(raise, 0f, 0f);
            }

            // ---- hit punch ----
            if (_hitTimer > 0f)
            {
                _hitTimer -= dt;
                if (Torso != null)
                {
                    Torso.localRotation = _torsoBaseRot * Quaternion.Euler(0f, Mathf.Sin(_hitTimer * 40f) * 12f, 0f);
                }
            }
            else if (Torso != null)
            {
                Torso.localRotation = Quaternion.Slerp(Torso.localRotation, _torsoBaseRot, dt * 12f);
            }
        }
    }
}
