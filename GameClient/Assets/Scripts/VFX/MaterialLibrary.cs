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

                // Stone-tile floor: soft noise, darker grout lines, per-tile tint variation.
                int size = 256, tile = 32;
                var tex = new Texture2D(size, size, TextureFormat.RGB24, true)
                {
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Trilinear,
                    anisoLevel = 4
                };
                var baseA = new Color(0.30f, 0.30f, 0.36f);
                var grout = new Color(0.12f, 0.12f, 0.16f);
                for (int ty = 0; ty < size / tile; ty++)
                {
                    for (int tx = 0; tx < size / tile; tx++)
                    {
                        float tint = 0.85f + 0.3f * Mathf.PerlinNoise(tx * 3.7f + 11f, ty * 3.7f + 5f);
                        for (int y = 0; y < tile; y++)
                        {
                            for (int x = 0; x < tile; x++)
                            {
                                int px = tx * tile + x, py = ty * tile + y;
                                float n = Mathf.PerlinNoise(px * 0.09f, py * 0.09f) * 0.18f
                                        + Mathf.PerlinNoise(px * 0.5f, py * 0.5f) * 0.05f;
                                Color c = baseA * (tint + n);
                                int edge = Mathf.Min(Mathf.Min(x, tile - 1 - x), Mathf.Min(y, tile - 1 - y));
                                if (edge < 2) c = Color.Lerp(grout, c, edge * 0.5f);
                                c.a = 1f;
                                tex.SetPixel(px, py, c);
                            }
                        }
                    }
                }
                tex.Apply(true);

                _ground = new Material(StandardShader);
                _ground.mainTexture = tex;
                _ground.mainTextureScale = new Vector2(10f, 10f);
                _ground.SetFloat("_Glossiness", 0.18f);
                _ground.SetFloat("_Metallic", 0f);
                _ground.color = Color.white;
                return _ground;
            }
        }

        /// <summary>Soft round glow texture used for the hero ground rings and blob shadows.</summary>
        private static Texture2D _ringTex;
        private static Material _ringMat;
        public static Material RingMaterial(Color color)
        {
            if (_ringTex == null)
            {
                int n = 128;
                _ringTex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                        float ring = Mathf.Clamp01(1f - Mathf.Abs(d - 0.82f) * 14f);
                        float fill = d < 0.82f ? 0.18f * (1f - d) : 0f;
                        float a = Mathf.Clamp01(ring + fill);
                        _ringTex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                _ringTex.Apply();
            }
            var sh = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            var m = new Material(sh);
            m.mainTexture = _ringTex;
            m.color = color;
            return m;
        }

        public static Color ElementColor(ElementType el)
        {
            return ElementalMatrix.GetElementColor(el);
        }
    }
}
