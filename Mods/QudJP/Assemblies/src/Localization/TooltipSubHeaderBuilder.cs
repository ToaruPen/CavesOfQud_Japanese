using System;
using QudJP.Localization;
using XRL.World;
using XRL.World.Parts;

namespace QudJP.Localization
{
    internal static class TooltipSubHeaderBuilder
    {
        public static string? Build(GameObject? go)
        {
            if (go == null)
            {
                return null;
            }

            var parts = new System.Collections.Generic.List<string>(2);

            var category = GetCategory(go);
            if (!string.IsNullOrEmpty(category))
            {
                parts.Add(category!);
            }

            var slot = GetSlot(go);
            if (!string.IsNullOrEmpty(slot))
            {
                parts.Add(slot!);
            }

            if (parts.Count == 0)
            {
                return null;
            }

            return string.Join(" ▸ ", parts);
        }

        private static string? GetCategory(GameObject go)
        {
            try
            {
                var category = go.GetInventoryCategory(AsIfKnown: true);
                if (string.IsNullOrWhiteSpace(category))
                {
                    return null;
                }

                if (!InventoryCategoryLocalization.TryTranslate(category!, out var localized))
                {
                    localized = SafeStringTranslator.SafeTranslate(category!, "Inventory.Category");
                }

                return string.IsNullOrWhiteSpace(localized) ? category : localized;
            }
            catch
            {
                return null;
            }
        }

        private static string? GetSlot(GameObject go)
        {
            var armor = go.GetPart<Armor>();
            if (armor != null)
            {
                var wornOn = armor.WornOn;
                if (!string.IsNullOrWhiteSpace(wornOn) &&
                    !string.Equals(wornOn, "*", StringComparison.Ordinal) &&
                    !string.Equals(wornOn, "Any", StringComparison.Ordinal))
                {
                    return SafeStringTranslator.SafeTranslate(wornOn!, "Armor.WornOn") ?? wornOn;
                }
            }

            var missile = go.GetPart<XRL.World.Parts.MissileWeapon>();
            return null;
        }
    }
}
