using UnityEngine;
using LegendsRPG.Art;
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
        public Transform Ring;
    }

    /// <summary>
    /// Builds a stylized humanoid rig from Unity primitives at runtime.
    /// Colliders on the primitive parts are removed — collision is handled by
    /// the CharacterController on the root. When the Art Kit has a prefab
    /// assigned for the requested slot (Assets/Resources/ArtSlotMap.asset),
    /// the real model is instantiated instead and the primitive rig is skipped.
    /// </summary>
    public static class CharacterBodyFactory
    {
        public static BodyRefs Build(GameObject root, ElementType element, bool isHero,
            float scale, Color primary, Color secondary, string artSlot = null)
        {
            var refs = new BodyRefs();

            // ---- real art first (v0.4 Art Kit); primitives are the fallback ----
            if (!string.IsNullOrEmpty(artSlot))
            {
                var model = ArtLibrary.Instantiate(artSlot, root.transform, 2.0f * scale);
                if (model != null)
                {
                    refs.Torso = model.transform;   // animator bobs/punches the model root
                    refs.Head = model.transform;
                    refs.MainRenderer = model.GetComponentInChildren<Renderer>();

                    var anchorGo = new GameObject("VfxAnchor");
                    anchorGo.transform.SetParent(model.transform, false);
                    anchorGo.transform.localPosition = new Vector3(0f, 0.55f, 0.35f);
                    refs.WeaponTip = anchorGo.transform;

                    refs.Ring = BuildRing(root.transform, isHero, primary, scale);
                    return refs;
                }
            }

            var bodyMat = MaterialLibrary.Solid(primary, 0.35f, 0.15f);
            // limbs were nearly the same grey as the rocks/pillars -> blend in the hero colour so units read clearly
            var limbMat = MaterialLibrary.Solid(Color.Lerp(secondary, primary, 0.55f), 0.3f, 0.1f);
            var trimMat = MaterialLibrary.Solid(Color.Lerp(secondary, Color.black, 0.35f), 0.4f, 0.3f);
            var headMat = MaterialLibrary.Solid(Color.Lerp(primary, Color.white, 0.35f), 0.4f, 0f);

            // ---- torso ----
            var torsoGo = CreatePart(root.transform, "Torso", PrimitiveType.Capsule, bodyMat,
                new Vector3(0f, 1.15f * scale, 0f), new Vector3(0.62f, 0.62f, 0.46f) * scale);
            refs.Torso = torsoGo.transform;
            refs.MainRenderer = torsoGo.GetComponent<Renderer>();

            // ---- head ----
            var headGo = CreatePart(root.transform, "Head", PrimitiveType.Sphere, headMat,
                new Vector3(0f, 1.98f * scale, 0f), Vector3.one * 0.40f * scale);
            refs.Head = headGo.transform;

            // eyes (gives immediate readability of facing direction)
            var eyeMat = MaterialLibrary.Emissive(new Color(0.9f, 0.95f, 1f), 2.2f);
            // (eye positions are in the head's local space: sphere radius = 0.5, so z must be ~0.45 to sit on the surface)
            CreatePart(headGo, "EyeL", PrimitiveType.Sphere, eyeMat, new Vector3(-0.17f, 0.06f, 0.43f), Vector3.one * 0.16f);
            CreatePart(headGo, "EyeR", PrimitiveType.Sphere, eyeMat, new Vector3(0.17f, 0.06f, 0.43f), Vector3.one * 0.16f);

            // belt + chest gem in the element colour (instant visual identity per hero)
            CreatePart(torsoGo, "Belt", PrimitiveType.Cylinder, trimMat,
                new Vector3(0f, -0.28f, 0f), new Vector3(1.04f, 0.07f, 1.04f));
            CreatePart(torsoGo, "Gem", PrimitiveType.Sphere, MaterialLibrary.Emissive(Color.Lerp(primary, Color.white, 0.5f), 2f),
                new Vector3(0f, 0.12f, 0.48f), Vector3.one * 0.2f);

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

            // ground ring: team colour marker so units are readable on the floor
            refs.Ring = BuildRing(root.transform, isHero, primary, scale);

            // ---- weapon with element-glowing tip ----
            if (isHero)
            {
                var weaponGo = CreatePart(refs.ArmR.gameObject, "Weapon",
                    PrimitiveType.Cube, MaterialLibrary.Emissive(primary, 1.8f),
                    new Vector3(0f, -0.85f, 0.12f), new Vector3(0.09f, 1.0f, 0.09f) * scale);
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

        private static Transform BuildRing(Transform root, bool isHero, Color primary, float scale)
        {
            var ringGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var rc = ringGo.GetComponent<Collider>(); if (rc != null) Object.Destroy(rc);
            ringGo.name = "GroundRing";
            ringGo.transform.SetParent(root.transform, false);
            ringGo.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            ringGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ringGo.transform.localScale = Vector3.one * (isHero ? 1.9f : 1.5f * scale);
            var rr = ringGo.GetComponent<MeshRenderer>();
            rr.material = MaterialLibrary.RingMaterial(isHero ? new Color(primary.r, primary.g, primary.b, 0.75f) : new Color(1f, 0.25f, 0.2f, 0.55f));
            rr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rr.receiveShadows = false;
            return ringGo.transform;
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
