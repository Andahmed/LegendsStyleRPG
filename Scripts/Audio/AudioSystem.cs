using System.Collections.Generic;
using UnityEngine;

namespace LegendsRPG.Audio
{
    /// <summary>
    /// Runtime-synthesized audio (no external asset files in the prototype):
    /// SFX (hit, skills, UI, level-up, victory, boss) + a looping ambient BGM.
    /// Plays 2D for UI and positional 3D for combat feedback.
    /// </summary>
    public class AudioSystem : MonoBehaviour
    {
        private static AudioSystem _instance;
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private AudioSource _uiSource;
        private AudioSource _musicSource;
        private const int SampleRate = 22050;

        public static void Play(string key, float volume = 1f, Vector3? position = null)
        {
            if (_instance == null) return;
            _instance.PlayInternal(key, volume, position);
        }

        public static void SetMusicVolume(float v)
        {
            if (_instance != null && _instance._musicSource != null)
            {
                _instance._musicSource.volume = Mathf.Clamp01(v);
            }
        }

        public static void SetSfxVolume(float v)
        {
            if (_instance != null && _instance._uiSource != null)
            {
                _instance._uiSource.volume = Mathf.Clamp01(v);
            }
        }

        public static float SfxVolume()
        {
            return _instance != null && _instance._uiSource != null ? _instance._uiSource.volume : 0.9f;
        }

        public static float MusicVolume()
        {
            return _instance != null && _instance._musicSource != null ? _instance._musicSource.volume : 0.6f;
        }

        private void Awake()
        {
            _instance = this;

            _uiSource = gameObject.AddComponent<AudioSource>();
            _uiSource.playOnAwake = false;
            _uiSource.spatialBlend = 0f;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = 0.5f;

            BuildClips();

            if (_clips.ContainsKey("BGM"))
            {
                _musicSource.clip = _clips["BGM"];
                _musicSource.Play();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void PlayInternal(string key, float volume, Vector3? position)
        {
            AudioClip clip;
            if (!_clips.TryGetValue(key, out clip) || clip == null) return;

            if (position.HasValue)
            {
                var go = new GameObject("Sfx_" + key);
                go.transform.position = position.Value;
                var src = go.AddComponent<AudioSource>();
                src.clip = clip;
                src.volume = Mathf.Clamp01(volume);
                src.spatialBlend = 1f;
                src.rolloffMode = AudioRolloffMode.Linear;
                src.minDistance = 4f;
                src.maxDistance = 40f;
                src.Play();
                Destroy(go, clip.length + 0.2f);
            }
            else
            {
                _uiSource.PlayOneShot(clip, Mathf.Clamp01(volume));
            }
        }

        // ==================== synthesis ====================
        private void BuildClips()
        {
            _clips["Hit"] = Synth(0.14f, t =>
            {
                float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 30f);
                float thump = Mathf.Sin(2f * Mathf.PI * (160f - 500f * t) * t) * Mathf.Exp(-t * 22f);
                return (noise * 0.5f + thump * 0.9f) * 0.55f;
            });

            _clips["Skill"] = Synth(0.3f, t =>
            {
                float sweep = Mathf.Sin(2f * Mathf.PI * (420f + 900f * t / 0.3f) * t);
                float env = Mathf.Exp(-t * 8f) * Mathf.Min(1f, t * 40f);
                return sweep * env * 0.4f;
            });

            _clips["Ult"] = Synth(0.75f, t =>
            {
                float deep = Mathf.Sin(2f * Mathf.PI * (90f + 240f * t) * t);
                float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 4.5f) * 0.5f;
                float env = Mathf.Min(1f, t * 25f) * Mathf.Exp(-t * 4f);
                return (deep * 0.7f + noise * 0.5f) * env * 0.55f;
            });

            _clips["UI"] = Synth(0.06f, t => Mathf.Sin(2f * Mathf.PI * 1250f * t) * Mathf.Exp(-t * 55f) * 0.3f);

            _clips["LevelUp"] = Synth(0.55f, t =>
            {
                float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f };
                int idx = Mathf.Clamp((int)(t / 0.14f), 0, 3);
                float local = t - idx * 0.14f;
                float env = Mathf.Exp(-local * 9f);
                return Mathf.Sin(2f * Mathf.PI * notes[idx] * local) * env * 0.3f;
            });

            _clips["Victory"] = Synth(1.1f, t =>
            {
                float[] chord = { 523.25f, 659.25f, 783.99f };
                float sum = 0f;
                for (int i = 0; i < chord.Length; i++)
                {
                    float local = t - i * 0.12f;
                    if (local >= 0f) sum += Mathf.Sin(2f * Mathf.PI * chord[i] * local) * Mathf.Exp(-local * 2.6f);
                }
                return sum * 0.2f;
            });

            _clips["Death"] = Synth(0.5f, t =>
            {
                float sweep = Mathf.Sin(2f * Mathf.PI * (300f - 420f * t / 0.5f) * t);
                return sweep * Mathf.Exp(-t * 6f) * 0.35f;
            });

