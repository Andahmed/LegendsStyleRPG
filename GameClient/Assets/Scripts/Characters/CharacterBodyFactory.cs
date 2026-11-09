using UnityEngine;
using LegendsRPG.Combat;
using LegendsRPG.Vfx;

namespace LegendsRPG.Characters
{
    public struct BodyRefs
    {
        public Transform Torso;
        public Transform Head;
        public Transform ArmL;
        public Transform ArmR;
        public Transform LegL;
        public Transform LegR;
        public Transform WeaponTip;
        public Renderer MainRenderer;
    }

    /// <summary>
    /// Builds a stylized humanoid rig from Unity primitives at runtime.
    /// Colliders on the primitive parts are removed — collision is handled by
    /// the CharacterController on the root. This gives real 3D characters with
    /// real (procedural) animation while production FBX/GLB assets are
    /// introduced in later prototypes.
    /// </summary>
    public static class CharacterBodyFactory
    {
        public static BodyRefs Build(GameObject root, ElementType element, bool isHero,
            float scale, Color primary, Color secondary)
        {
            var refs = new BodyRefs();
            var bodyMat = MaterialLibrary.Solid(primary, 0.35f, 0.15f);
            var limbMat = MaterialLibrary.Solid(secondary, 0.3f, 0.1f);
            var headMat = MaterialLibrary.Solid(Color.Lerp(primary, Color.white, 0.35f), 0.4f, 0f);

            // ---- torso ----
            var torsoGo = CreatePart(root.transform, "Torso", PrimitiveType.Capsule, bodyMat,
                new Vector3(0f, 1.15f, 0f), new Vector3(0.62f, 0.55f, 0.42f) * scale);
            refs.Torso = torsoGo.transform;
            refs.MainRenderer = torsoGo.GetComponent<Renderer>();

            // ---- head ----
            var headGo = CreatePart(root.transform, "Head", PrimitiveType.Sphere, headMat,
                new Vector3(0f, 2.0f, 0f), Vector3.one * 0.34f * scale);
            refs.Head = headGo.transform;

            // eyes (gives immediate readability of facing direction)
            var eyeMat = MaterialLibrary.Emissive(new Color(0.9f, 0.95f, 1f), 2.2f);
            CreatePart(headGo, "EyeL", PrimitiveType.Sphere, eyeMat, new Vector3(-0.12f, 0.04f, 0.26f) * scale, Vector3.one * 0.055f * scale);
            CreatePart(headGo, "EyeR", PrimitiveType.Sphere, eyeMat, new Vector3(0.12f, 0.04f, 0.26f) * scale, Vector3.one * 0.055f * scale);

            // ---- arms (pivot at shoulder) ----
            refs.ArmL = BuildLimb(root.transform, "ArmL", limbMat, new Vector3(-0.48f, 1.72f, 0f) * scale,
                new Vector3(0f, -0.38f, 0f), new Vector3(0.17f, 0.62f, 0.17f) * scale);
            refs.ArmR = BuildLimb(root.transform, "ArmR", limbMat, new Vector3(0.48f, 1.72f, 0f) * scale,
                new Vector3(0f, -0.38f, 0f), new Vector3(0.17f, 0.62f, 0.17f) * scale);

            // ---- legs (pivot at hip) ----
            refs.LegL = BuildLimb(root.transform, "LegL", limbMat, new Vector3(-0.2f, 0.92f, 0f) * scale,
                new Vector3(0f, -0.44f, 0f), new Vector3(0.19f, 0.78f, 0.19f) * scale);
            refs.LegR = BuildLimb(root.transform, "LegR", limbMat, new Vector3(0.2f, 0.92f, 0f) * scale,
                new Vector3(0f, -0.44f, 0f), new Vector3(0.19f, 0.78f, 0.19f) * scale);

            // ---- weapon with element-glowing tip ----
            if (isHero)
            {
                var weaponGo = CreatePart(refs.ArmR.gameObject, "Weapon",
                    PrimitiveType.Cube, MaterialLibrary.Emissive(primary, 1.8f),
                    new Vector3(0f, -0.85f, 0.12f), new Vector3(0.07f, 1.0f, 0.07f) * scale);
                refs.WeaponTip = weaponGo.transform;
            }
            else
            {
                refs.WeaponTip = refs.ArmR;
            }

            // ---- boss horns ----
            if (!isHero && scale > 1.6f)
            {
                var hornMat = MaterialLibrary.Solid(new Color(0.25f, 0.22f, 0.2f), 0.2f, 0.3f);
                CreatePart(headGo, "HornL", PrimitiveType.Cylinder, hornMat,
                    new Vector3(-0.18f, 0.3f, 0f), new Vector3(0.07f, 0.22f, 0.07f) * scale);
                CreatePart(headGo, "HornR", PrimitiveType.Cylinder, hornMat,
                    new Vector3(0.18f, 0.3f, 0f), new Vector3(0.07f, 0.22f, 0.07f) * scale);
            }

            return refs;
        }

        private static Transform BuildLimb(Transform parent, string name, Material mat,
            Vector3 pivotPos, Vector3 offset, Vector3 scale)
        {
            var pivot = new GameObject(name);
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = pivotPos;

            CreatePart(pivot, name + "_Mesh", PrimitiveType.Cube, mat, offset, scale);
            return pivot.transform;
        }

        private static GameObject CreatePart(GameObject parent, string name, PrimitiveType type,
            Material mat, Vector3 localPos, Vector3 localScale)
        {
            return CreatePart(parent.transform, name, type, mat, localPos, localScale);
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType type,
            Material mat, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(type);
            // collision is owned by the CharacterController on the root
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;
            go.GetComponent<MeshRenderer>().material = mat;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return go;
        }
    }
}
