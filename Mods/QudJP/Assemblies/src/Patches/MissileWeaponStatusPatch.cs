using HarmonyLib;
using Qud.UI;
using XRL.World.Parts;

namespace QudJP.Patches
{
    /// <summary>
    /// Ensures MissileWeapon modern UI status objects always receive a non-empty text payload.
    /// </summary>
    [HarmonyPatch(typeof(MissileWeapon))]
    internal static class MissileWeaponStatusPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MissileWeapon.Status))]
        private static void EnsureModernUIText(ref string __result, MissileWeaponArea.MissileWeaponAreaWeaponStatus modernUIStatus)
        {
            if (modernUIStatus == null)
            {
                return;
            }

            var fallback = !string.IsNullOrEmpty(__result)
                ? __result
                : modernUIStatus.display;

            if (string.IsNullOrEmpty(fallback))
            {
                fallback = modernUIStatus.renderable?.getRenderString();
            }

            fallback ??= "<missile weapon>";

            if (string.IsNullOrEmpty(modernUIStatus.text))
            {
                modernUIStatus.text = fallback;
            }

            if (string.IsNullOrEmpty(modernUIStatus.display))
            {
                modernUIStatus.display = fallback;
            }

            if (string.IsNullOrEmpty(__result))
            {
                __result = fallback;
            }
        }
    }
}
