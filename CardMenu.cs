using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace StackMenu
{
    public static class CardMenu
    {
        public const string PinnedKey = "stackmenu_pinned";
        public const string PinnedPosXKey = "stackmenu_pinned_x";
        public const string PinnedPosYKey = "stackmenu_pinned_y";
        public const string PinnedPosZKey = "stackmenu_pinned_z";

        public const string CompactedKey = "stackmenu_compacted";
        public const string CompactedDataKey = "stackmenu_compacted_data";
        public const string CompactedCountKey = "stackmenu_compacted_count";

        private static readonly Dictionary<GameCard, Vector3> pinnedCards = new Dictionary<GameCard, Vector3>();
        private static readonly HashSet<GameCard> knownCards = new HashSet<GameCard>();

        public static bool IsPinned(GameCard card)
        {
            if (card == null || card.CardData == null) return false;
            GameCard root = card.GetRootCard();
            if (root == null || root.CardData == null) return false;

            if (root.CardData.LeftoverExtraData != null &&
                root.CardData.LeftoverExtraData.Any(x => x.AttributeId == PinnedKey && x.BoolValue))
            {
                return true;
            }
            return pinnedCards.ContainsKey(root);
        }

        public static void PinCard(GameCard card)
        {
            if (card == null || card.CardData == null) return;
            GameCard root = card.GetRootCard();
            if (root == null || root.CardData == null) return;

            Vector3 pos = root.transform.position;

            SetExtraData(root.CardData, PinnedKey, true);
            SetExtraData(root.CardData, PinnedPosXKey, pos.x);
            SetExtraData(root.CardData, PinnedPosYKey, pos.y);
            SetExtraData(root.CardData, PinnedPosZKey, pos.z);

            root.PushEnabled = false;
            root.CardData.MyGameCard.BeingDragged = false;
            pinnedCards[root] = pos;
            UpdateBadge(root);

            if (AudioManager.me != null)
            {
                AudioManager.me.PlaySound2D(AudioManager.me.CardDrop, 1.3f, 0.3f);
            }
        }

        public static void UnpinCard(GameCard card)
        {
            if (card == null || card.CardData == null) return;
            GameCard root = card.GetRootCard();
            if (root == null || root.CardData == null) return;

            RemoveExtraData(root.CardData, PinnedKey);
            RemoveExtraData(root.CardData, PinnedPosXKey);
            RemoveExtraData(root.CardData, PinnedPosYKey);
            RemoveExtraData(root.CardData, PinnedPosZKey);

            root.PushEnabled = true;
            pinnedCards.Remove(root);
            UpdateBadge(root);

            if (AudioManager.me != null)
            {
                AudioManager.me.PlaySound2D(AudioManager.me.CardDrop, 0.9f, 0.3f);
            }
        }

        public static bool IsCompacted(GameCard card)
        {
            if (card == null || card.CardData == null) return false;
            GameCard root = card.GetRootCard();
            if (root == null || root.CardData == null) return false;

            return root.CardData.Id == "stackmenu_compactcard";
        }

        public static int GetCompactedCount(GameCard card)
        {
            if (card == null || card.CardData == null) return 0;
            GameCard root = card.GetRootCard();
            if (root == null || root.CardData == null) return 0;

            if (root.CardData is CompactCard compactCard && compactCard.CompactedData != null)
            {
                return compactCard.CompactedData.Count;
            }

            if (root.CardData.LeftoverExtraData == null) return 0;

            var extra = root.CardData.LeftoverExtraData.FirstOrDefault(x => x.AttributeId == CompactedCountKey);
            return extra != null ? extra.IntValue : 0;
        }

        public static List<SavedCard> GetStoredCards(CardData cardData)
        {
            var result = new List<SavedCard>();
            if (cardData == null || cardData.LeftoverExtraData == null) return result;

            var dataExtra = GetExtraData(cardData, CompactedDataKey);
            if (dataExtra != null && !string.IsNullOrEmpty(dataExtra.StringValue))
            {
                try
                {
                    var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SavedCard>>(dataExtra.StringValue);
                    if (list != null)
                    {
                        result.AddRange(list);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[StackMenu] Error deserializing compacted cards: {ex.Message}");
                }
            }
            return result;
        }

        public static int GetCompactedExtraValue(CardData cardData)
        {
            if (cardData == null || !IsCompactedCardData(cardData)) return 0;

            var stored = GetStoredCards(cardData);
            if (stored.Count == 0) return 0;

            int total = 0;
            bool isCities = WorldManager.instance != null && WorldManager.instance.CurrentBoard != null && WorldManager.instance.CurrentBoard.Location == Location.Cities;

            foreach (var saved in stored)
            {
                if (saved == null || string.IsNullOrEmpty(saved.CardPrefabId)) continue;
                CardData? prefab = WorldManager.instance?.GetCardPrefab(saved.CardPrefabId, showError: false);
                if (prefab != null)
                {
                    int val = isCities ? prefab.CitiesValue : prefab.Value;
                    if (val > 0)
                    {
                        total += val;
                    }
                }
            }
            return total;
        }

        private static bool IsCompactedCardData(CardData cardData)
        {
            return cardData.LeftoverExtraData != null &&
                   cardData.LeftoverExtraData.Any(x => x.AttributeId == CompactedKey && x.BoolValue);
        }

        public static void CompactStack(GameCard card)
        {
            if (card == null || card.CardData == null) return;
            GameCard root = card.GetRootCard();
            if (root == null || root.CardData == null) return;

            List<GameCard> stack = root.GetAllCardsInStack();
            if (stack.Count <= 1 && !IsCompacted(root))
            {
                return;
            }

            Vector3 spawnPosition = root.transform.position;
            GameBoard? currentBoard = root.MyBoard ?? WorldManager.instance?.CurrentBoard;
            bool isCities = currentBoard != null && currentBoard.Location == Location.Cities;
            List<string> savedData = new List<string>();
            int totalValue = 0;

            foreach (GameCard c in stack)
            {
                if (c == null || c.CardData == null) continue;

                int val = isCities ? c.CardData.CitiesValue : c.CardData.GetValue();
                if (c.CardData.Id == "gold" || c.CardData.Id == "shell")
                {
                    totalValue += 1;
                }
                if (val > 0)
                {
                    totalValue += val;
                }

                savedData.Add(c.CardData.Id);
            }

            for (int i = stack.Count - 1; i >= 0; i--)
            {
                GameCard c = stack[i];
                if (c != null)
                {
                    c.RemoveFromStack();
                    c.DestroyCard(spawnSmoke: false, playSound: false);
                }
            }

            CompactCard? newCardData = WorldManager.instance?.CreateCard(
                spawnPosition, 
                "stackmenu_compactcard", 
                faceUp: true, 
                checkAddToStack: false, 
                playSound: false
            ) as CompactCard;

            if (newCardData != null && newCardData.MyGameCard != null)
            {
                GameCard newCard = newCardData.MyGameCard;
                newCard.MyBoard = currentBoard;

                if (isCities)
                {
                    newCardData.CitiesValue = totalValue;
                    newCardData.SavedCitiesValue = totalValue;
                }
                else
                {
                    newCardData.Value = totalValue;
                    newCardData.SavedValue = totalValue;
                }

                newCardData.SetCompactedData(savedData);
                newCardData.IconCardId = root.CardData.Id;
                newCardData.Icon = root.CardData.Icon;
                newCard.CardData.descriptionOverride = string.Join(" + ", savedData);

                newCard.UpdateIcon();
                UpdateBadge(newCard);
            }

            if (WorldManager.instance != null)
            {
                WorldManager.instance.CreateSmoke(spawnPosition);
            }
            if (AudioManager.me != null)
            {
                AudioManager.me.PlaySound2D(AudioManager.me.CardDrop, 1.2f, 0.4f);
            }
        }

        public static void UnpackStack(GameCard card)
        {
            if (card == null || card.CardData == null) return;
            GameCard root = card.GetRootCard();
            if (root == null || root.CardData == null) return;

            Vector3 spawnPos = root.transform.position;
            GameBoard? board = root.MyBoard ?? WorldManager.instance?.CurrentBoard;
            List<string> savedData = new List<string>();

            root.RemoveFromStack();
            root.DestroyCard(spawnSmoke: false, playSound: false);

            if (root.CardData is CompactCard compactCard)
            {
                savedData = compactCard.CompactedData;
            }

            if (savedData != null && savedData.Count > 0 && WorldManager.instance != null)
            {
                GameCard? previousCard = null;

                foreach (string saved in savedData)
                {
                    CardData createdCardData = WorldManager.instance.CreateCard(spawnPos, saved, true, checkAddToStack: false, playSound: false);
                    if (createdCardData == null || createdCardData.MyGameCard == null) continue;

                    if(previousCard != null)
                    {
                        createdCardData.MyGameCard.SetParent(previousCard);
                    }
                    previousCard = createdCardData.MyGameCard;
                }
            }

            if (WorldManager.instance != null)
            {
                WorldManager.instance.CreateSmoke(spawnPos);
            }
            if (AudioManager.me != null)
            {
                AudioManager.me.PlaySound2D(AudioManager.me.CardDrop, 0.9f, 0.4f);
            }
        }

        public static void UpdateBadge(GameCard card)
        {
            if (card == null) return;

            Transform badgeTransform = card.transform.Find("StackMenuBadge");
            bool pinned = IsPinned(card);
            bool compacted = IsCompacted(card);
            int compactCount = GetCompactedCount(card);

            if (!pinned && !compacted)
            {
                if (badgeTransform != null)
                {
                    UnityEngine.Object.Destroy(badgeTransform.gameObject);
                }
                return;
            }

            TextMeshPro tmp;
            if (badgeTransform == null)
            {
                GameObject badgeGO = new GameObject("StackMenuBadge");
                badgeGO.transform.SetParent(card.transform, false);

                tmp = badgeGO.AddComponent<TextMeshPro>();
                if (card.CardNameText != null)
                {
                    tmp.font = card.CardNameText.font;
                    tmp.fontSize = card.CardNameText.fontSize * 0.2f;
                }
                tmp.alignment = TextAlignmentOptions.TopGeoAligned;
                tmp.color = new Color(1f, 0.85f, 0.2f, 1f);
                tmp.rectTransform.sizeDelta = new Vector2(1f, 0.5f);
                tmp.rectTransform.localPosition = new Vector3(0f, 0.1f, -0.01f);
                tmp.rectTransform.localRotation = Quaternion.identity;
            }
            else
            {
                tmp = badgeTransform.GetComponent<TextMeshPro>();
            }

            string text = "";
            if (pinned && compacted)
            {
                text = $"[•] [x{compactCount}]";
            }
            else if (pinned)
            {
                text = "[•]";
            }
            else if (compacted)
            {
                text = $"[ ] [x{compactCount}]";
            }

            if (tmp != null)
            {
                tmp.text = text;
            }
        }

        public static void UpdateAll()
        {
            if (WorldManager.instance == null || WorldManager.instance.AllCards == null) return;

            var allCards = WorldManager.instance.AllCards;

            if (pinnedCards.Count > 0)
            {
                var dead = pinnedCards.Keys.Where(c => c == null || c.Destroyed).ToList();
                foreach (var d in dead)
                {
                    pinnedCards.Remove(d);
                }
            }

            for (int i = 0; i < allCards.Count; i++)
            {
                GameCard card = allCards[i];
                if (card == null || card.Destroyed || card.CardData == null) continue;

                if (!knownCards.Contains(card))
                {
                    knownCards.Add(card);

                    if (IsPinned(card))
                    {
                        float? px = GetExtraDataFloat(card.CardData, PinnedPosXKey);
                        float? py = GetExtraDataFloat(card.CardData, PinnedPosYKey);
                        float? pz = GetExtraDataFloat(card.CardData, PinnedPosZKey);

                        Vector3 savedPos = (px.HasValue && py.HasValue && pz.HasValue)
                            ? new Vector3(px.Value, py.Value, pz.Value)
                            : card.transform.position;

                        card.PushEnabled = false;
                        pinnedCards[card] = savedPos;
                        UpdateBadge(card);
                    }
                    else if (IsCompacted(card))
                    {
                        if (card.CardData is CompactCard compactCard)
                        {
                            compactCard.UpdateVisuals();
                        }
                        UpdateBadge(card);
                    }
                }

                if (IsPinned(card))
                {
                    if (card.PushEnabled)
                    {
                        card.PushEnabled = false;
                    }

                    if (card.BeingDragged)
                    {
                        Vector3 currentPos = card.transform.position;
                        pinnedCards[card] = currentPos;
                        SetExtraData(card.CardData, PinnedPosXKey, currentPos.x);
                        SetExtraData(card.CardData, PinnedPosYKey, currentPos.y);
                        SetExtraData(card.CardData, PinnedPosZKey, currentPos.z);
                    }
                    else if (pinnedCards.TryGetValue(card, out Vector3 targetPos))
                    {
                        if (card.Velocity.HasValue && card.Velocity.Value.sqrMagnitude > 0.001f)
                        {
                            card.Velocity = Vector3.zero;
                        }
                        if ((card.transform.position - targetPos).sqrMagnitude > 0.0001f)
                        {
                            card.transform.position = Vector3.Lerp(card.transform.position, targetPos, Time.deltaTime * 20f);
                        }
                    }
                }
            }

            if (knownCards.Count > allCards.Count + 100)
            {
                knownCards.RemoveWhere(c => c == null || c.Destroyed);
            }
        }

        #region ExtraData Helpers
        public static ExtraCardData? GetExtraData(CardData cardData, string attributeId)
        {
            return cardData.LeftoverExtraData?.FirstOrDefault(x => x.AttributeId == attributeId);
        }

        public static float? GetExtraDataFloat(CardData cardData, string attributeId)
        {
            var extra = GetExtraData(cardData, attributeId);
            return extra?.FloatValue;
        }

        public static void SetExtraData(CardData cardData, string attributeId, bool value)
        {
            if (cardData.LeftoverExtraData == null) cardData.LeftoverExtraData = new List<ExtraCardData>();
            cardData.LeftoverExtraData.RemoveAll(x => x.AttributeId == attributeId);
            cardData.LeftoverExtraData.Add(new ExtraCardData(attributeId, value));
        }

        public static void SetExtraData(CardData cardData, string attributeId, string value)
        {
            if (cardData.LeftoverExtraData == null) cardData.LeftoverExtraData = new List<ExtraCardData>();
            cardData.LeftoverExtraData.RemoveAll(x => x.AttributeId == attributeId);
            cardData.LeftoverExtraData.Add(new ExtraCardData(attributeId, value));
        }

        public static void SetExtraData(CardData cardData, string attributeId, float value)
        {
            if (cardData.LeftoverExtraData == null) cardData.LeftoverExtraData = new List<ExtraCardData>();
            cardData.LeftoverExtraData.RemoveAll(x => x.AttributeId == attributeId);
            cardData.LeftoverExtraData.Add(new ExtraCardData(attributeId, value));
        }

        public static void SetExtraData(CardData cardData, string attributeId, int value)
        {
            if (cardData.LeftoverExtraData == null) cardData.LeftoverExtraData = new List<ExtraCardData>();
            cardData.LeftoverExtraData.RemoveAll(x => x.AttributeId == attributeId);
            cardData.LeftoverExtraData.Add(new ExtraCardData(attributeId, value));
        }

        public static void RemoveExtraData(CardData cardData, string attributeId)
        {
            cardData.LeftoverExtraData?.RemoveAll(x => x.AttributeId == attributeId);
        }
        #endregion
    }

    [HarmonyPatch(typeof(CardData), nameof(CardData.GetValue))]
    public static class CardData_GetValue_Patch
    {
        public static void Postfix(CardData __instance, ref int __result)
        {
            if (__result != -1)
            {
                int extra = CardMenu.GetCompactedExtraValue(__instance);
                if (extra > 0)
                {
                    __result += extra;
                }
            }
        }
    }
}