using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StackMenu
{
    public class MenuItem
    {
        public string Label { get; set; } = "";
        public Action? OnClick { get; set; }
        public bool IsSeparator { get; set; }

        public static MenuItem Action(string label, Action onClick) => new MenuItem { Label = label, OnClick = onClick, IsSeparator = false };
        public static MenuItem Separator() => new MenuItem { IsSeparator = true };
    }

    // Runtime-built popup menu of clickable actions, shown near the mouse and closed via Hide().
    public class CardStackMenu
    {
        private GameObject? root;

        public bool IsOpen => root != null;

        public void Show(List<(string Label, Action OnClick)> options, Vector2 screenPosition)
        {
            var items = new List<MenuItem>();
            foreach (var opt in options)
            {
                items.Add(MenuItem.Action(opt.Label, opt.OnClick));
            }
            Show(items, screenPosition);
        }

        public void Show(List<MenuItem> items, Vector2 screenPosition)
        {
            Hide();

            if (items == null || items.Count == 0) return;

            root = new GameObject("StackMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var panelGO = new GameObject("Panel", typeof(Image));
            panelGO.transform.SetParent(root.transform, false);
            var panelImage = panelGO.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.08f, 0.1f, 0.92f);

            var panelRect = panelGO.GetComponent<RectTransform>();
            // Anchor to the canvas's center (its pivot) to match the coordinate space returned by ScreenToCanvasPosition.
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0f, 1f);

            float rowHeight = 28f;
            float separatorHeight = 8f;
            float width = 175f;

            float totalHeight = 8f;
            foreach (var item in items)
            {
                totalHeight += item.IsSeparator ? separatorHeight : rowHeight;
            }

            panelRect.sizeDelta = new Vector2(width, totalHeight);
            panelRect.anchoredPosition = ScreenToCanvasPosition(screenPosition, (RectTransform)root.transform);

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            float currentY = 4f;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (item.IsSeparator)
                {
                    var sepGO = new GameObject("Separator" + i, typeof(Image));
                    sepGO.transform.SetParent(panelGO.transform, false);

                    var sepImage = sepGO.GetComponent<Image>();
                    sepImage.color = new Color(1f, 1f, 1f, 0.22f);
                    sepImage.raycastTarget = false;

                    var sepRect = sepGO.GetComponent<RectTransform>();
                    sepRect.anchorMin = new Vector2(0f, 1f);
                    sepRect.anchorMax = new Vector2(0f, 1f);
                    sepRect.pivot = new Vector2(0f, 1f);
                    sepRect.sizeDelta = new Vector2(width - 16f, 1.5f);
                    sepRect.anchoredPosition = new Vector2(8f, -currentY - (separatorHeight / 2f));

                    currentY += separatorHeight;
                }
                else
                {
                    var rowGO = new GameObject("Row" + i, typeof(Image), typeof(Button));
                    rowGO.transform.SetParent(panelGO.transform, false);

                    var rowImage = rowGO.GetComponent<Image>();
                    rowImage.color = new Color(1f, 1f, 1f, 0.08f);

                    var button = rowGO.GetComponent<Button>();
                    var colors = button.colors;
                    colors.normalColor = new Color(1f, 1f, 1f, 0.08f);
                    colors.highlightedColor = new Color(1f, 1f, 1f, 0.22f);
                    colors.pressedColor = new Color(1f, 1f, 1f, 0.35f);
                    colors.selectedColor = new Color(1f, 1f, 1f, 0.08f);
                    button.colors = colors;

                    var onClick = item.OnClick;
                    button.onClick.AddListener(() =>
                    {
                        onClick?.Invoke();
                        Hide();
                    });

                    var rowRect = rowGO.GetComponent<RectTransform>();
                    rowRect.anchorMin = new Vector2(0f, 1f);
                    rowRect.anchorMax = new Vector2(0f, 1f);
                    rowRect.pivot = new Vector2(0f, 1f);
                    rowRect.sizeDelta = new Vector2(width - 8f, rowHeight - 2f);
                    rowRect.anchoredPosition = new Vector2(4f, -currentY);

                    var textGO = new GameObject("Label", typeof(Text));
                    textGO.transform.SetParent(rowGO.transform, false);
                    var text = textGO.GetComponent<Text>();
                    text.font = font;
                    text.fontSize = 15;
                    text.color = Color.white;
                    text.alignment = TextAnchor.MiddleLeft;
                    text.raycastTarget = false;
                    text.text = item.Label;

                    var textRect = textGO.GetComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(8f, 0f);
                    textRect.offsetMax = new Vector2(-4f, 0f);

                    currentY += rowHeight;
                }
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
