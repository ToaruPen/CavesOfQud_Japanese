using System;
using HarmonyLib;
using QudJP.Localization;
using XRL;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(ModManager))]
    internal static class ModManagerLocalizationFilePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ModManager.ForEachFile), typeof(string), typeof(Action<string, ModInfo>), typeof(bool), typeof(bool))]
        private static void PreferLocalizedFiles(string FileName, Action<string, ModInfo> FileAction, bool IncludeDisabled, bool Recursive)
        {
            LocalizationAssetResolver.TryInjectOverride(FileName, FileAction, Recursive);
        }
    }
}
