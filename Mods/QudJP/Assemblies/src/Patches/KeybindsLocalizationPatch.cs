using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Qud.UI;
using QudJP.Localization;
using XRL.UI;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(CommandBindingManager))]
    internal static class CommandBindingManagerLocalizationPatch
    {
        private static readonly Dictionary<string, string> CategoryLabels =
            new(StringComparer.OrdinalIgnoreCase);

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CommandBindingManager.HandleCommandNode))]
        private static void Postfix(global::XRL.XmlDataHelper xml)
        {
            if (xml == null || CommandBindingManager.CommandsByID == null)
            {
                return;
            }

            var id = xml.GetAttribute("ID");
            if (string.IsNullOrEmpty(id) ||
                !CommandBindingManager.CommandsByID.TryGetValue(id, out var command) ||
                command == null)
            {
                return;
            }

            var fallback = command.DisplayText ?? id;
            command.DisplayText = HelpOptionsKeybindsLocalization.LocalizeCommandDisplay(id, fallback);

            if (!string.IsNullOrEmpty(command.Category))
            {
                CategoryLabels[command.Category] = HelpOptionsKeybindsLocalization.LocalizeCommandCategoryLabel(
                    command.Category,
                    command.Category);
            }
        }

        internal static string ResolveCategoryLabel(string categoryId, string fallback)
        {
            if (!string.IsNullOrEmpty(categoryId) &&
                CategoryLabels.TryGetValue(categoryId, out var cached) &&
                !string.IsNullOrEmpty(cached))
            {
                return cached;
            }

            var localized = HelpOptionsKeybindsLocalization.LocalizeCommandCategoryLabel(
                categoryId,
                fallback);

            if (!string.IsNullOrEmpty(categoryId))
            {
                CategoryLabels[categoryId] = localized;
            }

            return localized;
        }
    }

    [HarmonyPatch(typeof(KeybindRow))]
    internal static class KeybindRowLocalizationPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(KeybindRow.setData))]
        private static void Prefix(FrameworkDataElement data, out string? __state)
        {
            __state = null;

            if (data is KeybindCategoryRow categoryRow)
            {
                var label = CommandBindingManagerLocalizationPatch.ResolveCategoryLabel(
                    categoryRow.CategoryId,
                    categoryRow.CategoryDescription ?? categoryRow.CategoryId ?? string.Empty);
                categoryRow.CategoryDescription = label;
                __state = label;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(KeybindRow.setData))]
        private static void Postfix(KeybindRow __instance, FrameworkDataElement data, string? __state)
        {
            if (__instance == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(__state) && __instance.categoryDescription != null)
            {
                __instance.categoryDescription.SetText($"{{{{C|{__state}}}}}");
            }

            if (data is KeybindDataRow)
            {
                var localizedNone = HelpOptionsKeybindsLocalization.LocalizeKeybindNone("None");
                ReplaceNonePlaceholder(__instance.box1, localizedNone);
                ReplaceNonePlaceholder(__instance.box2, localizedNone);
                ReplaceNonePlaceholder(__instance.box3, localizedNone);
                ReplaceNonePlaceholder(__instance.box4, localizedNone);
            }
        }

        private static void ReplaceNonePlaceholder(KeybindBox? box, string localized)
        {
            if (box == null)
            {
                return;
            }

            if (string.Equals(box.boxText, "{{K|None}}", StringComparison.Ordinal) ||
                string.Equals(box.boxText, "{{c|None}}", StringComparison.Ordinal))
            {
                box.boxText = $"{{{{K|{localized}}}}}";
                box.forceUpdate = true;
            }
        }
    }

    [HarmonyPatch(typeof(KeybindsScreen))]
    internal static class KeybindsScreenLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(KeybindsScreen.QueryKeybinds))]
        private static void Postfix(KeybindsScreen __instance)
        {
            if (__instance == null)
            {
                return;
            }

            if (__instance.menuItems != null)
            {
                foreach (var element in __instance.menuItems)
                {
                    switch (element)
                    {
                        case KeybindCategoryRow categoryRow:
                            categoryRow.CategoryDescription = CommandBindingManagerLocalizationPatch.ResolveCategoryLabel(
                                categoryRow.CategoryId,
                                categoryRow.CategoryDescription ?? categoryRow.CategoryId ?? string.Empty);
                            break;

                        case KeybindDataRow keyRow:
                            var categoryLabel = CommandBindingManagerLocalizationPatch.ResolveCategoryLabel(
                                keyRow.CategoryId,
                                keyRow.CategoryId ?? string.Empty);
                            keyRow.SearchWords = HelpOptionsKeybindsLocalization.BuildOptionSearchKeywords(new[]
                            {
                                keyRow.SearchWords,
                                categoryLabel,
                                keyRow.KeyDescription
                            });
                            break;
                    }
                }
            }

            if (__instance.ControlTypeDisplayName != null)
            {
                var keys = __instance.ControlTypeDisplayName.Keys.ToArray();
                foreach (var key in keys)
                {
                    var label = __instance.ControlTypeDisplayName[key];
                    __instance.ControlTypeDisplayName[key] = HelpOptionsKeybindsLocalization.LocalizeControlTypeLabel(
                        GetDeviceKey(key),
                        label);
                }
            }

            var banner = HelpOptionsKeybindsLocalization.LocalizeKeybindInputBanner(
                ResolveDeviceLabel(__instance));
            __instance.inputTypeText?.SetText(banner);
        }

        private static string GetDeviceKey(ControlManager.InputDeviceType type) =>
            type switch
            {
                ControlManager.InputDeviceType.Gamepad => "Gamepad",
                ControlManager.InputDeviceType.Keyboard => "KeyboardMouse",
                _ => type.ToString()
            };

        private static readonly MethodInfo? CurrentGamepadGetter =
            AccessTools.PropertyGetter(typeof(KeybindsScreen), "currentGamepad");

        private static string ResolveDeviceLabel(KeybindsScreen screen)
        {
            if (screen.currentControllerType == ControlManager.InputDeviceType.Gamepad)
            {
                object? gamepad = null;
                try
                {
                    gamepad = CurrentGamepadGetter?.Invoke(screen, null);
                }
                catch
                {
                    gamepad = null;
                }

                if (gamepad != null)
                {
                    var nameProperty = gamepad.GetType().GetProperty("name", BindingFlags.Instance | BindingFlags.Public);
                    if (nameProperty?.GetValue(gamepad, null) is string label && !string.IsNullOrEmpty(label))
                    {
                        return label;
                    }
                }

                return HelpOptionsKeybindsLocalization.LocalizeNoControllerDetected("<no controller detected>");
            }

            if (screen.ControlTypeDisplayName != null &&
                screen.ControlTypeDisplayName.TryGetValue(ControlManager.InputDeviceType.Keyboard, out var keyboardLabel) &&
                !string.IsNullOrEmpty(keyboardLabel))
            {
                return keyboardLabel;
            }

            return "Keyboard && Mouse";
        }
    }

    [HarmonyPatch(typeof(FrameworkSearchInput))]
    internal static class FrameworkSearchInputLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FrameworkSearchInput.Awake))]
        private static void PostAwake(FrameworkSearchInput __instance)
        {
            if (__instance == null)
            {
                return;
            }

            __instance.PopupTitle = HelpOptionsKeybindsLocalization.LocalizeSearchPopupTitle(
                __instance.PopupTitle ?? "Enter search text");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(FrameworkSearchInput.Update))]
        private static void PostUpdate(FrameworkSearchInput __instance)
        {
            if (__instance?.InputText == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(__instance.SearchText))
            {
                var placeholder = HelpOptionsKeybindsLocalization.LocalizeSearchPlaceholder("<search>");
                __instance.InputText.SetText($"{{{{K|{placeholder}}}}}");
            }
        }
    }
}
