using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Data;

namespace WhiskerHaven.UI
{
    /// <summary>
    /// Fluent helpers for building Unity UI programmatically.
    /// All methods parent the new element and return it for further setup.
    /// </summary>
    public static class UIFactory
    {
        // ── Warm Cozy Palette ────────────────────────────────────────────────
        public static readonly Color BG_DARK   = H("2C1810");
        public static readonly Color BG_MED    = H("4A2F1E");
        public static readonly Color BG_LIGHT  = H("7A5C3A");
        public static readonly Color CREAM     = H("F5E6D3");
        public static readonly Color CREAM_ALT = H("EDD5B3");
        public static readonly Color AMBER     = H("C17F45");
        public static readonly Color AMBER_D   = H("8B5523");
        public static readonly Color TEXT_D    = H("2C1810");
        public static readonly Color TEXT_L    = H("F5E6D3");
        public static readonly Color SUCCESS   = H("5C8A3C");
        public static readonly Color GOLD      = H("E8C55E");
        public static readonly Color DANGER    = H("B03030");
        public static readonly Color PURR_CLR  = H("E8A84C");
        public static readonly Color RARE_CLR  = H("5599FF");
        public static readonly Color UNCOMMON  = H("55AA55");
        public static readonly Color COMMON    = H("AAAAAA");

        private static Color H(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            return c;
        }

        // ── Containers ────────────────────────────────────────────────────────

        /// <summary>Creates an Image-backed panel stretched to fill parent.</summary>
        public static GameObject Panel(Transform parent, string name, Color? color = null)
        {
            var go  = Make(parent, name);
            var img = go.AddComponent<Image>();
            img.color = color ?? CREAM;
            Stretch(go);
            return go;
        }

        /// <summary>Creates a transparent container stretched to fill parent.</summary>
        public static GameObject Group(Transform parent, string name)
        {
            var go = Make(parent, name);
            go.AddComponent<RectTransform>();
            Stretch(go);
            return go;
        }

        /// <summary>Adds a HorizontalLayoutGroup to a GameObject.</summary>
        public static HorizontalLayoutGroup HLayout(GameObject go, float spacing = 8, RectOffset padding = null)
        {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = false;
            h.padding = padding ?? new RectOffset(8, 8, 4, 4);
            return h;
        }

        /// <summary>Adds a VerticalLayoutGroup to a GameObject.</summary>
        public static VerticalLayoutGroup VLayout(GameObject go, float spacing = 8, RectOffset padding = null)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.childControlWidth = true;
            v.childControlHeight = false;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            v.padding = padding ?? new RectOffset(8, 8, 8, 8);
            return v;
        }

        public static GridLayoutGroup Grid(GameObject go, Vector2 cellSize, float spacing = 8)
        {
            var g = go.AddComponent<GridLayoutGroup>();
            g.cellSize = cellSize;
            g.spacing = new Vector2(spacing, spacing);
            g.padding = new RectOffset(8, 8, 8, 8);
            g.constraint = GridLayoutGroup.Constraint.Flexible;
            return g;
        }

        public static ContentSizeFitter Fitter(GameObject go, ContentSizeFitter.FitMode vertical = ContentSizeFitter.FitMode.PreferredSize)
        {
            var f = go.AddComponent<ContentSizeFitter>();
            f.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            f.verticalFit   = vertical;
            return f;
        }

        // ── Text ──────────────────────────────────────────────────────────────

        public static TextMeshProUGUI Text(Transform parent, string name,
            string content = "", float size = 16f, Color? color = null,
            TextAlignmentOptions align = TextAlignmentOptions.Left,
            FontStyles style = FontStyles.Normal)
        {
            var go  = Make(parent, name);
            Stretch(go);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text        = content;
            tmp.fontSize    = size;
            tmp.color       = color ?? TEXT_D;
            tmp.alignment   = align;
            tmp.fontStyle   = style;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return tmp;
        }

        // ── Buttons ───────────────────────────────────────────────────────────

        public static Button Btn(Transform parent, string name,
            string label = "", Color? bg = null, Color? textColor = null, float fontSize = 14f)
        {
            var go  = Make(parent, name);
            var img = go.AddComponent<Image>();
            img.color = bg ?? AMBER;
            var btn = go.AddComponent<Button>();
            ApplyBtnColors(btn, img, bg ?? AMBER);

            var labelGo = Make(go.transform, "Label");
            Stretch(labelGo);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = fontSize;
            tmp.color     = textColor ?? TEXT_L;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;

            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;

            return btn;
        }

        public static Button IconBtn(Transform parent, string name, string emoji, Color? bg = null)
            => Btn(parent, name, emoji, bg ?? BG_MED, TEXT_L, 18f);

        private static void ApplyBtnColors(Button btn, Image img, Color c)
        {
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor      = c;
            colors.highlightedColor = new Color(Mathf.Min(c.r * 1.2f, 1f), Mathf.Min(c.g * 1.2f, 1f), Mathf.Min(c.b * 1.2f, 1f));
            colors.pressedColor     = new Color(c.r * 0.75f, c.g * 0.75f, c.b * 0.75f);
            colors.selectedColor    = c;
            colors.fadeDuration     = 0.08f;
            btn.colors = colors;
        }

        // ── Sliders ───────────────────────────────────────────────────────────

