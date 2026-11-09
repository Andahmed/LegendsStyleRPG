using System.Collections.Generic;
using UnityEngine;
using LegendsRPG.Combat;

namespace LegendsRPG.Vfx
{
    /// <summary>
    /// Central material/texture factory. Everything visual is generated
    /// procedurally at runtime — no external asset dependencies in Phase 0.1.
    /// </summary>
    public static class MaterialLibrary
    {
        private static Shader _standard;
        private static Shader _particle;
        private static Material _ground;
        private static Material _particleMat;
        private static readonly Dictionary<Color32, Material> _solidCache = new Dictionary<Color32, Material>();
        private static readonly Dictionary<Color32, Material> _emissiveCache = new Dictionary<Color32, Material>();

        public static Shader StandardShader
        {
            get
            {
                if (_standard == null) _standard = Shader.Find("Standard");
                return _standard;
            }
        }

        public static Shader ParticleShader
        {
            get
            {
                if (_particle == null)
                {
                    _particle = Shader.Find("Particles/Additive");
                    if (_particle == null) _particle = Shader.Find("Legacy Shaders/Particles/Additive");
                    if (_particle == null) _particle = Shader.Find("Sprites/Default");
                }
                return _particle;
            }
        }

        public static Material Solid(Color color, float smoothness = 0.25f, float metallic = 0.1f)
        {
            var key = (Color32)color;
            Material m;
            if (_solidCache.TryGetValue(key, out m) && m != null) return m;

            m = new Material(StandardShader);
            m.color = color;
            m.SetFloat("_Glossiness", smoothness);
            m.SetFloat("_Metallic", metallic);
            _solidCache[key] = m;
            return m;
        }

        public static Material Emissive(Color color, float intensity = 1.4f)
        {
            var key = (Color32)color;
            Material m;
            if (_emissiveCache.TryGetValue(key, out m) && m != null) return m;

            m = new Material(StandardShader);
            m.color = color * 0.35f;
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * intensity);
            m.SetFloat("_Glossiness", 0.5f);
            _emissiveCache[key] = m;
            return m;
        }

        public static Material ParticleMaterial
        {
            get
            {
                if (_particleMat == null)
                {
                    _particleMat = new Material(ParticleShader);
                    _particleMat.enableInstancing = true;
                }
                return _particleMat;
            }
        }

        public static Material GroundMaterial
        {
            get
            {
                if (_ground != null) return _ground;

                int size = 64;
                var tex = new Texture2D(size, size, TextureFormat.RGB24, false)
                {
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Point
                };
                var a = new Color(0.16f, 0.17f, 0.20f);
                var b = new Color(0.20f, 0.21f, 0.25f);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        bool checker = ((x / 8) + (y / 8)) % 2 == 0;
                        tex.SetPixel(x, y, checker ? a : b);
                    }
                }
                tex.Apply();

                _ground = new Material(StandardShader);
                _ground.mainTexture = tex;
                _ground.mainTextureScale = new Vector2(14f, 14f);
                _ground.SetFloat("_Glossiness", 0.08f);
                _ground.color = new Color(0.9f, 0.9f, 0.95f);
                return _ground;
            }
        }

        public static Color ElementColor(ElementType el)
        {
            return ElementalMatrix.GetElementColor(el);
        }
    }
}
