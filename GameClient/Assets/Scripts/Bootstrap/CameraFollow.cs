using UnityEngine;

namespace LegendsReborn.Rebuild
{
    public sealed class CameraFollow : MonoBehaviour
    {
        public Transform Target;
        public Vector3 Offset = new Vector3(0f, 3.6f, -6.8f);
        public float SmoothTime = 0.12f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (Target == null) return;

            Vector3 desired = Target.position + Target.rotation * Offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, SmoothTime);
            transform.LookAt(Target.position + Vector3.up * 1.25f);
        }
    }
}
