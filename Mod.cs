using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace StackMenu
{
    public class StackMenuMod : Mod
    {
        private readonly CardStackMenu menu = new CardStackMenu();
        public static ConfigEntry<bool> AddCardToBottomConfig;

        public static ConfigEntry<string> pin_unpin_shortcut;
        public static ConfigEntry<string> compact_uncompact_shortcut;
        public static ConfigEntry<string> sort_by_name_shortcut;
        public static ConfigEntry<string> sort_by_value_shortcut;
        public static ConfigEntry<string> sort_by_type_shortcut;
        public static ConfigEntry<string> slip_all_card_shortcut;
        public static ConfigEntry<string> slip_all_card_to_stack_shortcut;

        public static Key pin_unpin_key => KeybindSettingUI.ParseKey(pin_unpin_shortcut, Key.P);
        public static Key compact_uncompact_key => KeybindSettingUI.ParseKey(compact_uncompact_shortcut, Key.C);
        public static Key sort_by_name_key => KeybindSettingUI.ParseKey(sort_by_name_shortcut, Key.N);
        public static Key sort_by_value_key => KeybindSettingUI.ParseKey(sort_by_value_shortcut, Key.V);
        public static Key sort_by_type_key => KeybindSettingUI.ParseKey(sort_by_type_shortcut, Key.T);
        public static Key slip_all_card_key => KeybindSettingUI.ParseKey(slip_all_card_shortcut, Key.S);
        public static Key slip_all_card_to_stack_key => KeybindSettingUI.ParseKey(slip_all_card_to_stack_shortcut, Key.A);

        public override void Ready()
        {
            AddCardToBottomConfig = Config.GetEntry<bool>("add_card_to_bottom", false);
            AddCardToBottomConfig.UI.Name = "Add Card to Bottom of Stack";
            AddCardToBottomConfig.UI.Tooltip = "Currently disabled";

            pin_unpin_shortcut = SetupKeybindConfig("pin_unpin_shortcut", "Pin/Unpin Card", "P");
            compact_uncompact_shortcut = SetupKeybindConfig("compact_uncompact_shortcut", "Compact/Unpack Card", "C");
            sort_by_name_shortcut = SetupKeybindConfig("sort_by_name_shortcut", "Sort by Name", "N");
            sort_by_value_shortcut = SetupKeybindConfig("sort_by_value_shortcut", "Sort by Value", "V");
            sort_by_type_shortcut = SetupKeybindConfig("sort_by_type_shortcut", "Sort by Type", "T");
            slip_all_card_shortcut = SetupKeybindConfig("slip_all_card_shortcut", "Split All Cards", "S");
            slip_all_card_to_stack_shortcut = SetupKeybindConfig("slip_all_card_to_stack_shortcut", "Split All to Each Stack", "A");

            try
            {
                Harmony.PatchAll();
                Logger.Log("StackMenuMod Harmony patches applied.");
            }
            catch (Exception ex)
            {
                Logger.Log($"StackMenuMod failed to apply Harmony patches: {ex.Message}");
            }
            Logger.Log("StackMenuMod Ready called.");
        }

        private ConfigEntry<string> SetupKeybindConfig(string name, string displayName, string defaultKey)
        {
            var entry = Config.GetEntry<string>(name, defaultKey);
            entry.UI.Name = displayName;
            entry.UI.Hidden = true;
            entry.UI.OnUI = (ConfigEntryBase entryBase) =>
            {
                if (PrefabManager.instance == null || ModOptionsScreen.instance == null) return;

                var btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                btn.transform.localScale = Vector3.one;
                btn.transform.localPosition = Vector3.zero;
                btn.transform.localRotation = Quaternion.identity;

                var keybindUI = btn.gameObject.AddComponent<KeybindSettingUI>();
                Key current = KeybindSettingUI.ParseKey(entry, Key.None);
                keybindUI.Initialize(current, displayName, (newKey) =>
                {
                    entry.Value = newKey.ToString();
                });
            };
            return entry;
        }

        private void Update()
        {
            if (WorldManager.instance == null)
            {
                return;
            }

            // Keep pinned cards anchored and badges synchronized
            CardMenu.UpdateAll();

            if (menu.IsOpen && menu.UpdateInput())
            {
                return;
            }

            if (Mouse.current == null)
            {
                return;
            }

            // InputController only tracks the left mouse button by default, so read the right/left buttons directly.
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                GameCard hoveredCard = WorldManager.instance.HoveredCard;
                if (hoveredCard != null && hoveredCard.CardData != null)
                {
                    var stack = hoveredCard.GetAllCardsInStack();
                    var items = new List<MenuItem>();

                    // --- Card Menu (Top Section) ---
                    bool isPinned = CardMenu.IsPinned(hoveredCard);
                    if (isPinned)
                    {
                        items.Add(MenuItem.Action("Unpin Card", () => CardMenu.UnpinCard(hoveredCard), pin_unpin_key));
                    }
                    else
                    {
                        items.Add(MenuItem.Action("Pin Card", () => CardMenu.PinCard(hoveredCard), pin_unpin_key));
                    }

                    if (CardMenu.IsCompacted(hoveredCard))
                    {
                        items.Add(MenuItem.Action("Unpack", () => CardMenu.UnpackStack(hoveredCard), compact_uncompact_key));
                    }
                    else if (stack != null && stack.Count > 1)
                    {
                        items.Add(MenuItem.Action("Compact Stack", () => CardMenu.CompactStack(hoveredCard), compact_uncompact_key));
                    }

                    // --- Stack Menu (Bottom Section) ---
                    if (stack != null && stack.Count > 1)
                    {
                        // Separator line between Card Menu and Stack Menu
                        items.Add(MenuItem.Separator());

                        items.Add(MenuItem.Action("Sort by Name", () => StackSorter.Sort(hoveredCard, StackSortOption.Name), sort_by_name_key));
                        items.Add(MenuItem.Action("Sort by Value", () => StackSorter.Sort(hoveredCard, StackSortOption.Value), sort_by_value_key));
                        items.Add(MenuItem.Action("Sort by Type", () => StackSorter.Sort(hoveredCard, StackSortOption.Type), sort_by_type_key));
                        items.Add(MenuItem.Action("Split All to Each Stack", () => StackSorter.SplitAllToEachStack(hoveredCard), slip_all_card_to_stack_key));
                        items.Add(MenuItem.Action("Split All", () => StackSorter.SplitAll(hoveredCard), slip_all_card_key));
                    }

                    if (items.Count > 0)
                    {
                        menu.Show(items, Mouse.current.position.ReadValue());
                    }
                }
            }
            else if (menu.IsOpen && Mouse.current.leftButton.wasPressedThisFrame
                && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                // Only treat clicks outside the menu's own buttons as "close"; button clicks close themselves after acting.
                menu.Hide();
            }
        }
    }

    // [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CheckIfCanAddOnStack))]
    // public static class WorldManager_CheckIfCanAddOnStack_Patch
    // {
    //     public static bool Prefix(WorldManager __instance, GameCard topCard, ref bool __result)
    //     {
    //         if (StackMenuMod.AddCardToBottomConfig != null && StackMenuMod.AddCardToBottomConfig.Value)
    //         {
    //             // Bottom insertion mode: topCard will become the parent of the root of the target stack
    //             List<GameCard> overlappingCards = topCard.GetOverlappingCards();
    //             float num = float.MaxValue;
    //             GameCard targetRoot = null;

    //             foreach (GameCard item in overlappingCards)
    //             {
    //                 if (item == topCard || item.IsChildOf(topCard))
    //                 {
    //                     continue;
    //                 }

    //                 GameCard rootCard = item.GetRootCard();
    //                 if (rootCard == topCard || rootCard.IsChildOf(topCard))
    //                 {
    //                     continue;
    //                 }

    //                 // Check if topCard (or bottom of topCard stack) can have rootCard on top
    //                 GameCard topCardLeaf = topCard.GetLeafCard();
    //                 GameCard cardWithStatus = rootCard.GetCardWithStatusInStack();
    //                 if (cardWithStatus != null && !cardWithStatus.CardData.CanHaveCardsWhileHasStatus())
    //                 {
    //                     continue;
    //                 }

    //                 if (topCardLeaf.CardData.CanHaveCardOnTop(rootCard.CardData))
    //                 {
    //                     Vector3 vector = topCard.transform.position - item.transform.position;
    //                     vector.y = 0f;
    //                     if (vector.magnitude < num)
    //                     {
    //                         targetRoot = rootCard;
    //                         num = vector.magnitude;
    //                     }
    //                 }
    //             }

    //             if (targetRoot != null)
    //             {
    //                 GameCard topCardLeaf = topCard.GetLeafCard();
    //                 targetRoot.SetParent(topCardLeaf);
    //                 __result = true;
    //                 return false;
    //             }

    //             __result = false;
    //             return false;
    //         }
    //     }
    // }
    //         }

    //         // Default top insertion mode (vanilla behavior)
    //         List<GameCard> defaultOverlappingCards = topCard.GetOverlappingCards();
    //         float defaultNum = float.MaxValue;
    //         GameCard defaultGameCard = null;

    //         foreach (GameCard item in defaultOverlappingCards)
    //         {
    //             if (item == topCard || item.IsChildOf(topCard))
    //             {
    //                 continue;
    //             }
    //             bool num2 = topCard == item.removedChild;
    //             GameCard leafCard = item.GetLeafCard();
    //             if (!num2)
    //             {
    //                 GameCard cardWithStatusInStack = leafCard.GetCardWithStatusInStack();
    //                 if (cardWithStatusInStack != null && !cardWithStatusInStack.CardData.CanHaveCardsWhileHasStatus())
    //                 {
    //                     continue;
    //                 }
    //             }
    //             if (leafCard.CardData.CanHaveCardOnTop(topCard.CardData))
    //             {
    //                 Vector3 vector = topCard.transform.position - item.transform.position;
    //                 vector.y = 0f;
    //                 if (vector.magnitude < defaultNum)
    //                 {
    //                     defaultGameCard = leafCard;
    //                     defaultNum = vector.magnitude;
    //                 }
    //             }
    //         }

    //         if (defaultGameCard != null)
    //         {
    //             topCard.SetParent(defaultGameCard);
    //             __result = true;
    //             return false;
    //         }

    //         __result = false;
    //         return false;
    //     }
    // }

    // [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CreateCardStack))]
    // public static class WorldManager_CreateCardStack_Patch
    // {
    //     public static bool Prefix(WorldManager __instance, Vector3 pos, int amount, string cardId, bool checkAddToStack, ref GameCard __result)
    //     {
    //         if (amount == 0)
    //         {
    //             __result = null;
    //             return false;
    //         }

    //         GameCard gameCard = null;
    //         while (amount > 0)
    //         {
    //             int num = Mathf.Min(amount, 10);
    //             gameCard = null;
    //             for (int i = 0; i < num; i++)
    //             {
    //                 GameCard myGameCard = __instance.CreateCard(pos, cardId, faceUp: true, checkAddToStack).MyGameCard;
    //                 if (gameCard != null)
    //                 {
    //                     gameCard.SetParent(myGameCard);
    //                 }
    //                 gameCard = myGameCard;
    //             }
    //             amount -= num;
    //         }

    //         __result = gameCard;
    //         return false;
    //     }
    // }
}
