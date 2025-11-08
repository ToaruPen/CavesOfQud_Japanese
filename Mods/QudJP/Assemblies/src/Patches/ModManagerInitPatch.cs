using HarmonyLib;
using XRL;

namespace QudJP
{
    [HarmonyPatch(typeof(ModManager), nameof(ModManager.Init))]
    internal static class ModManagerInitPatch
    {
        private static void Postfix()
        {
            QudJPMod.EnsureInitialized();
        }
    }
}
