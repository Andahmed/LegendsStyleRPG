using System.Collections.Generic;
using UnityEngine;

namespace LegendsRPG.Core
{
    public class ErrorOverlay : MonoBehaviour
    {
        private readonly List<string> _lines = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("ErrorOverlay");
            DontDestroyOnLoad(go);
            go.AddComponent<ErrorOverlay>();
        }

        private void OnEnable() { Application.logMessageReceived += OnLog; }
        private void OnDisable() { Application.logMessageReceived -= OnLog; }

        private void OnLog(string msg, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            string s = stack.Length > 300 ? stack.Substring(0, 300) : stack;
            _lines.Add(type + ": " + msg + "\n" + s);
            if (_lines.Count > 4) _lines.RemoveAt(0);
        }

        private void OnGUI()
        {
            if (_lines.Count == 0) return;
            var style = new GUIStyle(GUI.skin.box);
            style.fontSize = Mathf.Max(18, Screen.width / 40);
            style.wordWrap = true;
            style.alignment = TextAnchor.UpperLeft;
            style.normal.textColor = Color.red;
            GUI.Box(new Rect(10, 10, Screen.width - 20, Screen.height * 0.6f), string.Join("\n\n", _lines), style);
        }
    }
}
