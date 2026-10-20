using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace SephiriaMods.ActiveAbilityAutoCast
{
    /// <summary>
    /// Entry point loaded by Sephiria's built-in AddOnLoader.
    /// </summary>
    public sealed class ActiveAbilityAutoCastMod : HorayModBase
    {
        private GameObject controllerObject;

        protected override void OnModLoaded()
        {
            controllerObject = new GameObject("ActiveAbilityAutoCastController");
            controllerObject.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(controllerObject);
            controllerObject.AddComponent<ActiveAbilityAutoCastController>();

            Debug.Log(
                "[ActiveAbilityAutoCast] Loaded. Hold Alt and press 1-8, "
                + "middle-click a quick slot, or hold LT+RT and press its Xbox "
                + "slot button to toggle an active ability or single-shot grimoire.");
        }

        protected override void OnModUnloaded()
        {
            if (controllerObject != null)
            {
                UnityEngine.Object.Destroy(controllerObject);
                controllerObject = null;
            }
        }
    }

    /// <summary>
    /// Tracks the local quick-slot controller and invokes Sephiria's normal cast
    /// path whenever an enabled active ability or grimoire reports that it is
    /// ready.
    /// </summary>
    public sealed class ActiveAbilityAutoCastController : MonoBehaviour
    {
        private const int QuickSlotCount = 8;
        private const float ReadyRatioEpsilon = 0.001f;
        private const float RetryDelay = 0.25f;
        private const string IndicatorObjectName = "ActiveAbilityAutoCastIndicator";

        private readonly HashSet<string> enabledActionKeys = new HashSet<string>();
        private readonly Dictionary<string, float> nextAttemptTimes =
            new Dictionary<string, float>();

        private PlayerSpawner localSpawner;
        private PlayerAvatar playerAvatar;
        private IntegratedActionController actionController;
        private SkillController skillController;
        private UI_SkillQuickSlotBar quickSlotBar;
        private GameObject[] indicators = new GameObject[QuickSlotCount];
        private bool instructionsShown;
        private float nextMagicAttemptTime;

        private void Update()
        {
            RefreshPlayer();
            RefreshQuickSlotBar();
            HandleKeyboardToggle();
            HandleGamepadToggle();
            HandleMouseToggle();
            RefreshIndicators();
            TryAutoCast();
        }

        private void OnDestroy()
        {
            DestroyIndicators();
            enabledActionKeys.Clear();
            nextAttemptTimes.Clear();
        }

        private void RefreshPlayer()
        {
            PlayerSpawner nextSpawner = FindLocalSpawner();
            PlayerAvatar nextAvatar = nextSpawner == null ? null : nextSpawner.PlayerAvatar;

            if (nextSpawner == localSpawner && nextAvatar == playerAvatar)
            {
                return;
            }

            localSpawner = nextSpawner;
            playerAvatar = nextAvatar;
            actionController = playerAvatar == null
                ? null
                : playerAvatar.GetComponent<IntegratedActionController>();
            skillController = playerAvatar == null
                ? null
                : playerAvatar.GetComponent<SkillController>();

            if (playerAvatar != null && !instructionsShown)
            {
                instructionsShown = true;
                ShowMessage(
                    "Auto-cast: Alt+1-8, middle-click, or LT+RT + slot button");
            }
        }

        private static PlayerSpawner FindLocalSpawner()
        {
            if (PlayerSpawner.MultiplayerList == null)
            {
                return null;
            }

            foreach (PlayerSpawner spawner in PlayerSpawner.MultiplayerList)
            {
                if (spawner != null && spawner.isLocalPlayer)
                {
                    return spawner;
                }
            }

            return null;
        }

        private void HandleKeyboardToggle()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.altKey.isPressed)
            {
                return;
            }

            KeyControl[] slotKeys =
            {
                keyboard.digit1Key,
                keyboard.digit2Key,
                keyboard.digit3Key,
                keyboard.digit4Key,
                keyboard.digit5Key,
                keyboard.digit6Key,
                keyboard.digit7Key,
                keyboard.digit8Key
            };

            for (int slot = 0; slot < slotKeys.Length; slot++)
            {
                if (slotKeys[slot].wasPressedThisFrame)
                {
                    ToggleSlot(slot);
                    return;
                }
            }
        }

        private void HandleMouseToggle()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null
                || !mouse.middleButton.wasPressedThisFrame
                || quickSlotBar == null
                || quickSlotBar.pool == null)
            {
                return;
            }

            Vector2 pointerPosition = mouse.position.ReadValue();
            int visibleSlots = Math.Min(QuickSlotCount, quickSlotBar.pool.Length);

            for (int slot = 0; slot < visibleSlots; slot++)
            {
                UI_SkillQuickSlotElement element = quickSlotBar.pool[slot];
                if (element == null || !element.gameObject.activeInHierarchy)
                {
                    continue;
                }

                RectTransform rect = element.transform as RectTransform;
                Canvas canvas = element.GetComponentInParent<Canvas>();
                Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? canvas.worldCamera
                    : null;

                if (rect != null
                    && RectTransformUtility.RectangleContainsScreenPoint(
                        rect,
                        pointerPosition,
                        eventCamera))
                {
                    ToggleSlot(slot);
                    return;
                }
            }
        }

        private void HandleGamepadToggle()
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad == null
                || !gamepad.leftTrigger.isPressed
                || !gamepad.rightTrigger.isPressed)
            {
                return;
            }

            ButtonControl[] slotButtons =
            {
                gamepad.buttonSouth,    // A: slot 1
                gamepad.buttonEast,     // B: slot 2
                gamepad.buttonWest,     // X: slot 3
                gamepad.buttonNorth,    // Y: slot 4
                gamepad.dpad.down,      // slot 5
                gamepad.dpad.right,     // slot 6
                gamepad.dpad.up,        // slot 7
                gamepad.dpad.left       // slot 8
            };

            for (int slot = 0; slot < slotButtons.Length; slot++)
            {
                if (slotButtons[slot].wasPressedThisFrame)
                {
                    ToggleSlot(slot);
                    return;
                }
            }
        }

        private void ToggleSlot(int slot)
        {
            QuickSlotData slotData = GetSlotData(slot);
            if (slotData == null)
            {
                ShowMessage(
                    "Auto-cast needs an active ability or grimoire in that slot");
                return;
            }

            string key;
            string name;

            if (slotData.Type == QuickSlotType.Active && slotData.active != null)
            {
                key = GetAbilityKey(slotData.active);
                name = GetAbilityName(slotData.active);
            }
            else if (slotData.Type == QuickSlotType.Magic && slotData.magic != null)
            {
                if (!IsSupportedMagic(slotData.magic))
                {
                    ShowMessage(
                        "Charged grimoires are not supported by auto-cast: "
                        + GetMagicName(slotData.magic));
                    return;
                }

                key = GetMagicKey(slotData.magic);
                name = GetMagicName(slotData.magic);
            }
            else
            {
                ShowMessage(
                    "Auto-cast needs an active ability or grimoire in that slot");
                return;
            }

            bool enabled;
            if (enabledActionKeys.Contains(key))
            {
                enabledActionKeys.Remove(key);
                nextAttemptTimes.Remove(key);
                enabled = false;
            }
            else
            {
                enabledActionKeys.Add(key);
                nextAttemptTimes[key] = Time.unscaledTime + 0.1f;
                enabled = true;
            }

            ShowMessage(
                "Auto-cast "
                + (enabled ? "ON" : "OFF")
                + ": "
                + name
                + " (slot "
                + (slot + 1)
                + ")");

            Debug.Log(
                "[ActiveAbilityAutoCast] "
                + name
                + " auto-cast "
                + (enabled ? "enabled" : "disabled")
                + ".");
        }

        private void TryAutoCast()
        {
            if (actionController == null
                || playerAvatar == null
                || playerAvatar.IsDead
                || Time.timeScale <= 0f
                || IsAvatarInputBlocked())
            {
                return;
            }

            HashSet<string> attemptedThisFrame = new HashSet<string>();
            int slotCount = GetQuickSlotCount();

            for (int slot = 0; slot < slotCount; slot++)
            {
                QuickSlotData slotData = GetSlotData(slot);
                string key = GetSlotActionKey(slotData);
                if (key == null)
                {
                    continue;
                }

                if (!enabledActionKeys.Contains(key) || !attemptedThisFrame.Add(key))
                {
                    continue;
                }

                float nextAttempt;
                if (nextAttemptTimes.TryGetValue(key, out nextAttempt)
                    && Time.unscaledTime < nextAttempt)
                {
                    continue;
                }

                try
                {
                    if (slotData.Type == QuickSlotType.Active)
                    {
                        IActiveCasting active = slotData.active;
                        float cooldownRatio = active.GetCooldownRatio();
                        if (float.IsNaN(cooldownRatio)
                            || cooldownRatio > ReadyRatioEpsilon)
                        {
                            continue;
                        }
                    }
                    else if (slotData.Type == QuickSlotType.Magic)
                    {
                        Charm_Magic magic = slotData.magic;
                        if (!CanAutoCastMagic(magic))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception exception)
                {
                    DisableBrokenAction(key, GetSlotActionName(slotData), exception);
                    continue;
                }

                nextAttemptTimes[key] = Time.unscaledTime + RetryDelay;

                try
                {
                    UnitAvatar aimedTarget = GetAimedTarget();
                    Vector3 aimedPosition = GetAimedPosition(aimedTarget);
                    actionController.Cast(slot, aimedPosition, aimedTarget);

                    if (slotData.Type == QuickSlotType.Magic)
                    {
                        nextMagicAttemptTime = Time.unscaledTime + RetryDelay;
                        return;
                    }
                }
                catch (Exception exception)
                {
                    DisableBrokenAction(key, GetSlotActionName(slotData), exception);
                }
            }
        }

        private bool CanAutoCastMagic(Charm_Magic magic)
        {
            if (magic == null
                || !IsSupportedMagic(magic)
                || skillController == null
                || !skillController.CanCast
                || skillController.IsInGlobalCooldown
                || Time.unscaledTime < nextMagicAttemptTime)
            {
                return false;
            }

            ECanUseSkillResult result = magic.CanCast(playerAvatar, true, true);
            return result == ECanUseSkillResult.Succeeded
                || result == ECanUseSkillResult.Succeeded_SequenceAction;
        }

        private bool IsAvatarInputBlocked()
        {
            PlayerInputController input = PlayerInputController.Instance;
            return input != null && input.BlockAvatarInput;
        }

        private UnitAvatar GetAimedTarget()
        {
            PlayerInputController input = PlayerInputController.Instance;
            if (input != null && input.autoAimedTarget != null)
            {
                return input.autoAimedTarget;
            }

            return playerAvatar.autoAimedTarget;
        }

        private Vector3 GetAimedPosition(UnitAvatar aimedTarget)
        {
            if (playerAvatar.aimObject != null)
            {
                return playerAvatar.aimObject.transform.position;
            }

            if (aimedTarget != null)
            {
                return aimedTarget.transform.position;
            }

            return playerAvatar.transform.position;
        }

        private QuickSlotData GetSlotData(int slot)
        {
            if (actionController == null
                || actionController.quickSlots == null
                || slot < 0
                || slot >= actionController.quickSlots.Length)
            {
                return null;
            }

            return actionController.quickSlots[slot];
        }

        private int GetQuickSlotCount()
        {
            return actionController == null || actionController.quickSlots == null
                ? 0
                : Math.Min(QuickSlotCount, actionController.quickSlots.Length);
        }

        private void DisableBrokenAction(
            string key,
            string name,
            Exception exception)
        {
            enabledActionKeys.Remove(key);
            nextAttemptTimes.Remove(key);

            ShowMessage("Auto-cast disabled after an error: " + name);
            Debug.LogError(
                "[ActiveAbilityAutoCast] Disabled "
                + name
                + " after a cast error: "
                + exception);
        }

        private static string GetAbilityKey(IActiveCasting active)
        {
            Type type = active.GetType();
            return "active:" + (type.FullName ?? type.Name);
        }

        private static string GetAbilityName(IActiveCasting active)
        {
            try
            {
                string name = active.GetNameString();
                return string.IsNullOrEmpty(name) ? GetAbilityKey(active) : name;
            }
            catch
            {
                return GetAbilityKey(active);
            }
        }

        private static bool IsSupportedMagic(Charm_Magic magic)
        {
            return magic != null
                && magic.ContainedMagic != null
                && magic.ContainedMagic.castingType == ECastingType.SingleShot;
        }

        private static string GetMagicKey(Charm_Magic magic)
        {
            ActiveSkillEntity containedMagic = magic == null ? null : magic.ContainedMagic;
            if (containedMagic != null)
            {
                return "magic:" + containedMagic.id;
            }

            return "magic-instance:" + (magic == null ? 0 : magic.GetInstanceID());
        }

        private static string GetMagicName(Charm_Magic magic)
        {
            try
            {
                ActiveSkillEntity containedMagic =
                    magic == null ? null : magic.ContainedMagic;
                string name = containedMagic == null ? null : containedMagic.Name;
                return string.IsNullOrEmpty(name) ? GetMagicKey(magic) : name;
            }
            catch
            {
                return GetMagicKey(magic);
            }
        }

        private static string GetSlotActionKey(QuickSlotData slotData)
        {
            if (slotData == null)
            {
                return null;
            }

            if (slotData.Type == QuickSlotType.Active && slotData.active != null)
            {
                return GetAbilityKey(slotData.active);
            }

            if (slotData.Type == QuickSlotType.Magic
                && slotData.magic != null
                && IsSupportedMagic(slotData.magic))
            {
                return GetMagicKey(slotData.magic);
            }

            return null;
        }

        private static string GetSlotActionName(QuickSlotData slotData)
        {
            if (slotData != null
                && slotData.Type == QuickSlotType.Active
                && slotData.active != null)
            {
                return GetAbilityName(slotData.active);
            }

            if (slotData != null
                && slotData.Type == QuickSlotType.Magic
                && slotData.magic != null)
            {
                return GetMagicName(slotData.magic);
            }

            return "unknown quick-slot action";
        }

        private static void ShowMessage(string message)
        {
            try
            {
                UIManager manager = UIManager.Instance;
                UI_SystemMessage systemMessage =
                    manager == null ? null : manager.GetElement<UI_SystemMessage>();
                if (systemMessage != null)
                {
                    systemMessage.Open(message, 2.5f, false);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[ActiveAbilityAutoCast] Could not show UI message: "
                    + exception.Message);
            }
        }

        private void RefreshQuickSlotBar()
        {
            if (quickSlotBar != null)
            {
                return;
            }

            UI_SkillQuickSlotBar nextBar =
                UnityEngine.Object.FindAnyObjectByType<UI_SkillQuickSlotBar>();
            if (nextBar == null)
            {
                return;
            }

            quickSlotBar = nextBar;
            DestroyIndicators();
            indicators = new GameObject[QuickSlotCount];
        }

        private void RefreshIndicators()
        {
            if (quickSlotBar == null || quickSlotBar.pool == null)
            {
                return;
            }

            int visibleSlots = Math.Min(QuickSlotCount, quickSlotBar.pool.Length);
            for (int slot = 0; slot < visibleSlots; slot++)
            {
                UI_SkillQuickSlotElement element = quickSlotBar.pool[slot];
                if (element == null)
                {
                    continue;
                }

                if (indicators[slot] == null)
                {
                    indicators[slot] = CreateIndicator(element);
                }

                string key = GetSlotActionKey(GetSlotData(slot));
                bool enabled = key != null && enabledActionKeys.Contains(key);
                indicators[slot].SetActive(enabled);
            }
        }

        private static GameObject CreateIndicator(UI_SkillQuickSlotElement element)
        {
            GameObject marker = new GameObject(
                IndicatorObjectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            marker.hideFlags = HideFlags.HideAndDontSave;

            RectTransform rect = marker.GetComponent<RectTransform>();
            rect.SetParent(element.transform, false);
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-3f, -3f);
            rect.sizeDelta = new Vector2(11f, 11f);
            rect.SetAsLastSibling();

            Image image = marker.GetComponent<Image>();
            image.color = new Color(0.15f, 1f, 0.3f, 0.95f);
            image.raycastTarget = false;

            marker.SetActive(false);
            return marker;
        }

        private void DestroyIndicators()
        {
            if (indicators == null)
            {
                return;
            }

            for (int slot = 0; slot < indicators.Length; slot++)
            {
                if (indicators[slot] != null)
                {
                    UnityEngine.Object.Destroy(indicators[slot]);
                    indicators[slot] = null;
                }
            }
        }
    }
}
