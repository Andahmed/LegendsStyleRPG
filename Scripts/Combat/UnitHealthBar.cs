using UnityEngine;
using LegendsRPG.Characters;

namespace LegendsRPG.Combat
{
    /// <summary>
    /// v1.1: small billboarded HP bar above every unit (heroes and monsters),
    /// built from two SpriteRenderers — zero scene setup, zero art files.
    /// The fill sprite has a left-edge pivot so scaling X grows the bar from
    /// the left, and the whole bar rotates to face the camera every frame.
    /// </summary>
    public class UnitHealthBar : MonoBehaviour
    {
        private static Sprite _white;

        private Health _hp;
        private Transform _bar;
        private Transform _fill;
        private SpriteRenderer _fillRenderer;
        private float _width;
        private float _height;
        private bool _isHero;

        /// <summary>Shared 1x1 white sprite with a LEFT-EDGE pivot (0, 0.5).</summary>
        public static Sprite WhiteSprite
        {
            get
            {
                if (_white == null)
                {
                    var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                    for (int y = 0; y < 4; y++)
                        for (int x = 0; x < 4; x++)
                            tex.SetPixel(x, y, Color.white);
                    tex.Apply();
                    _white = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0f, 0.5f), 4f);
                }
                return _white;
            }
        }

        public void Bind(Health hp, float width, float height, float yOffset, bool isHero)
        {
            _hp = hp;
            _width = width;
            _height = height;
            _isHero = isHero;

            var go = new GameObject("HPBar");
            _bar = go.transform;
            _bar.SetParent(transform, false);
            _bar.localPosition = new Vector3(0f, yOffset, 0f);

            var bgGo = new GameObject("Bg");
            var bg = bgGo.AddComponent<SpriteRenderer>();
            bg.sprite = WhiteSprite;
            bg.color = new Color(0f, 0f, 0f, 0.65f);
            bg.sortingOrder = 20;
            bgGo.transform.SetParent(_bar, false);
            bgGo.transform.localPosition = new Vector3(-_width * 0.5f, 0f, 0f);
            bgGo.transform.localScale = new Vector3(_width, _height, 1f);

            var fillGo = new GameObject("Fill");
            _fill = fillGo.transform;
            _fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            _fillRenderer.sprite = WhiteSprite;
            _fillRenderer.sortingOrder = 21;
            fillGo.transform.SetParent(_bar, false);
            fillGo.transform.localPosition = new Vector3(-_width * 0.5f, 0f, 0f);
            fillGo.transform.localScale = new Vector3(_width, _height * 0.66f, 1f);
        }

        private void LateUpdate()
        {
            if (_bar == null) return;

            var cam = Camera.main;
            if (cam != null) _bar.rotation = cam.transform.rotation;

            if (_hp == null || _hp.IsDead)
            {
                if (_bar.gameObject.activeSelf) _bar.gameObject.SetActive(false);
                return;
            }
            if (!_bar.gameObject.activeSelf) _bar.gameObject.SetActive(true);

            float r = Mathf.Clamp01(_hp.Ratio);
            _fill.localScale = new Vector3(Mathf.Max(0.0001f, _width * r), _height * 0.66f, 1f);
            if (_fillRenderer != null)
            {
                _fillRenderer.color = _isHero
                    ? (r > 0.5f ? new Color(0.2f, 0.9f, 0.3f)
                     : r > 0.25f ? new Color(0.95f, 0.75f, 0.2f)
                     : new Color(0.95f, 0.3f, 0.25f))
                    : new Color(0.95f, 0.3f, 0.25f);
            }
        }
    }
}
