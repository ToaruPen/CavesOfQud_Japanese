using System;
using System.Collections.Generic;
using HarmonyLib;
using Qud.UI;
using QudJP.Localization;
using UnityEngine;
using XRL.World;

namespace QudJP
{
    /// <summary>
    /// Ensures inventory line entries always have a visible display name.
    /// </summary>
    [HarmonyPatch(typeof(InventoryLineData))]
    internal static class InventoryLineLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryLineData.displayName), MethodType.Getter)]
        private static void EnsureDisplayName(InventoryLineData __instance, ref string __result)
        {
            if (!string.IsNullOrWhiteSpace(__result) || __instance?.go == null)
            {
                if (!string.IsNullOrEmpty(__result))
                {
                    __result = SanitizeMarkers(__result);
                }
                return;
            }

            var fallback = __instance.go.DisplayNameOnlyDirect;
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                __result = SanitizeMarkers(fallback);
                return;
            }

            fallback = __instance.go.DisplayNameOnly;
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                __result = SanitizeMarkers(fallback);
                return;
            }

            fallback = __instance.go.Blueprint;
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                __result = SanitizeMarkers(fallback);
                return;
            }

            var id = __instance.go.IDIfAssigned ?? "<none>";
            var renderName = __instance.go.Render?.DisplayName ?? "<null>";
            Debug.LogWarning($"[QudJP] Inventory display name missing for blueprint '{__instance.go.Blueprint}' (ID={id}, Render='{renderName}')");
        }

        private static string SanitizeMarkers(string value)
        {
            try
            {
                return System.Text.RegularExpressions.Regex.Replace(value, "\\s*<[A-Z]{1,3}\\d{1,3}>", string.Empty);
            }
            catch
            {
                return value;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryLineData))]
    internal static class InventoryLineCategoryLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryLineData.set))]
        private static void LocalizeCategory(
            InventoryLineData __instance,
            [HarmonyArgument(0)] bool category,
            [HarmonyArgument(1)] string categoryName)
        {
            if (!category)
            {
                return;
            }

            if (InventoryCategoryLocalization.TryTranslate(categoryName, out var translated))
            {
                __instance.categoryName = translated;
            }
        }
    }

    [HarmonyPatch(typeof(InventoryLineData))]
    internal static class InventoryLineDiagnosticsPatch
    {
        private static readonly HashSet<string> Logged = new(StringComparer.OrdinalIgnoreCase);

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryLineData.set))]
        private static void LogMissingDisplayName(
            [HarmonyArgument(0)] bool category,
            [HarmonyArgument(6)] XRL.World.GameObject? go)
        {
            if (category || go == null)
            {
                return;
            }

            var displayName = go.DisplayName;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return;
            }

            var blueprint = go.Blueprint ?? "<null>";
            var renderName = go.Render?.DisplayName ?? "<null>";
            var id = go.IDIfAssigned ?? "<none>";
            var displayNameOnly = go.DisplayNameOnly ?? "<null>";
            var displayNameDirect = go.DisplayNameOnlyDirect ?? "<null>";

            var key = $"{blueprint}::{renderName}::{id}";
            if (!Logged.Add(key))
            {
                return;
            }

            Debug.LogWarning(
                $"[QudJP] Inventory line missing display name (Blueprint='{blueprint}', ID={id}, Render='{renderName}', DisplayNameOnly='{displayNameOnly}', DisplayNameOnlyDirect='{displayNameDirect}').");
        }
    }
}
