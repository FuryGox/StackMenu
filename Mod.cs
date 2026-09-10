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

        public override void Ready()
        {
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

        private void Update()
        {
            if (WorldManager.instance == null)
            {
                return;
            }

            // Keep pinned cards anchored and badges synchronized
            CardMenu.UpdateAll();

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
                        items.Add(MenuItem.Action("Unpin Card", () => CardMenu.UnpinCard(hoveredCard)));
                    }
                    else
                    {
                        items.Add(MenuItem.Action("Pin Card", () => CardMenu.PinCard(hoveredCard)));
                    }

                    if (CardMenu.IsCompacted(hoveredCard))
                    {
                        items.Add(MenuItem.Action("Unpack", () => CardMenu.UnpackStack(hoveredCard)));
                    }
                    else if (stack != null && stack.Count > 1)
                    {
                        items.Add(MenuItem.Action("Compact Stack", () => CardMenu.CompactStack(hoveredCard)));
                    }

                    // --- Stack Menu (Bottom Section) ---
                    if (stack != null && stack.Count > 1)
                    {
                        // Separator line between Card Menu and Stack Menu
                        items.Add(MenuItem.Separator());

                        items.Add(MenuItem.Action("Sort by Name", () => StackSorter.Sort(hoveredCard, StackSortOption.Name)));
                        items.Add(MenuItem.Action("Sort by Value", () => StackSorter.Sort(hoveredCard, StackSortOption.Value)));
                        items.Add(MenuItem.Action("Sort by Type", () => StackSorter.Sort(hoveredCard, StackSortOption.Type)));
                        items.Add(MenuItem.Action("Split All to Each Stack", () => StackSorter.SplitAllToEachStack(hoveredCard)));
                        items.Add(MenuItem.Action("Split All", () => StackSorter.SplitAll(hoveredCard)));
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
}
