using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
namespace StackMenu
{
    public class KeybindSettingUI : MonoBehaviour
    {
        // Phím tắt hiện tại
        public Key currentKey = Key.Space;
        public string labelPrefix = "Key";
        public Action<Key>? onKeyRebound;

        private Button? rebindButton;
        private CustomButton? customButton;
        private Text? buttonText;
        private bool isListening = false;
        private Coroutine? listenCoroutine;

        private void Awake()
        {
            SetupButton();
            UpdateButtonText();
        }

        public static Key ParseKey(ConfigEntry<string>? config, Key defaultKey)
        {
            if (config != null && !string.IsNullOrEmpty(config.Value) && Enum.TryParse<Key>(config.Value, true, out var key))
            {
                return key;
            }
            return defaultKey;
        }

        private void SetupButton()
        {
            customButton = GetComponent<CustomButton>();
            if (customButton != null)
            {
                customButton.Clicked -= OnButtonClicked;
                customButton.Clicked += OnButtonClicked;
            }

            rebindButton = GetComponent<Button>();
            if (rebindButton != null)
            {
                rebindButton.onClick.RemoveListener(OnButtonClicked);
                rebindButton.onClick.AddListener(OnButtonClicked);
            }

            buttonText = GetComponentInChildren<Text>();
        }

        private void OnButtonClicked()
        {
            StartListeningForKey();
        }

        public void StartListeningForKey()
        {
            if (isListening)
            {
                return;
            }

            if (listenCoroutine != null)
            {
                StopCoroutine(listenCoroutine);
            }
            listenCoroutine = StartCoroutine(WaitForKeyPress());
        }

        private System.Collections.IEnumerator WaitForKeyPress()
        {
            isListening = true;
            SetButtonText("Press any key...");

            yield return null;

            while (isListening)
            {
                if (Keyboard.current != null)
                {
                    foreach (var control in Keyboard.current.allControls)
                    {
                        if (control is UnityEngine.InputSystem.Controls.KeyControl keyControl && keyControl.wasPressedThisFrame)
                        {
                            Key pressedKey = keyControl.keyCode;
                            if (pressedKey == Key.Escape)
                            {
                                isListening = false;
                                UpdateButtonText();
                                listenCoroutine = null;
                                yield break;
                            }
                            currentKey = pressedKey;
                            isListening = false;

                            SaveKeybindSetting(currentKey);
                            UpdateButtonText();
                            listenCoroutine = null;
                            yield break;
                        }
                    }
                }
                yield return null;
            }
            listenCoroutine = null;
        }

        public void Initialize(Key initialKey, string prefix = "Key", Action<Key>? onKeyChanged = null)
        {
            currentKey = initialKey;
            labelPrefix = prefix;
            onKeyRebound = onKeyChanged;

            SetupButton();
            UpdateButtonText();
        }

        private void SetButtonText(string label)
        {
            if (buttonText != null)
            {
                buttonText.text = label;
            }

            if (customButton == null)
            {
                customButton = GetComponent<CustomButton>();
            }

            if (customButton != null && customButton.TextMeshPro != null)
            {
                customButton.TextMeshPro.text = label;
            }
        }

        private void UpdateButtonText()
        {
            SetButtonText($"{labelPrefix}: {currentKey}");
        }

        private void SaveKeybindSetting(Key newKey)
        {
            onKeyRebound?.Invoke(newKey);
            Debug.Log($"[StackMenu] Keybind saved to: {newKey}");
        }
    }

    public class ColorSettingUI : MonoBehaviour
    {
        public struct NamedColor
        {
            public string Name;
            public Color Color;

            public NamedColor(string name, Color color)
            {
                Name = name;
                Color = color;
            }
        }

