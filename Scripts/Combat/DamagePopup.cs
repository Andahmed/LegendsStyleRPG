using System.Collections;
using UnityEngine;
using LegendsRPG.Core;

namespace LegendsRPG.Combat
{
    /// <summary>
    /// Floating combat text (damage / crit / heal / XP) as pooled billboard TextMesh.
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        private TextMesh _text;
        private float _life;
        private const float Duration = 0.9f;
        private Vector3 _velocity;

        public static void Spawn(Vector3 worldPos, string message, Color color, bool big = false)
        {
            if (ObjectPool.Instance == null) return;
            var go = ObjectPool.Instance.Get("dmg_popup", () => BuildPopup());
            var p = go.GetComponent<DamagePopup>();
            p.transform.position = worldPos + new Vector3(Random.Range(-0.25f, 0.25f), 2.3f, 0f);
            p.transform.localScale = Vector3.one * (big ? 0.22f : 0.15f);
            p.Activate(message, color);
        }

        private static GameObject BuildPopup()
        {
            var go = new GameObject("DamagePopup");
            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = 64;
            tm.characterSize = 0.1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var mr = go.GetComponent<MeshRenderer>();
            // 3D text requires the font material
            mr.sharedMaterial = tm.font.material;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            go.AddComponent<DamagePopup>();
            return go;
        }

        private void Activate(string message, Color color)
        {
            if (_text == null) _text = GetComponent<TextMesh>();
            _text.text = message;
            _text.color = color;
            _life = Duration;
            _velocity = Vector3.up * 1.8f;
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            float t = 0f;
            Color c = _text.color;
            while (t < Duration)
            {
                t += Time.deltaTime;
                _text.color = new Color(c.r, c.g, c.b, 1f - t / Duration);
                transform.position += _velocity * Time.deltaTime;
                _velocity *= 0.92f;
                FaceCamera();
                yield return null;
            }
            _text.color = c;
            if (ObjectPool.Instance != null) ObjectPool.Instance.Release("dmg_popup", gameObject);
            else Destroy(gameObject);
        }

        private void FaceCamera()
        {
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _life = 0f;
        }
    }
}
