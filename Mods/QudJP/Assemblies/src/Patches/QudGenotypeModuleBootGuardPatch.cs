#nullable disable

using System;
using HarmonyLib;
using UnityEngine;
using XRL;
using XRL.CharacterBuilds;
using XRL.CharacterBuilds.Qud;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(QudGenotypeModule), nameof(QudGenotypeModule.handleBootEvent))]
    public static class QudGenotypeModuleBootGuardPatch
    {
        private const string DefaultGenotype = "Mutated Human";
        private const string SecondaryGenotype = "True Kin";

        public static bool Prefix(QudGenotypeModule __instance, string id, XRLGame game, EmbarkInfo info, object element, ref object __result)
        {
            if (__instance == null || __instance.data != null)
            {
                return true;
            }

            if (TryAssignFallback(__instance))
            {
                return true;
            }

            Debug.LogWarning("[QudJP] Genotype data was null during boot. Skipping QudGenotypeModule.handleBootEvent to avoid a crash.");
            __result = (object)null;
            return false;
        }

        private static bool TryAssignFallback(QudGenotypeModule module)
        {
            if (TryAssignDirect(module, DefaultGenotype))
            {
                return true;
            }

            if (TryAssignDirect(module, SecondaryGenotype))
            {
                return true;
            }

            return false;
        }

        private static bool TryAssignDirect(QudGenotypeModule module, string genotypeId)
        {
            if (module == null || string.IsNullOrWhiteSpace(genotypeId))
            {
                return false;
            }

            if (!module.genotypesByName.ContainsKey(genotypeId))
            {
                return false;
            }

            try
            {
                module.setDataDirect(new QudGenotypeModuleData(genotypeId));
                return module.data != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QudJP] Failed to assign fallback genotype '{genotypeId}': {ex}");
                return false;
            }
        }
    }
}