        public static readonly NamedColor[] PaletteColors = new[]
        {
            new NamedColor("Red", new Color(0.957f, 0.263f, 0.212f, 1f)),
            new NamedColor("Coral", new Color(1f, 0.341f, 0.133f, 1f)),
            new NamedColor("Orange", new Color(1f, 0.596f, 0f, 1f)),
            new NamedColor("Amber", new Color(1f, 0.757f, 0.027f, 1f)),
            new NamedColor("Gold", new Color(1f, 0.85f, 0.2f, 1f)),
            new NamedColor("Yellow", new Color(1f, 0.922f, 0.231f, 1f)),

            new NamedColor("Lime", new Color(0.804f, 0.863f, 0.224f, 1f)),
            new NamedColor("Light Green", new Color(0.545f, 0.765f, 0.29f, 1f)),
            new NamedColor("Green", new Color(0.298f, 0.686f, 0.314f, 1f)),
            new NamedColor("Teal", new Color(0f, 0.588f, 0.533f, 1f)),
            new NamedColor("Cyan", new Color(0f, 0.737f, 0.831f, 1f)),
            new NamedColor("Sky Blue", new Color(0.012f, 0.663f, 0.957f, 1f)),

            new NamedColor("Blue", new Color(0.129f, 0.588f, 0.953f, 1f)),
            new NamedColor("Indigo", new Color(0.247f, 0.318f, 0.71f, 1f)),
            new NamedColor("Purple", new Color(0.612f, 0.153f, 0.69f, 1f)),
            new NamedColor("Deep Purple", new Color(0.404f, 0.227f, 0.718f, 1f)),
            new NamedColor("Magenta", new Color(0.914f, 0.118f, 0.388f, 1f)),
            new NamedColor("Pink", new Color(1f, 0.251f, 0.506f, 1f)),

            new NamedColor("Rose", new Color(0.957f, 0.247f, 0.369f, 1f)),
            new NamedColor("Peach", new Color(1f, 0.627f, 0.478f, 1f)),
            new NamedColor("Lavender", new Color(0.702f, 0.533f, 1f, 1f)),
            new NamedColor("White", new Color(1f, 1f, 1f, 1f)),
            new NamedColor("Silver", new Color(0.69f, 0.745f, 0.773f, 1f)),
            new NamedColor("Charcoal", new Color(0.271f, 0.353f, 0.392f, 1f)),
        };

        public Color currentColor = new Color(1f, 0.85f, 0.2f, 1f);
        public string labelPrefix = "Color";
        public Action<Color>? onColorChanged;

        private Button? rebindButton;
        private CustomButton? customButton;
        private Text? buttonText;

        private void Awake()
        {
            SetupButton();
            UpdateButtonText();
        }

        public static Color ParseColor(ConfigEntry<string>? config, Color defaultColor)
        {
            if (config != null && !string.IsNullOrEmpty(config.Value))
            {
                string val = config.Value.Trim();
                if (!val.StartsWith("#") && (val.Length == 6 || val.Length == 8))
                {
                    val = "#" + val;
                }
                if (ColorUtility.TryParseHtmlString(val, out var color))
                {
                    return color;
                }
            }
            return defaultColor;
        }

        public static string ColorToHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGBA(color);
        }

        private void SetupButton()
        {
            customButton = GetComponent<CustomButton>();
            if (customButton != null)
            {
                customButton.Clicked -= OnButtonClicked;
                customButton.Clicked += OnButtonClicked;
            }

            rebindButton = GetComponent<Button>();
            if (rebindButton != null)
            {
                rebindButton.onClick.RemoveListener(OnButtonClicked);
                rebindButton.onClick.AddListener(OnButtonClicked);
            }

            buttonText = GetComponentInChildren<Text>();
        }

        private void OnButtonClicked()
        {
            OpenColorPalette();
        }

        public void OpenColorPalette()
        {
            ColorPalettePopup.Open(labelPrefix, currentColor, PaletteColors, (newColor) =>
            {
                currentColor = newColor;
                SaveColorSetting(currentColor);
                UpdateButtonText();
            });
        }

        public static bool IsColorSimilar(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.05f &&
                   Mathf.Abs(a.g - b.g) < 0.05f &&
                   Mathf.Abs(a.b - b.b) < 0.05f &&
                   Mathf.Abs(a.a - b.a) < 0.05f;
        }

        public void Initialize(Color initialColor, string prefix = "Color", Action<Color>? onChanged = null)
        {
            currentColor = initialColor;
            labelPrefix = prefix;
            onColorChanged = onChanged;

            SetupButton();
            UpdateButtonText();
        }

        private void SetButtonText(string label)
        {
            if (buttonText != null)
            {
                buttonText.text = label;
            }

            if (customButton == null)
            {
                customButton = GetComponent<CustomButton>();
            }

            if (customButton != null && customButton.TextMeshPro != null)
            {
                customButton.TextMeshPro.text = label;
            }
        }

