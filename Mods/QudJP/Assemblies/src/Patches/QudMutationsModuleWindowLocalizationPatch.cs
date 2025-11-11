using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using QudJP.Localization;
using XRL.CharacterBuilds.Qud.UI;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    /// <summary>
    /// Localizes mutation long descriptions in the character creation mutations window.
    /// </summary>
    [HarmonyPatch(typeof(QudMutationsModuleWindow), nameof(QudMutationsModuleWindow.UpdateControls))]
    internal static class QudMutationsModuleWindowLocalizationPatch
    {
        private static readonly FieldInfo CategoryMenusField =
            AccessTools.Field(typeof(QudMutationsModuleWindow), "categoryMenus");

        // Use a simple Postfix hook instead of IL rewrite to avoid ambiguous matches
        [HarmonyPostfix]
        private static void Postfix(QudMutationsModuleWindow __instance)
        {
            try
            {
                LocalizeQueuedOptions(__instance);
            }
            catch { }
        }

        private static void LocalizeQueuedOptions(QudMutationsModuleWindow instance)
        {
            if (instance == null)
            {
                return;
            }

            if (CategoryMenusField?.GetValue(instance) is not List<CategoryMenuData> categories)
            {
                return;
            }

            foreach (var category in categories)
            {
                if (category?.menuOptions == null)
                {
                    continue;
                }

                foreach (var option in category.menuOptions)
                {
                    if (option == null || string.IsNullOrEmpty(option.LongDescription))
                    {
                        continue;
                    }

                    option.LongDescription = MutationDescriptionLocalizer.Localize(option.Id, option.LongDescription);
                }
            }
        }
    }
}
