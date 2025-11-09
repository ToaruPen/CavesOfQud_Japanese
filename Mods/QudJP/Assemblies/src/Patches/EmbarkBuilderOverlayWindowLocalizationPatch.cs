using HarmonyLib;
using System;
using UnityEngine;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.UI;
using XRL.UI;
using XRL.UI.Framework;
using QudJP.Localization;

namespace QudJP
{
    /// <summary>
    /// キャラ作成オーバーレイの Back / Next ボタンは静的 MenuOption を共有しているため、
    /// ここで強制的に日本語テキストへ置き換える。
    /// </summary>
    [HarmonyPatch(typeof(EmbarkBuilderOverlayWindow))]
    internal static class EmbarkBuilderOverlayWindowLocalizationPatch
    {
        private const string BackCommandId = "Cancel";
        private const string NextCommandId = "Page Right";
        private const string BackFallback = "戻る";
        private const string NextFallback = "次へ";

        [HarmonyPostfix]
        [HarmonyPatch(MethodType.StaticConstructor)]
        private static void LocalizeStaticMenuOptions()
        {
            ApplyMenuOptionLocalization(EmbarkBuilderOverlayWindow.BackMenuOption, BackCommandId, BackFallback);
            ApplyMenuOptionLocalization(EmbarkBuilderOverlayWindow.NextMenuOption, NextCommandId, NextFallback);
            Debug.Log("[QudJP] EmbarkBuilderOverlayWindow menu options localized.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EmbarkBuilderOverlayWindow.UpdateMenuBars), new Type[] { })]
        private static void LocalizeButtonsAfterUpdate(EmbarkBuilderOverlayWindow __instance)
        {
            if (__instance == null)
            {
                return;
            }

            ApplyMenuOptionLocalization(EmbarkBuilderOverlayWindow.BackMenuOption, BackCommandId, BackFallback);
            ApplyMenuOptionLocalization(EmbarkBuilderOverlayWindow.NextMenuOption, NextCommandId, NextFallback);

            __instance.backButton?.ForceUpdate();
            __instance.nextButton?.ForceUpdate();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EmbarkBuilderOverlayWindow.UpdateMenuBars), new Type[] { typeof(EmbarkBuilderModuleWindowDescriptor) })]
        private static void LocalizeButtonsAfterUpdateWithDescriptor(EmbarkBuilderOverlayWindow __instance)
        {
            LocalizeButtonsAfterUpdate(__instance);
        }

        private static void ApplyMenuOptionLocalization(MenuOption? option, string? commandId, string fallback)
        {
            if (option == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(commandId) &&
                CommandBindingManager.CommandsByID != null &&
                CommandBindingManager.CommandsByID.TryGetValue(commandId!, out var command) &&
                !string.IsNullOrEmpty(command?.DisplayText))
            {
                option.Description = command!.DisplayText!;
                return;
            }

            if (MenuOptionLegendLocalizer.TryApply(option))
            {
                return;
            }

            if (!string.Equals(option.Description, fallback, StringComparison.Ordinal))
            {
                option.Description = fallback;
            }
        }
    }
}
