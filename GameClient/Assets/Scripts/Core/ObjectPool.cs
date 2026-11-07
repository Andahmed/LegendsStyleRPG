using System;
using System.Collections.Generic;
using UnityEngine;

namespace LegendsRPG.Core
{
    /// <summary>
    /// Generic pooled objects (projectiles, VFX bursts, damage popups).
    /// Avoids runtime Instantiate/Destroy churn on mobile — an explicit
    /// performance requirement of this project.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private Transform _root;

        private void Awake()
        {
            _root = new GameObject("PoolRoot").transform;
            _root.SetParent(transform, false);
        }

        public static ObjectPool Instance { get; private set; }

        private void OnEnable() { Instance = this; }
        private void OnDisable() { if (Instance == this) Instance = null; }

        /// <summary>Fetch an instance. The prefab may be created lazily from a factory delegate.</summary>
        public GameObject Get(string key, Func<GameObject> factory)
        {
            Queue<GameObject> q;
            if (!_pools.TryGetValue(key, out q))
            {
                q = new Queue<GameObject>();
                _pools[key] = q;
            }

            GameObject go = null;
            while (q.Count > 0)
            {
                var candidate = q.Dequeue();
                if (candidate != null) { go = candidate; break; }
            }

            if (go == null)
            {
                go = factory != null ? factory() : new GameObject(key);
                go.transform.SetParent(_root, false);
                go.name = key;
            }

            go.SetActive(true);
            return go;
        }

        public void Release(string key, GameObject go, float delay = 0f)
        {
            if (go == null) return;
            if (delay <= 0f)
            {
                Push(key, go);
            }
            else
            {
                StartCoroutine(ReleaseAfter(key, go, delay));
            }
        }

        private System.Collections.IEnumerator ReleaseAfter(string key, GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            Push(key, go);
        }

        private void Push(string key, GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            Queue<GameObject> q;
            if (!_pools.TryGetValue(key, out q))
            {
                q = new Queue<GameObject>();
                _pools[key] = q;
            }
            go.transform.SetParent(_root, false);
            q.Enqueue(go);
        }
    }
}
