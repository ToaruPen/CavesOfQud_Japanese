using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
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

        private static readonly MethodInfo BeforeShowMethod =
            AccessTools.Method(typeof(CategoryMenusScroller), nameof(CategoryMenusScroller.BeforeShow));

        private static readonly MethodInfo LocalizeMethod =
            AccessTools.Method(typeof(QudMutationsModuleWindowLocalizationPatch), nameof(LocalizeQueuedOptions));

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> InsertLocalizationHook(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (BeforeShowMethod != null && instruction.Calls(BeforeShowMethod))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, LocalizeMethod);
                }

                yield return instruction;
            }
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