            _clips["BossRoar"] = Synth(0.9f, t =>
            {
                float growl = Mathf.Sin(2f * Mathf.PI * (70f + 30f * Mathf.Sin(t * 9f)) * t);
                float noise = (Random.value * 2f - 1f) * 0.25f;
                float env = Mathf.Min(1f, t * 14f) * Mathf.Exp(-t * 3.2f);
                return (growl * 0.8f + noise * 0.4f) * env * 0.6f;
            });

            _clips["BGM"] = BuildBgm(9.6f);

            // ---- v1.0 meta sounds ----
            _clips["Coin"] = Synth(0.35f, t =>
            {
                float a = Mathf.Sin(2f * Mathf.PI * 1568f * t) * Mathf.Exp(-t * 14f);
                float b = Mathf.Sin(2f * Mathf.PI * 2093f * t) * Mathf.Exp(-(t - 0.08f) * 14f) * (t > 0.08f ? 1f : 0f);
                return (a * 0.5f + b * 0.4f);
            });

            _clips["Fanfare"] = Synth(1.0f, t =>
            {
                float[] notes = { 523.25f, 659.25f, 783.99f, 1046.5f, 1318.5f };
                int idx = Mathf.Clamp((int)(t / 0.2f), 0, 4);
                float local = t - idx * 0.2f;
                float env = Mathf.Exp(-local * 6f) * Mathf.Min(1f, local * 60f);
                float wave = Mathf.Sin(2f * Mathf.PI * notes[idx] * local)
                           + 0.4f * Mathf.Sin(2f * Mathf.PI * notes[idx] * 2f * local);
                return wave * env * 0.22f;
            });

            _clips["Equip"] = Synth(0.18f, t =>
            {
                float clink = Mathf.Sin(2f * Mathf.PI * 2400f * t) * Mathf.Exp(-t * 40f)
                            + Mathf.Sin(2f * Mathf.PI * 3170f * t) * Mathf.Exp(-t * 55f) * 0.6f;
                return clink * 0.3f;
            });

            _clips["Error"] = Synth(0.22f, t =>
            {
                float buzz = Mathf.Sin(2f * Mathf.PI * 190f * t) * (0.6f + 0.4f * Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 28f * t)));
                return buzz * Mathf.Exp(-t * 10f) * 0.25f;
            });

            _clips["Open"] = Synth(0.24f, t =>
            {
                float sweep = Mathf.Sin(2f * Mathf.PI * (500f + 1400f * t / 0.24f) * t);
                return sweep * Mathf.Exp(-t * 12f) * 0.22f;
            });

            _clips["Summon"] = Synth(1.4f, t =>
            {
                float shimmer = Mathf.Sin(2f * Mathf.PI * (880f + 660f * Mathf.Sin(t * 5f)) * t);
                float env = Mathf.Min(1f, t * 6f) * Mathf.Exp(-t * 2.2f);
                float sparkle = Mathf.Sin(2f * Mathf.PI * 2637f * t) * Mathf.Exp(-((t * 3f) % 1f) * 8f) * 0.3f;
                return (shimmer * 0.4f + sparkle) * env * 0.4f;
            });

            _clips["Chest"] = Synth(0.6f, t =>
            {
                float creak = Mathf.Sin(2f * Mathf.PI * (120f + 340f * t) * t) * Mathf.Exp(-t * 4f);
                float ding = Mathf.Sin(2f * Mathf.PI * 1568f * (t - 0.35f)) * Mathf.Exp(-(t - 0.35f) * 12f) * (t > 0.35f ? 0.8f : 0f);
                return creak * 0.3f + ding * 0.35f;
            });
        }

        private delegate float SynthFunc(float t);

        private static AudioClip Synth(float duration, SynthFunc f)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                data[i] = Mathf.Clamp(f(i / (float)SampleRate), -1f, 1f);
            }
            var clip = AudioClip.Create("synth_" + duration, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildBgm(float duration)
        {
            int samples = Mathf.CeilToInt(duration * SampleRate);
            var data = new float[samples];

            // slow minor-key pad: A2 C3 E3 + gentle 5th, loop-safe envelope
            float[] freqs = { 110f, 130.81f, 164.81f, 220f };
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float value = 0f;
                for (int k = 0; k < freqs.Length; k++)
                {
                    value += Mathf.Sin(2f * Mathf.PI * freqs[k] * t + k * 1.7f) / (k + 2f);
                }
                float lfo = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 0.15f * t);
                // loop click removal: crossfade tail into head
                float loopEnv = 1f;
                float tail = duration - t;
                if (tail < 0.5f) loopEnv = tail / 0.5f;
                if (t < 0.5f) loopEnv = Mathf.Min(loopEnv, t / 0.5f);
                data[i] = Mathf.Clamp(value * 0.16f * lfo * loopEnv, -1f, 1f);
            }

            var clip = AudioClip.Create("bgm", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