        public static Slider SliderH(Transform parent, string name, Color? fillColor = null, float height = 20f)
        {
            var go = Make(parent, name);
            var rt = go.GetComponent<RectTransform>();

            // BG
            var bg    = Make(go.transform, "Background"); Stretch(bg);
            var bgImg = bg.AddComponent<Image>(); bgImg.color = new Color(0, 0, 0, 0.25f);

            // Fill area / fill
            var fa = Make(go.transform, "Fill Area"); Stretch(fa);
            var fi = Make(fa.transform, "Fill"); Stretch(fi);
            var fiImg = fi.AddComponent<Image>(); fiImg.color = fillColor ?? AMBER;

            // Handle area / handle
            var ha  = Make(go.transform, "Handle Slide Area"); Stretch(ha);
            var hdl = Make(ha.transform, "Handle");
            var hdlRt = hdl.GetComponent<RectTransform>();
            hdlRt.sizeDelta = new Vector2(height, height);
            var hdlImg = hdl.AddComponent<Image>(); hdlImg.color = Color.white;

            var sl = go.AddComponent<Slider>();
            sl.fillRect   = fi.GetComponent<RectTransform>();
            sl.handleRect = hdl.GetComponent<RectTransform>();
            sl.targetGraphic = hdlImg;
            sl.direction  = Slider.Direction.LeftToRight;
            sl.minValue   = 0f; sl.maxValue = 1f; sl.value = 0f;

            var nav = sl.navigation;
            nav.mode = Navigation.Mode.None;
            sl.navigation = nav;

            return sl;
        }

        // ── ScrollRect ────────────────────────────────────────────────────────

        public static (ScrollRect scroll, RectTransform content) ScrollV(
            Transform parent, string name, Color? bgColor = null)
        {
            var go = Make(parent, name);
            Stretch(go);
            var bgImg = go.AddComponent<Image>();
            bgImg.color = bgColor ?? new Color(0, 0, 0, 0.05f);

            var viewport = Make(go.transform, "Viewport");
            Stretch(viewport);
            var vpImg  = viewport.AddComponent<Image>();
            vpImg.color = Color.clear;
            var vpMask = viewport.AddComponent<Mask>();
            vpMask.showMaskGraphic = false;

            var content = Make(viewport.transform, "Content");
            var crt     = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 1);
            crt.anchorMax = new Vector2(1, 1);
            crt.pivot     = new Vector2(0.5f, 1);
            crt.sizeDelta = new Vector2(0, 0);

            var scroll = go.AddComponent<ScrollRect>();
            scroll.viewport        = viewport.GetComponent<RectTransform>();
            scroll.content         = crt;
            scroll.horizontal      = false;
            scroll.vertical        = true;
            scroll.scrollSensitivity = 30f;
            scroll.movementType    = ScrollRect.MovementType.Elastic;

            return (scroll, crt);
        }

        // ── CanvasGroup ───────────────────────────────────────────────────────

        public static CanvasGroup CG(GameObject go, float alpha = 1f, bool interactable = true)
        {
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha         = alpha;
            cg.interactable  = interactable;
            cg.blocksRaycasts = interactable;
            return cg;
        }

        // ── Layout Positioning ────────────────────────────────────────────────

        public static void Stretch(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin      = Vector2.zero;
            rt.anchorMax      = Vector2.one;
            rt.offsetMin      = Vector2.zero;
            rt.offsetMax      = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        /// Top-anchored bar of fixed height
        public static void AnchorTop(GameObject go, float height, float offsetY = 0f)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, height);
            rt.anchoredPosition = new Vector2(0, offsetY);
        }

        /// Bottom-anchored bar of fixed height
        public static void AnchorBot(GameObject go, float height, float offsetY = 0f)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot     = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, height);
            rt.anchoredPosition = new Vector2(0, offsetY);
        }

        /// Stretch but with top/bottom margin
        public static void StretchWithMargin(GameObject go, float top, float bottom, float left = 0, float right = 0)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        /// Fixed size centered
        public static void Center(GameObject go, Vector2 size)
        {
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
        }

        // ── LayoutElement ─────────────────────────────────────────────────────

        public static LayoutElement LE(GameObject go, float? minH = null, float? prefH = null,
            float? minW = null, float? prefW = null, float? flexW = null, float? flexH = null)
        {
            var le = go.AddComponent<LayoutElement>();
            if (minH.HasValue)  le.minHeight     = minH.Value;
            if (prefH.HasValue) le.preferredHeight = prefH.Value;
            if (minW.HasValue)  le.minWidth      = minW.Value;
            if (prefW.HasValue) le.preferredWidth = prefW.Value;
            if (flexW.HasValue) le.flexibleWidth  = flexW.Value;
            if (flexH.HasValue) le.flexibleHeight = flexH.Value;
            return le;
        }

        // ── Internal ──────────────────────────────────────────────────────────

        public static GameObject Make(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        public static Color RarityColor(CatRarity r) => r switch
        {
            CatRarity.Common    => COMMON,
            CatRarity.Uncommon  => UNCOMMON,
            CatRarity.Rare      => RARE_CLR,
            CatRarity.Epic      => H("AA55FF"),
            CatRarity.Legendary => H("FFAA00"),
            _ => Color.white
        };
    }
}
