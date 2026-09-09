using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StackMenu
{
    // Runtime-built popup menu of clickable actions, shown near the mouse and closed via Hide().
    public class CardStackMenu
    {
        private GameObject? root;

        public bool IsOpen => root != null;

        public void Show(List<(string Label, Action OnClick)> options, Vector2 screenPosition)
        {
            Hide();

            root = new GameObject("StackMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var panelGO = new GameObject("Panel", typeof(Image));
            panelGO.transform.SetParent(root.transform, false);
            var panelImage = panelGO.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);

            var panelRect = panelGO.GetComponent<RectTransform>();
            // Anchor to the canvas's center (its pivot) to match the coordinate space returned by ScreenToCanvasPosition.
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0f, 1f);

            float rowHeight = 28f;
            float width = 160f;
            panelRect.sizeDelta = new Vector2(width, rowHeight * Mathf.Max(options.Count, 1) + 8f);
            panelRect.anchoredPosition = ScreenToCanvasPosition(screenPosition, (RectTransform)root.transform);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            for (int i = 0; i < options.Count; i++)
            {
                var (label, onClick) = options[i];

                var rowGO = new GameObject("Row" + i, typeof(Image), typeof(Button));
                rowGO.transform.SetParent(panelGO.transform, false);

                var rowImage = rowGO.GetComponent<Image>();
                rowImage.color = new Color(1f, 1f, 1f, 0.08f);

                var button = rowGO.GetComponent<Button>();
                button.onClick.AddListener(() => onClick());

                var rowRect = rowGO.GetComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0f, 1f);
                rowRect.anchorMax = new Vector2(0f, 1f);
                rowRect.pivot = new Vector2(0f, 1f);
                rowRect.sizeDelta = new Vector2(width - 8f, rowHeight - 2f);
                rowRect.anchoredPosition = new Vector2(4f, -4f - i * rowHeight);

                var textGO = new GameObject("Label", typeof(Text));
                textGO.transform.SetParent(rowGO.transform, false);
                var text = textGO.GetComponent<Text>();
                text.font = font;
                text.fontSize = 16;
                text.color = Color.white;
                text.alignment = TextAnchor.MiddleLeft;
                text.raycastTarget = false;
                text.text = label;

                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(6f, 0f);
                textRect.offsetMax = Vector2.zero;
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                UnityEngine.Object.Destroy(root);
                root = null;
            }
        }

        private static Vector2 ScreenToCanvasPosition(Vector2 screenPosition, RectTransform canvasRect)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out var localPoint);
            return localPoint;
        }
    }
}
