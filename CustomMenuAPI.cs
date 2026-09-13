using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StackMenu
{
    /// <summary>
    /// Represents a custom menu item or separator that can be registered by external mods.
    /// </summary>
    public class CustomMenuItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Label { get; set; } = "";
        public Func<GameCard, string>? DynamicLabel { get; set; }
        public Action<GameCard>? OnClick { get; set; }

        /// <summary>
        /// Condition function evaluated on the hovered card. If null or returns true, the item is displayed.
        /// </summary>
        public Func<GameCard, bool>? Condition { get; set; }

        public bool IsSeparator { get; set; }

        /// <summary>
        /// Shortcut key triggered when the menu is open.
        /// </summary>
        public Key? ShortcutKey { get; set; }
        public Func<Key?>? DynamicShortcutKey { get; set; }

        /// <summary>
        /// Display text for the shortcut key (e.g. "K", "Ctrl+S"). If null, defaults to ShortcutKey.ToString().
        /// </summary>
        public string? ShortcutLabel { get; set; }
        public Func<string?>? DynamicShortcutLabel { get; set; }

        /// <summary>
        /// Sorting priority. Higher priority items appear earlier. Default is 0.
        /// </summary>
        public int Priority { get; set; } = 0;

        public static CustomMenuItem CreateAction(
            string label,
            Action<GameCard> onClick,
            Func<GameCard, bool>? condition = null,
            Key? shortcutKey = null,
            string? shortcutLabel = null,
            int priority = 0)
        {
            return new CustomMenuItem
            {
                Label = label,
                OnClick = onClick,
                Condition = condition,
                ShortcutKey = shortcutKey,
                ShortcutLabel = shortcutLabel,
                Priority = priority,
                IsSeparator = false
            };
        }

        public static CustomMenuItem CreateAction(
            Func<GameCard, string> dynamicLabel,
            Action<GameCard> onClick,
            Func<GameCard, bool>? condition = null,
            Key? shortcutKey = null,
            string? shortcutLabel = null,
            int priority = 0)
        {
            return new CustomMenuItem
            {
                DynamicLabel = dynamicLabel,
                OnClick = onClick,
                Condition = condition,
                ShortcutKey = shortcutKey,
                ShortcutLabel = shortcutLabel,
                Priority = priority,
                IsSeparator = false
            };
        }

        public static CustomMenuItem CreateAction(
            string label,
            Action<GameCard> onClick,
            Func<GameCard, bool>? condition,
            Func<Key?> dynamicShortcutKey,
            Func<string?>? dynamicShortcutLabel = null,
            int priority = 0)
        {
            return new CustomMenuItem
            {
                Label = label,
                OnClick = onClick,
                Condition = condition,
                DynamicShortcutKey = dynamicShortcutKey,
                DynamicShortcutLabel = dynamicShortcutLabel,
                Priority = priority,
                IsSeparator = false
            };
        }

        public static CustomMenuItem CreateAction(
            Func<GameCard, string> dynamicLabel,
            Action<GameCard> onClick,
            Func<GameCard, bool>? condition,
            Func<Key?> dynamicShortcutKey,
            Func<string?>? dynamicShortcutLabel = null,
            int priority = 0)
        {
            return new CustomMenuItem
            {
                DynamicLabel = dynamicLabel,
                OnClick = onClick,
                Condition = condition,
                DynamicShortcutKey = dynamicShortcutKey,
                DynamicShortcutLabel = dynamicShortcutLabel,
                Priority = priority,
                IsSeparator = false
            };
        }

        public static CustomMenuItem CreateSeparator(Func<GameCard, bool>? condition = null, int priority = 0)
        {
            return new CustomMenuItem
            {
                IsSeparator = true,
                Condition = condition,
                Priority = priority
            };
        }

        public MenuItem? ToMenuItem(GameCard card)
        {
            if (Condition != null)
            {
                try
                {
                    if (!Condition(card)) return null;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[StackMenu] Error evaluating condition for custom menu item '{Label}': {ex}");
                    return null;
                }
            }

            if (IsSeparator)
            {
                return MenuItem.Separator();
            }

            string text = DynamicLabel != null ? DynamicLabel(card) : Label;
            Key? key = DynamicShortcutKey != null ? DynamicShortcutKey() : ShortcutKey;
            string? shortLabel = DynamicShortcutLabel != null ? DynamicShortcutLabel() : ShortcutLabel;

            return MenuItem.Action(text, () =>
            {
                try
                {
                    OnClick?.Invoke(card);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[StackMenu] Error executing custom menu item '{text}': {ex}");
                }
            }, key, shortLabel);
        }
    }

    /// <summary>
    /// Public API for other mods to register custom actions, conditions, shortcut keys, and separators into the StackMenu context menu.
    /// </summary>
    public static class CustomMenuAPI
    {
        private static readonly List<CustomMenuItem> registeredItems = new List<CustomMenuItem>();
        private static readonly List<Func<GameCard, IEnumerable<CustomMenuItem>>> registeredCustomProviders = new List<Func<GameCard, IEnumerable<CustomMenuItem>>>();
        private static readonly List<Func<GameCard, IEnumerable<MenuItem>>> registeredMenuProviders = new List<Func<GameCard, IEnumerable<MenuItem>>>();

        /// <summary>
        /// Registers a custom menu item.
        /// </summary>
        public static CustomMenuItem RegisterItem(CustomMenuItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            lock (registeredItems)
            {
                registeredItems.Add(item);
            }
            return item;
        }

        /// <summary>
        /// Registers a custom action to appear in the card context menu.
        /// </summary>
        /// <param name="label">Text displayed in the menu button.</param>
        /// <param name="onClick">Callback invoked when the item is clicked or shortcut is pressed, receives the hovered GameCard.</param>
        /// <param name="condition">Predicate to determine if this action is shown for the given GameCard. If null, always shown.</param>
        /// <param name="shortcutKey">Optional shortcut key to trigger this action while the menu is open.</param>
        /// <param name="shortcutLabel">Optional custom shortcut label (defaults to shortcutKey name).</param>
        /// <param name="priority">Ordering priority. Higher numbers appear first.</param>
        public static CustomMenuItem RegisterAction(
            string label,
            Action<GameCard> onClick,
            Func<GameCard, bool>? condition = null,
            Key? shortcutKey = null,
            string? shortcutLabel = null,
            int priority = 0)
        {
            var item = CustomMenuItem.CreateAction(label, onClick, condition, shortcutKey, shortcutLabel, priority);
            return RegisterItem(item);
        }

        /// <summary>
        /// Registers a custom action with a dynamic label.
        /// </summary>
        public static CustomMenuItem RegisterAction(
            Func<GameCard, string> dynamicLabel,
            Action<GameCard> onClick,
            Func<GameCard, bool>? condition = null,
            Key? shortcutKey = null,
            string? shortcutLabel = null,
            int priority = 0)
        {
            var item = CustomMenuItem.CreateAction(dynamicLabel, onClick, condition, shortcutKey, shortcutLabel, priority);
            return RegisterItem(item);
        }

        /// <summary>
        /// Registers a custom action with a dynamic shortcut key provider.
        /// </summary>
        public static CustomMenuItem RegisterAction(
            string label,
            Action<GameCard> onClick,
            Func<GameCard, bool>? condition,
            Func<Key?> dynamicShortcutKey,
            Func<string?>? dynamicShortcutLabel = null,
            int priority = 0)
        {
            var item = CustomMenuItem.CreateAction(label, onClick, condition, dynamicShortcutKey, dynamicShortcutLabel, priority);
            return RegisterItem(item);
        }

        /// <summary>
        /// Registers a custom action with dynamic label and dynamic shortcut key.
        /// </summary>
        public static CustomMenuItem RegisterAction(
            Func<GameCard, string> dynamicLabel,
            Action<GameCard> onClick,
            Func<GameCard, bool>? condition,
            Func<Key?> dynamicShortcutKey,
            Func<string?>? dynamicShortcutLabel = null,
            int priority = 0)
        {
            var item = CustomMenuItem.CreateAction(dynamicLabel, onClick, condition, dynamicShortcutKey, dynamicShortcutLabel, priority);
            return RegisterItem(item);
        }

        /// <summary>
        /// Registers a separator line in the menu with an optional condition and priority.
        /// </summary>
        /// <param name="condition">Optional condition to display the separator.</param>
        /// <param name="priority">Ordering priority.</param>
        public static CustomMenuItem RegisterSeparator(Func<GameCard, bool>? condition = null, int priority = 0)
        {
            var item = CustomMenuItem.CreateSeparator(condition, priority);
            return RegisterItem(item);
        }

        /// <summary>
        /// Registers a dynamic provider callback that generates custom menu items on the fly for any card.
        /// </summary>
        public static void RegisterProvider(Func<GameCard, IEnumerable<CustomMenuItem>> provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            lock (registeredCustomProviders)
            {
                registeredCustomProviders.Add(provider);
            }
        }

        /// <summary>
        /// Registers a dynamic provider callback that generates MenuItem instances directly.
        /// </summary>
        public static void RegisterProvider(Func<GameCard, IEnumerable<MenuItem>> provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            lock (registeredMenuProviders)
            {
                registeredMenuProviders.Add(provider);
            }
        }

        /// <summary>
        /// Unregisters a previously registered CustomMenuItem.
        /// </summary>
        public static bool UnregisterItem(CustomMenuItem item)
        {
            if (item == null) return false;
            lock (registeredItems)
            {
                return registeredItems.Remove(item);
            }
        }

        /// <summary>
        /// Unregisters a CustomMenuItem by its Id.
        /// </summary>
        public static bool UnregisterItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            lock (registeredItems)
            {
                return registeredItems.RemoveAll(x => x.Id == id) > 0;
            }
        }

        /// <summary>
        /// Unregisters a dynamic CustomMenuItem provider callback.
        /// </summary>
        public static bool UnregisterProvider(Func<GameCard, IEnumerable<CustomMenuItem>> provider)
        {
            if (provider == null) return false;
            lock (registeredCustomProviders)
            {
                return registeredCustomProviders.Remove(provider);
            }
        }

        /// <summary>
        /// Unregisters a dynamic MenuItem provider callback.
        /// </summary>
        public static bool UnregisterProvider(Func<GameCard, IEnumerable<MenuItem>> provider)
        {
            if (provider == null) return false;
            lock (registeredMenuProviders)
            {
                return registeredMenuProviders.Remove(provider);
            }
        }

        /// <summary>
        /// Creates a keybind setting button in Mod Options for a mod config entry.
        /// </summary>
        public static ConfigEntry<string> SetupKeybindConfig(
            ConfigFile config,
            string name,
            string displayName,
            string tooltip,
            string defaultKey = "None",
            Action<Key>? onKeyChanged = null)
        {
            var entry = config.GetEntry<string>(name, defaultKey);
            entry.UI.Name = displayName;
            entry.UI.Hidden = true;
            entry.UI.Tooltip = tooltip;
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
                    onKeyChanged?.Invoke(newKey);
                });
            };
            return entry;
        }

        public static ConfigEntry<string> SetupKeybindConfig(
            Mod mod,
            string name,
            string displayName,
            string tooltip,
            string defaultKey = "None",
            Action<Key>? onKeyChanged = null)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));
            return SetupKeybindConfig(mod.Config, name, displayName, tooltip, defaultKey, onKeyChanged);
        }

        public static ConfigEntry<string> SetupKeybindConfig(
            ConfigFile config,
            string name,
            string displayName,
            string tooltip,
            Key defaultKey,
            Action<Key>? onKeyChanged = null)
        {
            return SetupKeybindConfig(config, name, displayName, tooltip, defaultKey.ToString(), onKeyChanged);
        }

        public static ConfigEntry<string> SetupKeybindConfig(
            Mod mod,
            string name,
            string displayName,
            string tooltip,
            Key defaultKey,
            Action<Key>? onKeyChanged = null)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));
            return SetupKeybindConfig(mod.Config, name, displayName, tooltip, defaultKey.ToString(), onKeyChanged);
        }

        /// <summary>
        /// Creates a color palette picker button in Mod Options for a mod config entry.
        /// </summary>
        public static ConfigEntry<string> SetupColorConfig(
            ConfigFile config,
            string name,
            string displayName,
            string tooltip,
            string defaultColorHex = "#FFFFFF",
            Action<Color>? onColorChanged = null)
        {
            var entry = config.GetEntry<string>(name, defaultColorHex);
            entry.UI.Name = displayName;
            entry.UI.Hidden = true;
            entry.UI.Tooltip = tooltip;
            entry.UI.OnUI = (ConfigEntryBase entryBase) =>
            {
                if (PrefabManager.instance == null || ModOptionsScreen.instance == null) return;

                var btn = UnityEngine.Object.Instantiate(PrefabManager.instance.ButtonPrefab, ModOptionsScreen.instance.ButtonsParent);
                btn.transform.localScale = Vector3.one;
                btn.transform.localPosition = Vector3.zero;
                btn.transform.localRotation = Quaternion.identity;

                var colorUI = btn.gameObject.AddComponent<ColorSettingUI>();
                Color current = ColorSettingUI.ParseColor(entry, Color.white);
                colorUI.Initialize(current, displayName, (newColor) =>
                {
                    entry.Value = ColorSettingUI.ColorToHex(newColor);
                    onColorChanged?.Invoke(newColor);
                });
            };
            return entry;
        }

        public static ConfigEntry<string> SetupColorConfig(
            Mod mod,
            string name,
            string displayName,
            string tooltip,
            string defaultColorHex = "#FFFFFF",
            Action<Color>? onColorChanged = null)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));
            return SetupColorConfig(mod.Config, name, displayName, tooltip, defaultColorHex, onColorChanged);
        }

        public static ConfigEntry<string> SetupColorConfig(
            ConfigFile config,
            string name,
            string displayName,
            string tooltip,
            Color defaultColor,
            Action<Color>? onColorChanged = null)
        {
            return SetupColorConfig(config, name, displayName, tooltip, ColorSettingUI.ColorToHex(defaultColor), onColorChanged);
        }

        public static ConfigEntry<string> SetupColorConfig(
            Mod mod,
            string name,
            string displayName,
            string tooltip,
            Color defaultColor,
            Action<Color>? onColorChanged = null)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));
            return SetupColorConfig(mod.Config, name, displayName, tooltip, ColorSettingUI.ColorToHex(defaultColor), onColorChanged);
        }

        /// <summary>
        /// Parses a Keybind string config into a Key enum value.
        /// </summary>
        public static Key ParseKey(ConfigEntry<string>? config, Key defaultKey = Key.None)
            => KeybindSettingUI.ParseKey(config, defaultKey);

        /// <summary>
        /// Parses a Hex color string config into a Unity Color.
        /// </summary>
        public static Color ParseColor(ConfigEntry<string>? config, Color defaultColor = default)
            => ColorSettingUI.ParseColor(config, defaultColor);

        /// <summary>
        /// Converts a Unity Color to Hex string format (#RRGGBBAA).
        /// </summary>
        public static string ColorToHex(Color color)
            => ColorSettingUI.ColorToHex(color);

        /// <summary>
        /// Clears all registered custom items and providers.
        /// </summary>
        public static void Clear()
        {
            lock (registeredItems) { registeredItems.Clear(); }
            lock (registeredCustomProviders) { registeredCustomProviders.Clear(); }
            lock (registeredMenuProviders) { registeredMenuProviders.Clear(); }
        }

        /// <summary>
        /// Evaluates all registered items and providers for the given card, returning a list of ready MenuItems.
        /// </summary>
        public static List<MenuItem> GetCustomMenuItems(GameCard card)
        {
            if (card == null) return new List<MenuItem>();

            var allCustomItems = new List<CustomMenuItem>();

            lock (registeredItems)
            {
                allCustomItems.AddRange(registeredItems);
            }

            lock (registeredCustomProviders)
            {
                foreach (var provider in registeredCustomProviders)
                {
                    try
                    {
                        var provided = provider(card);
                        if (provided != null)
                        {
                            allCustomItems.AddRange(provided);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[StackMenu] Error in custom item provider: {ex}");
                    }
                }
            }

            // Sort by priority descending (stable order preserved for same priority)
            var sorted = allCustomItems.OrderByDescending(x => x.Priority);

            var result = new List<MenuItem>();
            foreach (var custom in sorted)
            {
                var menuItem = custom.ToMenuItem(card);
                if (menuItem != null)
                {
                    result.Add(menuItem);
                }
            }

            lock (registeredMenuProviders)
            {
                foreach (var provider in registeredMenuProviders)
                {
                    try
                    {
                        var provided = provider(card);
                        if (provided != null)
                        {
                            result.AddRange(provided.Where(x => x != null));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[StackMenu] Error in MenuItem provider: {ex}");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Cleans up a list of MenuItems by removing leading, trailing, and duplicate consecutive separators.
        /// </summary>
        public static List<MenuItem> SanitizeMenuItems(List<MenuItem> items)
        {
            if (items == null) return new List<MenuItem>();
            var result = new List<MenuItem>();
            foreach (var item in items)
            {
                if (item == null) continue;
                if (item.IsSeparator)
                {
                    if (result.Count == 0 || result[result.Count - 1].IsSeparator)
                        continue;
                }
                result.Add(item);
            }
            if (result.Count > 0 && result[result.Count - 1].IsSeparator)
            {
                result.RemoveAt(result.Count - 1);
            }
            return result;
        }
    }

    /// <summary>
    /// Alias for CustomMenuAPI for ease of discovery.
    /// </summary>
    public static class StackMenuAPI
    {
        public static CustomMenuItem RegisterAction(string label, Action<GameCard> onClick, Func<GameCard, bool>? condition = null, Key? shortcutKey = null, string? shortcutLabel = null, int priority = 0)
            => CustomMenuAPI.RegisterAction(label, onClick, condition, shortcutKey, shortcutLabel, priority);

        public static CustomMenuItem RegisterAction(Func<GameCard, string> dynamicLabel, Action<GameCard> onClick, Func<GameCard, bool>? condition = null, Key? shortcutKey = null, string? shortcutLabel = null, int priority = 0)
            => CustomMenuAPI.RegisterAction(dynamicLabel, onClick, condition, shortcutKey, shortcutLabel, priority);

        public static CustomMenuItem RegisterAction(string label, Action<GameCard> onClick, Func<GameCard, bool>? condition, Func<Key?> dynamicShortcutKey, Func<string?>? dynamicShortcutLabel = null, int priority = 0)
            => CustomMenuAPI.RegisterAction(label, onClick, condition, dynamicShortcutKey, dynamicShortcutLabel, priority);

        public static CustomMenuItem RegisterAction(Func<GameCard, string> dynamicLabel, Action<GameCard> onClick, Func<GameCard, bool>? condition, Func<Key?> dynamicShortcutKey, Func<string?>? dynamicShortcutLabel = null, int priority = 0)
            => CustomMenuAPI.RegisterAction(dynamicLabel, onClick, condition, dynamicShortcutKey, dynamicShortcutLabel, priority);

        public static CustomMenuItem RegisterSeparator(Func<GameCard, bool>? condition = null, int priority = 0)
            => CustomMenuAPI.RegisterSeparator(condition, priority);

        public static CustomMenuItem RegisterItem(CustomMenuItem item)
            => CustomMenuAPI.RegisterItem(item);

        public static void RegisterProvider(Func<GameCard, IEnumerable<CustomMenuItem>> provider)
            => CustomMenuAPI.RegisterProvider(provider);

        public static void RegisterProvider(Func<GameCard, IEnumerable<MenuItem>> provider)
            => CustomMenuAPI.RegisterProvider(provider);

        public static bool UnregisterItem(CustomMenuItem item)
            => CustomMenuAPI.UnregisterItem(item);

        public static bool UnregisterItem(string id)
            => CustomMenuAPI.UnregisterItem(id);

        public static bool UnregisterProvider(Func<GameCard, IEnumerable<CustomMenuItem>> provider)
            => CustomMenuAPI.UnregisterProvider(provider);

        public static bool UnregisterProvider(Func<GameCard, IEnumerable<MenuItem>> provider)
            => CustomMenuAPI.UnregisterProvider(provider);

        public static ConfigEntry<string> SetupKeybindConfig(ConfigFile config, string name, string displayName, string tooltip, string defaultKey = "None", Action<Key>? onKeyChanged = null)
            => CustomMenuAPI.SetupKeybindConfig(config, name, displayName, tooltip, defaultKey, onKeyChanged);

        public static ConfigEntry<string> SetupKeybindConfig(Mod mod, string name, string displayName, string tooltip, string defaultKey = "None", Action<Key>? onKeyChanged = null)
            => CustomMenuAPI.SetupKeybindConfig(mod, name, displayName, tooltip, defaultKey, onKeyChanged);

        public static ConfigEntry<string> SetupKeybindConfig(ConfigFile config, string name, string displayName, string tooltip, Key defaultKey, Action<Key>? onKeyChanged = null)
            => CustomMenuAPI.SetupKeybindConfig(config, name, displayName, tooltip, defaultKey, onKeyChanged);

        public static ConfigEntry<string> SetupKeybindConfig(Mod mod, string name, string displayName, string tooltip, Key defaultKey, Action<Key>? onKeyChanged = null)
            => CustomMenuAPI.SetupKeybindConfig(mod, name, displayName, tooltip, defaultKey, onKeyChanged);

        public static ConfigEntry<string> SetupColorConfig(ConfigFile config, string name, string displayName, string tooltip, string defaultColorHex = "#FFFFFF", Action<Color>? onColorChanged = null)
            => CustomMenuAPI.SetupColorConfig(config, name, displayName, tooltip, defaultColorHex, onColorChanged);

        public static ConfigEntry<string> SetupColorConfig(Mod mod, string name, string displayName, string tooltip, string defaultColorHex = "#FFFFFF", Action<Color>? onColorChanged = null)
            => CustomMenuAPI.SetupColorConfig(mod, name, displayName, tooltip, defaultColorHex, onColorChanged);

        public static ConfigEntry<string> SetupColorConfig(ConfigFile config, string name, string displayName, string tooltip, Color defaultColor, Action<Color>? onColorChanged = null)
            => CustomMenuAPI.SetupColorConfig(config, name, displayName, tooltip, defaultColor, onColorChanged);

        public static ConfigEntry<string> SetupColorConfig(Mod mod, string name, string displayName, string tooltip, Color defaultColor, Action<Color>? onColorChanged = null)
            => CustomMenuAPI.SetupColorConfig(mod, name, displayName, tooltip, defaultColor, onColorChanged);

        public static Key ParseKey(ConfigEntry<string>? config, Key defaultKey = Key.None)
            => CustomMenuAPI.ParseKey(config, defaultKey);

        public static Color ParseColor(ConfigEntry<string>? config, Color defaultColor = default)
            => CustomMenuAPI.ParseColor(config, defaultColor);

        public static string ColorToHex(Color color)
            => CustomMenuAPI.ColorToHex(color);

        public static void Clear()
            => CustomMenuAPI.Clear();
    }
}
