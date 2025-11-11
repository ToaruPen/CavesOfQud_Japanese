using System.Globalization;

namespace QudJP.Localization
{
    internal static class InventoryLabelLocalizer
    {
        private const string PriceTemplate = "{{B|${value}}}";
        private const string PriceFallback = PriceTemplate;
        private const string WeightHeaderTemplate = "{{C|{carried}{{K|/{capacity}} lbs. }}";
        private const string WeightHeaderFallback = WeightHeaderTemplate;
        private const string CategoryWeightTemplate = "|{items} items|{weight} lbs.|";
        private const string CategoryWeightFallback = CategoryWeightTemplate;
        private const string CategoryWeightOnlyTemplate = "|{weight} lbs.|";
        private const string CategoryWeightOnlyFallback = CategoryWeightOnlyTemplate;
        private const string ItemWeightTemplate = "[{weight} lbs.]";
        private const string ItemWeightFallback = ItemWeightTemplate;

        public static string FormatPrice(int drams)
        {
            var template = Translator.Instance.Apply(PriceTemplate, "Qud.UI.InventoryAndEquipmentStatusScreen.PriceText");
            if (string.Equals(template, PriceTemplate, System.StringComparison.Ordinal))
            {
                template = PriceFallback;
            }

            return ReplaceTokens(
                template,
                ("{value}", drams.ToString(CultureInfo.InvariantCulture)));
        }

        public static string FormatHeaderWeight(int carried, int capacity)
        {
            var template = Translator.Instance.Apply(WeightHeaderTemplate, "Qud.UI.InventoryAndEquipmentStatusScreen.WeightText");
            if (string.Equals(template, WeightHeaderTemplate, System.StringComparison.Ordinal))
            {
                template = WeightHeaderFallback;
            }

            return ReplaceTokens(
                template,
                ("{carried}", carried.ToString(CultureInfo.InvariantCulture)),
                ("{capacity}", capacity.ToString(CultureInfo.InvariantCulture)));
        }

        public static string FormatCategoryWeight(int items, int pounds, bool includeItemCount)
        {
            if (includeItemCount)
            {
                var template = Translator.Instance.Apply(CategoryWeightTemplate, "Qud.UI.InventoryLine.CategoryWeightText");
                if (string.Equals(template, CategoryWeightTemplate, System.StringComparison.Ordinal))
                {
                    template = CategoryWeightFallback;
                }

                return ReplaceTokens(
                    template,
                    ("{items}", items.ToString(CultureInfo.InvariantCulture)),
                    ("{weight}", pounds.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                var template = Translator.Instance.Apply(CategoryWeightOnlyTemplate, "Qud.UI.InventoryLine.CategoryWeightText.WeightOnly");
                if (string.Equals(template, CategoryWeightOnlyTemplate, System.StringComparison.Ordinal))
                {
                    template = CategoryWeightOnlyFallback;
                }

                return ReplaceTokens(
                    template,
                    ("{weight}", pounds.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public static string FormatItemWeight(int pounds)
        {
            var template = Translator.Instance.Apply(ItemWeightTemplate, "Qud.UI.InventoryLine.ItemWeightLabel");
            if (string.Equals(template, ItemWeightTemplate, System.StringComparison.Ordinal))
            {
                template = ItemWeightFallback;
            }

            return ReplaceTokens(
                template,
                ("{weight}", pounds.ToString(CultureInfo.InvariantCulture)));
        }

        private static string ReplaceTokens(string template, params (string Token, string Value)[] replacements)
        {
            var result = template;
            for (int i = 0; i < replacements.Length; i++)
            {
                result = result.Replace(replacements[i].Token, replacements[i].Value);
            }

            return result;
        }
    }
}


