using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace LegendsRPG.World
{
    /// <summary>
    /// Replaces a procedural/FBX landmark fallback with its bundled GLB after
    /// asynchronous import succeeds. The home scene and camera remain synchronous.
    /// </summary>
    public sealed class DioramaGlbRuntimeLoader : MonoBehaviour
    {
        private string _modelId;
        private Transform _fallback;
        private float _targetHeight;
        private bool _configured;
        private readonly List<GltfImport> _liveImports = new List<GltfImport>();

        public void Configure(string modelId, Transform fallback, float targetHeight)
        {
            _modelId = modelId;
            _fallback = fallback;
            _targetHeight = Mathf.Max(0.1f, targetHeight);
            _configured = true;
        }

        private IEnumerator Start()
        {
            if (!_configured) yield break;

            var load = LoadAndReplaceAsync();
            while (!load.IsCompleted) yield return null;

            if (load.IsFaulted)
                Debug.LogWarning("[LegendsRPG] Diorama GLB '" + _modelId + "' failed; keeping fallback. " +
                    load.Exception.GetBaseException().Message, this);
            else if (!load.Result)
                Debug.LogWarning("[LegendsRPG] Diorama GLB '" + _modelId + "' unavailable or invalid; keeping fallback.", this);
        }

        private async Task<bool> LoadAndReplaceAsync()
        {
            var asset = Resources.Load<TextAsset>("Models/Diorama/diorama_" + _modelId);
            if (asset == null || asset.bytes == null || asset.bytes.Length < 20)
                return false;

            var importer = new GltfImport();
            GameObject modelRoot = null;
            try
            {
                bool loaded = await importer.LoadGltfBinary(asset.bytes);
                if (!loaded || this == null) return false;

                modelRoot = new GameObject("DioramaGLB_" + _modelId);
                modelRoot.transform.SetParent(transform, false);
                modelRoot.transform.localPosition = Vector3.zero;
                // These five supplied GLBs were exported with +Z as their up axis.
                // Convert them to Unity's +Y-up scene before measuring bounds or placing them.
                modelRoot.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                modelRoot.transform.localScale = Vector3.one;

                bool instantiated = await importer.InstantiateMainSceneAsync(modelRoot.transform);
                if (!instantiated || this == null)
                {
                    if (modelRoot != null) Destroy(modelRoot);
                    return false;
                }

                var renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    Destroy(modelRoot);
                    return false;
                }

                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                if (bounds.size.y < 0.0001f)
                {
                    Destroy(modelRoot);
                    return false;
                }

                modelRoot.transform.localScale = Vector3.one * (_targetHeight / bounds.size.y);

                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                // The supplied models already include their own stone/metal bases.
                // Ground those bases directly on the tabletop, then remove the temporary
                // wooden display pedestal together with the fallback model.
                const float groundOffset = -0.06f;
                modelRoot.transform.position += new Vector3(
                    transform.position.x - bounds.center.x,
                    transform.position.y + groundOffset - bounds.min.y,
                    transform.position.z - bounds.center.z);

                Debug.Log("[LegendsRPG] Diorama GLB loaded: " + _modelId +
                    " | renderers=" + renderers.Length +
                    " | height=" + bounds.size.y.ToString("F2"), this);

                if (_fallback != null)
                    Destroy(_fallback.gameObject);

                RemoveLegacyPedestal();

                // Keep the importer alive while its generated meshes/materials are in use.
                _liveImports.Add(importer);
                importer = null;
                return true;
            }
            catch
            {
                if (modelRoot != null) Destroy(modelRoot);
                throw;
            }
            finally
            {
                if (importer != null) importer.Dispose();
            }
        }

        private void RemoveLegacyPedestal()
        {
            string[] names = { "PedestalBase", "PedestalGoldTrim", "PedestalTop" };
            for (int i = 0; i < names.Length; i++)
            {
                var part = transform.Find(names[i]);
                if (part != null)
                    Destroy(part.gameObject);
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _liveImports.Count; i++)
                _liveImports[i]?.Dispose();
            _liveImports.Clear();
        }
    }
}
