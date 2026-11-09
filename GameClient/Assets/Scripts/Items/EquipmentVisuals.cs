using UnityEngine;
using LegendsRPG.Vfx;

namespace LegendsRPG.Items
{
    /// <summary>
    /// Procedural flapping wings attached to a hero's back.
    /// Two pivots at shoulder height carry flattened panels that
    /// oscillate around the Z axis. Pure runtime construction.
    /// </summary>
    public class WingVisual : MonoBehaviour
    {
        private Transform _pivotL;
        private Transform _pivotR;
        private float _flapSpeed = 7f;
        private float _flapDegrees = 24f;
        private float _phase;

        public void Build(Color color)
        {
            _phase = Random.value * Mathf.PI * 2f;

            var mat = MaterialLibrary.Emissive(color, 1.1f);

            _pivotL = MakeWingPivot("WingPivotL", new Vector3(-0.34f, 1.55f, -0.22f), -1f, mat);
            _pivotR = MakeWingPivot("WingPivotR", new Vector3(0.34f, 1.55f, -0.22f), 1f, mat);
        }

        private Transform MakeWingPivot(string name, Vector3 localPos, float side, Material mat)
        {
            var pivotGo = new GameObject(name);
            pivotGo.transform.SetParent(transform, false);
            pivotGo.transform.localPosition = localPos;
            pivotGo.transform.localScale = Vector3.one;

            // main panel
            var panel = CreatePanel(pivotGo, mat, new Vector3(side * 0.55f, 0.12f, -0.05f),
                new Vector3(0.95f, 0.42f, 0.03f), side * 18f);

            // tip panel (smaller, angled further out)
            CreatePanel(panel, mat, new Vector3(side * 0.5f, 0.18f, 0f),
                new Vector3(0.55f, 0.28f, 0.03f), side * 22f);

            return pivotGo.transform;
        }

        private GameObject CreatePanel(GameObject parent, Material mat, Vector3 localPos,
            Vector3 scale, float zTilt)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);

            go.name = "WingPanel";
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, zTilt);
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().material = mat;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }

        private void Update()
        {
            if (_pivotL == null || _pivotR == null) return;
            float flap = Mathf.Sin(Time.time * _flapSpeed + _phase) * _flapDegrees;
            _pivotL.localRotation = Quaternion.Euler(0f, 0f, flap);
            _pivotR.localRotation = Quaternion.Euler(0f, 0f, -flap);
        }
    }

    /// <summary>
    /// Equipped pet: a small glowing orb that orbits its owner and bobs.
    /// Visible proof that the pet slot is doing something real.
    /// </summary>
    public class PetOrb : MonoBehaviour
    {
        private Transform _orb;
        private float _radius = 1.1f;
        private float _height = 1.2f;
        private float _speed = 2.2f;
        private float _phase;

        public void Build(Color color)
        {
            _phase = Random.value * Mathf.PI * 2f;

            _orb = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            var col = _orb.GetComponent<Collider>();
            if (col != null) Destroy(col);

            _orb.name = "PetOrb";
            _orb.SetParent(transform, false);
            _orb.localScale = Vector3.one * 0.26f;
            _orb.GetComponent<MeshRenderer>().material = MaterialLibrary.Emissive(color, 2.0f);
        }

        private void Update()
        {
            if (_orb == null) return;
            float t = Time.time * _speed + _phase;
            float bob = Mathf.Sin(Time.time * 3.1f + _phase) * 0.12f;
            _orb.localPosition = new Vector3(
                Mathf.Cos(t) * _radius,
                _height + bob,
                Mathf.Sin(t) * _radius);
        }
    }
}