        private void UpdateButtonText()
        {
            string hex = ColorUtility.ToHtmlStringRGB(currentColor);
            string colorName = GetColorName(currentColor);
            SetButtonText($"{labelPrefix}: <color=#{hex}>■ {colorName}</color>");
        }

        public static string GetColorName(Color color)
        {
            for (int i = 0; i < PaletteColors.Length; i++)
            {
                if (IsColorSimilar(PaletteColors[i].Color, color))
                {
                    return PaletteColors[i].Name;
                }
            }
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        private void SaveColorSetting(Color newColor)
        {
            onColorChanged?.Invoke(newColor);
            Debug.Log($"[StackMenu] Color setting saved to: {ColorToHex(newColor)}");
        }
    }

    public class ColorSatValPicker : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public Action<float, float>? onSatValChanged;
        private RectTransform? rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            HandleInput(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            HandleInput(eventData);
        }

        private void HandleInput(PointerEventData eventData)
        {
            if (rectTransform == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                Rect rect = rectTransform.rect;
                float s = Mathf.Clamp01((localPoint.x - rect.xMin) / rect.width);
                float v = Mathf.Clamp01((localPoint.y - rect.yMin) / rect.height);
                onSatValChanged?.Invoke(s, v);
            }
        }
    }

    public class ColorHueSlider : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public Action<float>? onHueChanged;
        private RectTransform? rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            HandleInput(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            HandleInput(eventData);
        }

        private void HandleInput(PointerEventData eventData)
        {
            if (rectTransform == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                Rect rect = rectTransform.rect;
                float h = Mathf.Clamp01((localPoint.x - rect.xMin) / rect.width);
                onHueChanged?.Invoke(h);
            }
        }
    }

    public class ColorPalettePopup : MonoBehaviour
    {
        private static ColorPalettePopup? currentInstance;
        private Action<Color>? onColorSelected;

        private float currentH = 0.15f;
        private float currentS = 0.8f;
        private float currentV = 1f;

        private Texture2D? svTexture;
        private Texture2D? hueTexture;
        private Texture2D? ringTexture;
        private Texture2D? thumbTexture;

        private RawImage? svRawImage;
        private RectTransform? svRect;
        private RectTransform? svHandleRect;

        private RectTransform? hueRect;
        private RectTransform? hueThumbRect;
        private RawImage? hueThumbImage;

        private Image? previewImage;
        private Text? previewHexText;
        private Text? previewNameText;

        private readonly List<(Color Color, GameObject CheckGO)> swatchChecks = new List<(Color, GameObject)>();

        public static void Open(string title, Color selectedColor, ColorSettingUI.NamedColor[] colors, Action<Color> onSelected)
        {
            CloseCurrent();

            GameObject root = new GameObject("ColorPaletteCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2500;

            var popup = root.AddComponent<ColorPalettePopup>();
            popup.onColorSelected = onSelected;
            currentInstance = popup;

            popup.BuildUI(title, selectedColor, colors);
        }

        public static void CloseCurrent()
        {
            if (currentInstance != null)
            {
                Destroy(currentInstance.gameObject);
                currentInstance = null;
            }
        }

        private void OnDestroy()
        {
            if (svTexture != null) Destroy(svTexture);
            if (hueTexture != null) Destroy(hueTexture);
            if (ringTexture != null) Destroy(ringTexture);
            if (thumbTexture != null) Destroy(thumbTexture);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseCurrent();
            }
        }

        private void BuildUI(string title, Color selectedColor, ColorSettingUI.NamedColor[] colors)
        {
            Color.RGBToHSV(selectedColor, out currentH, out currentS, out currentV);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            // Fullscreen backdrop (click outside to dismiss)
            GameObject backdropGO = new GameObject("Backdrop", typeof(Image), typeof(Button));
            backdropGO.transform.SetParent(transform, false);
            var backdropImg = backdropGO.GetComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.55f);
            var backdropRect = backdropGO.GetComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;

            var backdropBtn = backdropGO.GetComponent<Button>();
            backdropBtn.onClick.AddListener(CloseCurrent);

            // Center Panel
            GameObject panelGO = new GameObject("PalettePanel", typeof(Image));
            panelGO.transform.SetParent(transform, false);
            var panelImg = panelGO.GetComponent<Image>();
            panelImg.color = new Color(0.11f, 0.11f, 0.14f, 0.98f);

            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(330f, 460f);
            panelRect.anchoredPosition = Vector2.zero;

            // Header Title
            GameObject titleGO = new GameObject("Title", typeof(Text));
            titleGO.transform.SetParent(panelGO.transform, false);
            var titleText = titleGO.GetComponent<Text>();
            titleText.font = font;
            titleText.fontSize = 15;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = Color.white;
            titleText.text = $"Choose {title}";
            titleText.alignment = TextAnchor.MiddleLeft;

            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(14f, -8f);
            titleRect.sizeDelta = new Vector2(-60f, 26f);

            // Close button (X)
            GameObject closeBtnGO = new GameObject("CloseBtn", typeof(Image), typeof(Button));
            closeBtnGO.transform.SetParent(panelGO.transform, false);
            var closeImg = closeBtnGO.GetComponent<Image>();
            closeImg.color = new Color(1f, 1f, 1f, 0.1f);

            var closeBtn = closeBtnGO.GetComponent<Button>();
            var closeColors = closeBtn.colors;
            closeColors.highlightedColor = new Color(0.9f, 0.2f, 0.2f, 0.8f);
            closeBtn.colors = closeColors;
            closeBtn.onClick.AddListener(CloseCurrent);

            var closeRect = closeBtnGO.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-10f, -8f);
            closeRect.sizeDelta = new Vector2(24f, 24f);

            GameObject closeTextGO = new GameObject("XText", typeof(Text));
            closeTextGO.transform.SetParent(closeBtnGO.transform, false);
            var closeText = closeTextGO.GetComponent<Text>();
            closeText.font = font;
            closeText.fontSize = 13;
            closeText.color = Color.white;
            closeText.alignment = TextAnchor.MiddleCenter;
            closeText.text = "✕";
            var closeTextRect = closeTextGO.GetComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            // ==================== COLOR PICKER SECTION ====================
            ringTexture = CreateRingTexture(32);
            thumbTexture = CreateCircleThumbTexture(32);

            // 1. Preview Box (Left)
            GameObject previewGO = new GameObject("PreviewBlock", typeof(Image));
            previewGO.transform.SetParent(panelGO.transform, false);
            previewImage = previewGO.GetComponent<Image>();
            previewImage.color = selectedColor;

            var prevRect = previewGO.GetComponent<RectTransform>();
            prevRect.anchorMin = new Vector2(0f, 1f);
            prevRect.anchorMax = new Vector2(0f, 1f);
            prevRect.pivot = new Vector2(0f, 1f);
            prevRect.anchoredPosition = new Vector2(12f, -38f);
            prevRect.sizeDelta = new Vector2(86f, 96f);

            // Preview Text details at bottom of Preview Box
            GameObject previewTextBgGO = new GameObject("PreviewTextBg", typeof(Image));
            previewTextBgGO.transform.SetParent(previewGO.transform, false);
            var previewTextBg = previewTextBgGO.GetComponent<Image>();
            previewTextBg.color = new Color(0f, 0f, 0f, 0.6f);
            var previewTextBgRect = previewTextBgGO.GetComponent<RectTransform>();
            previewTextBgRect.anchorMin = new Vector2(0f, 0f);
            previewTextBgRect.anchorMax = new Vector2(1f, 0f);
            previewTextBgRect.pivot = new Vector2(0.5f, 0f);
            previewTextBgRect.sizeDelta = new Vector2(0f, 32f);
            previewTextBgRect.anchoredPosition = Vector2.zero;

            GameObject hexTextGO = new GameObject("HexText", typeof(Text));
            hexTextGO.transform.SetParent(previewTextBgGO.transform, false);
            previewHexText = hexTextGO.GetComponent<Text>();
            previewHexText.font = font;
            previewHexText.fontSize = 11;
            previewHexText.color = Color.white;
            previewHexText.alignment = TextAnchor.MiddleCenter;
            var hexRect = hexTextGO.GetComponent<RectTransform>();
            hexRect.anchorMin = new Vector2(0f, 0.5f);
            hexRect.anchorMax = new Vector2(1f, 1f);
            hexRect.offsetMin = Vector2.zero;
            hexRect.offsetMax = Vector2.zero;

            GameObject nameTextGO = new GameObject("NameText", typeof(Text));
            nameTextGO.transform.SetParent(previewTextBgGO.transform, false);
            previewNameText = nameTextGO.GetComponent<Text>();
            previewNameText.font = font;
            previewNameText.fontSize = 10;
            previewNameText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            previewNameText.alignment = TextAnchor.MiddleCenter;
            var nameRect = nameTextGO.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0.5f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // 2. Saturation / Value 2D Box (Right)
            GameObject svGO = new GameObject("SVBox", typeof(RawImage), typeof(ColorSatValPicker));
            svGO.transform.SetParent(panelGO.transform, false);
            svRawImage = svGO.GetComponent<RawImage>();
            svRect = svGO.GetComponent<RectTransform>();
            svRect.anchorMin = new Vector2(0f, 1f);
            svRect.anchorMax = new Vector2(0f, 1f);
            svRect.pivot = new Vector2(0f, 1f);
            svRect.anchoredPosition = new Vector2(104f, -38f);
            svRect.sizeDelta = new Vector2(214f, 96f);

            svTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            svTexture.wrapMode = TextureWrapMode.Clamp;
            svTexture.filterMode = FilterMode.Bilinear;
            svRawImage.texture = svTexture;
            UpdateSVTexture(currentH);

            var svPicker = svGO.GetComponent<ColorSatValPicker>();
            svPicker.onSatValChanged = (s, v) =>
            {
                currentS = s;
                currentV = v;
                OnHSVUpdated();
            };

            // SV Handle Ring
            GameObject svHandleGO = new GameObject("SVHandle", typeof(RawImage));
            svHandleGO.transform.SetParent(svGO.transform, false);
            var svHandleImg = svHandleGO.GetComponent<RawImage>();
            svHandleImg.texture = ringTexture;
            svHandleImg.raycastTarget = false;
            svHandleRect = svHandleGO.GetComponent<RectTransform>();
            svHandleRect.anchorMin = Vector2.zero;
            svHandleRect.anchorMax = Vector2.zero;
            svHandleRect.pivot = new Vector2(0.5f, 0.5f);
            svHandleRect.sizeDelta = new Vector2(18f, 18f);

            // 3. Hue Rainbow Slider (Bottom of Picker)
            GameObject hueGO = new GameObject("HueBar", typeof(RawImage), typeof(ColorHueSlider));
            hueGO.transform.SetParent(panelGO.transform, false);
            var hueRawImage = hueGO.GetComponent<RawImage>();
            hueRect = hueGO.GetComponent<RectTransform>();
            hueRect.anchorMin = new Vector2(0f, 1f);
            hueRect.anchorMax = new Vector2(0f, 1f);
            hueRect.pivot = new Vector2(0f, 1f);
            hueRect.anchoredPosition = new Vector2(12f, -142f);
            hueRect.sizeDelta = new Vector2(306f, 14f);

            hueTexture = CreateHueTexture();
            hueRawImage.texture = hueTexture;

            var hueSlider = hueGO.GetComponent<ColorHueSlider>();
            hueSlider.onHueChanged = (h) =>
            {
                currentH = h;
                UpdateSVTexture(currentH);
                OnHSVUpdated();
            };

            // Hue Slider Thumb
            GameObject hueThumbGO = new GameObject("HueThumb", typeof(RawImage));
            hueThumbGO.transform.SetParent(hueGO.transform, false);
            hueThumbImage = hueThumbGO.GetComponent<RawImage>();
            hueThumbImage.texture = thumbTexture;
            hueThumbImage.raycastTarget = false;
            hueThumbRect = hueThumbGO.GetComponent<RectTransform>();
            hueThumbRect.anchorMin = new Vector2(0f, 0.5f);
            hueThumbRect.anchorMax = new Vector2(0f, 0.5f);
            hueThumbRect.pivot = new Vector2(0.5f, 0.5f);
            hueThumbRect.sizeDelta = new Vector2(18f, 18f);

            // ==================== PALETTE PRESETS SECTION ====================
            GameObject palLabelGO = new GameObject("PaletteLabel", typeof(Text));
            palLabelGO.transform.SetParent(panelGO.transform, false);
            var palLabel = palLabelGO.GetComponent<Text>();
            palLabel.font = font;
            palLabel.fontSize = 12;
            palLabel.color = new Color(0.7f, 0.7f, 0.75f, 1f);
            palLabel.text = "Color Palette";
            var palLabelRect = palLabelGO.GetComponent<RectTransform>();
            palLabelRect.anchorMin = new Vector2(0f, 1f);
            palLabelRect.anchorMax = new Vector2(1f, 1f);
            palLabelRect.pivot = new Vector2(0f, 1f);
            palLabelRect.anchoredPosition = new Vector2(14f, -164f);
            palLabelRect.sizeDelta = new Vector2(-28f, 18f);

            // Swatches Grid
            GameObject gridGO = new GameObject("Grid", typeof(GridLayoutGroup));
            gridGO.transform.SetParent(panelGO.transform, false);
            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(44f, 34f);
            grid.spacing = new Vector2(8f, 6f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 6;
            grid.childAlignment = TextAnchor.MiddleCenter;

            var gridRect = gridGO.GetComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.5f, 1f);
            gridRect.anchorMax = new Vector2(0.5f, 1f);
            gridRect.pivot = new Vector2(0.5f, 1f);
            gridRect.anchoredPosition = new Vector2(0f, -186f);
            gridRect.sizeDelta = new Vector2(306f, 160f);

            swatchChecks.Clear();
            for (int i = 0; i < colors.Length; i++)
            {
                var namedColor = colors[i];
                GameObject swatchBtnGO = new GameObject("Swatch_" + namedColor.Name, typeof(Image), typeof(Button));
                swatchBtnGO.transform.SetParent(gridGO.transform, false);

                var swatchImg = swatchBtnGO.GetComponent<Image>();
                swatchImg.color = namedColor.Color;

                var swatchBtn = swatchBtnGO.GetComponent<Button>();
                var colorsBlock = swatchBtn.colors;
                colorsBlock.highlightedColor = Color.Lerp(namedColor.Color, Color.white, 0.3f);
                colorsBlock.pressedColor = Color.Lerp(namedColor.Color, Color.black, 0.3f);
                swatchBtn.colors = colorsBlock;

                Color picked = namedColor.Color;
                swatchBtn.onClick.AddListener(() =>
                {
                    SelectColor(picked);
                });

                // Selection checkmark
                GameObject checkGO = new GameObject("Check", typeof(Text));
                checkGO.transform.SetParent(swatchBtnGO.transform, false);
                var checkText = checkGO.GetComponent<Text>();
                checkText.font = font;
                checkText.fontSize = 16;
                checkText.fontStyle = FontStyle.Bold;
                float luminance = 0.299f * picked.r + 0.587f * picked.g + 0.114f * picked.b;
                checkText.color = luminance > 0.5f ? Color.black : Color.white;
                checkText.alignment = TextAnchor.MiddleCenter;
                checkText.text = "✓";
                checkText.raycastTarget = false;

                var checkRect = checkGO.GetComponent<RectTransform>();
                checkRect.anchorMin = Vector2.zero;
                checkRect.anchorMax = Vector2.one;
                checkRect.offsetMin = Vector2.zero;
                checkRect.offsetMax = Vector2.zero;

                swatchChecks.Add((picked, checkGO));
            }

            // ==================== BOTTOM DONE BUTTON ====================
            GameObject doneBtnGO = new GameObject("DoneButton", typeof(Image), typeof(Button));
            doneBtnGO.transform.SetParent(panelGO.transform, false);
            var doneImg = doneBtnGO.GetComponent<Image>();
            doneImg.color = new Color(0.2f, 0.6f, 0.9f, 1f);

            var doneBtn = doneBtnGO.GetComponent<Button>();
            var doneColors = doneBtn.colors;
            doneColors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
            doneColors.pressedColor = new Color(0.15f, 0.5f, 0.8f, 1f);
            doneBtn.colors = doneColors;
            doneBtn.onClick.AddListener(CloseCurrent);

            var doneRect = doneBtnGO.GetComponent<RectTransform>();
            doneRect.anchorMin = new Vector2(0.5f, 0f);
            doneRect.anchorMax = new Vector2(0.5f, 0f);
            doneRect.pivot = new Vector2(0.5f, 0f);
            doneRect.anchoredPosition = new Vector2(0f, 12f);
            doneRect.sizeDelta = new Vector2(130f, 30f);

            GameObject doneTextGO = new GameObject("DoneText", typeof(Text));
            doneTextGO.transform.SetParent(doneBtnGO.transform, false);
            var doneText = doneTextGO.GetComponent<Text>();
            doneText.font = font;
            doneText.fontSize = 14;
            doneText.fontStyle = FontStyle.Bold;
            doneText.color = Color.white;
            doneText.alignment = TextAnchor.MiddleCenter;
            doneText.text = "Done";
            var doneTextRect = doneTextGO.GetComponent<RectTransform>();
            doneTextRect.anchorMin = Vector2.zero;
            doneTextRect.anchorMax = Vector2.one;
            doneTextRect.offsetMin = Vector2.zero;
            doneTextRect.offsetMax = Vector2.zero;

            // Initial visual sync
            UpdateVisuals();
        }

        public void SelectColor(Color color)
        {
            Color.RGBToHSV(color, out currentH, out currentS, out currentV);
            UpdateSVTexture(currentH);
            OnHSVUpdated();
        }

        private void OnHSVUpdated()
        {
            Color color = Color.HSVToRGB(currentH, currentS, currentV);
            UpdateVisuals();
            onColorSelected?.Invoke(color);
        }

        private void UpdateVisuals()
        {
            Color color = Color.HSVToRGB(currentH, currentS, currentV);

            // 1. Preview
            if (previewImage != null)
            {
                previewImage.color = color;
            }
            if (previewHexText != null)
            {
                previewHexText.text = $"#{ColorUtility.ToHtmlStringRGB(color)}";
            }
            if (previewNameText != null)
            {
                previewNameText.text = ColorSettingUI.GetColorName(color);
            }

            // 2. SV Handle Position
            if (svRect != null && svHandleRect != null)
            {
                Rect rect = svRect.rect;
                float x = currentS * rect.width;
                float y = currentV * rect.height;
                svHandleRect.anchoredPosition = new Vector2(x, y);
            }

            // 3. Hue Thumb Position and Color
            if (hueRect != null && hueThumbRect != null)
            {
                Rect rect = hueRect.rect;
                float x = currentH * rect.width;
                hueThumbRect.anchoredPosition = new Vector2(x, 0f);
            }
            if (hueThumbImage != null)
            {
                hueThumbImage.color = Color.HSVToRGB(currentH, 1f, 1f);
            }

            // 4. Update Swatch Checkmarks
            foreach (var item in swatchChecks)
            {
                if (item.CheckGO != null)
                {
                    bool isMatch = ColorSettingUI.IsColorSimilar(item.Color, color);
                    item.CheckGO.SetActive(isMatch);
                }
            }
        }

        private void UpdateSVTexture(float hue)
        {
            if (svTexture == null) return;
            Color[] pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                float v = y / 63f;
                for (int x = 0; x < 64; x++)
                {
                    float s = x / 63f;
                    pixels[y * 64 + x] = Color.HSVToRGB(hue, s, v);
                }
            }
            svTexture.SetPixels(pixels);
            svTexture.Apply();
        }

        private static Texture2D CreateHueTexture()
        {
            Texture2D tex = new Texture2D(256, 1, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[256];
            for (int x = 0; x < 256; x++)
            {
                pixels[x] = Color.HSVToRGB(x / 255f, 1f, 1f);
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRingTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            float center = (size - 1) / 2f;
            float outerR = center;
            float innerR = center * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= outerR && dist >= innerR)
                    {
                        if (dist > outerR - 1.2f || dist < innerR + 1.2f)
                        {
                            pixels[y * size + x] = new Color(0f, 0f, 0f, 0.75f);
                        }
                        else
                        {
                            pixels[y * size + x] = Color.white;
                        }
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCircleThumbTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            float center = (size - 1) / 2f;
            float outerR = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= outerR)
                    {
                        if (dist > outerR - 2f)
                        {
                            pixels[y * size + x] = Color.white;
                        }
                        else
                        {
                            pixels[y * size + x] = Color.white;
                        }
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}