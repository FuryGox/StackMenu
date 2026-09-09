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
            Logger.Log("StackMenuMod Ready called.");
        }

        private void Update()
        {
            if (WorldManager.instance == null || Mouse.current == null)
            {
                return;
            }

            // InputController only tracks the left mouse button by default, so read the right/left buttons directly.
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                GameCard hoveredCard = WorldManager.instance.HoveredCard;
                if (hoveredCard != null)
                {
                    var stack = hoveredCard.GetAllCardsInStack();
                    if (stack.Count > 1)
                    {
                        var options = new List<(string Label, Action OnClick)>
                        {
                            ("Sort by Name", () => { StackSorter.Sort(hoveredCard, StackSortOption.Name); menu.Hide(); }),
                            ("Sort by Value", () => { StackSorter.Sort(hoveredCard, StackSortOption.Value); menu.Hide(); }),
                            ("Sort by Type", () => { StackSorter.Sort(hoveredCard, StackSortOption.Type); menu.Hide(); }),
                            ("Split All to Each Stack", () => { StackSorter.SplitAllToEachStack(hoveredCard); menu.Hide(); }),
                            ("Split All", () => { StackSorter.SplitAll(hoveredCard); menu.Hide(); }),
                        };
                        menu.Show(options, Mouse.current.position.ReadValue());
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
