using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

        private Button rebindButton;
        private CustomButton customButton;
        private Text buttonText;
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
}