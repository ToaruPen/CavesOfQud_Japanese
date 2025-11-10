using System;
using System.Collections.Generic;

namespace QudJP.Localization
{
    /// <summary>
    /// Provides a shared translation map for inventory category labels (FilterBar, inventory list, container view, etc.).
    /// </summary>
    internal static class InventoryCategoryLocalization
    {
        private static readonly Dictionary<string, string> CategoryTranslations = new(StringComparer.Ordinal)
        {
            ["*All"] = "すべて",
            ["Light Sources"] = "光源",
            ["Melee Weapons"] = "近接武器",
            ["Thrown Weapons"] = "投擲武器",
            ["Miscellaneous"] = "その他",
            ["Food"] = "食料",
            ["Corpses"] = "死体",
            ["Plants"] = "植物",
            ["Missile Weapons"] = "射撃武器",
            ["Projectiles"] = "投射物",
            ["Ammo"] = "弾薬",
            ["Armor"] = "防具",
            ["Shields"] = "盾",
            ["Grenades"] = "手榴弾",
            ["Creatures"] = "生物",
            ["Applicators"] = "塗布器具",
            ["Energy Cells"] = "エネルギーセル",
            ["Natural Weapons"] = "自然武器",
            ["Natural Missile Weapons"] = "自然射撃武器",
            ["Natural Missile Weapon"] = "自然射撃武器",
            ["Natural Armor"] = "自然防具",
            ["Meds"] = "医薬品",
            ["Tonics"] = "トニック",
            ["Water Containers"] = "水容器",
            ["Books"] = "書物",
            ["Tools"] = "道具",
            ["Artifacts"] = "アーティファクト",
            ["Clothes"] = "衣服",
            ["Trade Goods"] = "交易品",
            ["Quest Items"] = "クエスト品",
            ["Data Disks"] = "データディスク",
            ["Scrap"] = "スクラップ",
            ["Trinkets"] = "装身具",
            ["Cybernetic Implants"] = "サイバネ義肢",
            ["CommonMods"] = "汎用改造",
            ["WeaponMods"] = "武器改造",
            ["BladeMods"] = "刃物改造",
            ["LongBladeMods"] = "長刃改造",
            ["CudgelMods"] = "棍棒改造",
            ["AxeMods"] = "斧改造",
            ["ThrownWeaponMods"] = "投擲改造",
            ["GrenadeMods"] = "手榴弾改造",
            ["MissileWeaponMods"] = "射撃武器改造",
            ["BowMods"] = "弓改造",
            ["FirearmMods"] = "火器改造",
            ["RifleMods"] = "ライフル改造",
            ["PistolMods"] = "拳銃改造",
            ["BeamWeaponMods"] = "ビーム武器改造",
            ["MagazineMods"] = "弾倉改造",
            ["BodyMods"] = "胴体改造",
            ["CloakMods"] = "マント改造",
            ["MaskMods"] = "仮面改造",
            ["BootMods"] = "ブーツ改造",
            ["GauntletMods"] = "篭手改造",
            ["HeadwearMods"] = "頭部装飾改造",
            ["HelmetMods"] = "兜改造",
            ["EyewearMods"] = "ゴーグル改造",
            ["ShieldMods"] = "盾改造",
            ["GloveMods"] = "手袋改造",
            ["BracerMods"] = "腕輪改造",
            ["WingsMods"] = "翼改造",
            ["ExoskeletonMods"] = "外骨格改造",
            ["EnergyCellMods"] = "エネルギーセル改造",
            ["ElectronicsMods"] = "電子機器改造",
            ["Locations"] = "ロケーション",
            ["Gossip and Lore"] = "噂と伝承",
            ["Sultan Histories"] = "スルタン史",
            ["Village Histories"] = "村の歴史",
            ["Chronology"] = "年代記",
            ["General Notes"] = "雑記",
            ["Recipes"] = "レシピ"
        };

        public static bool TryTranslate(string? source, out string translation)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (CategoryTranslations.TryGetValue(source!, out var mapped))
                {
                    translation = mapped;
                    return true;
                }
            }

            translation = source ?? string.Empty;
            return false;
        }

        public static string TranslateOrOriginal(string? source)
        {
            return TryTranslate(source, out var translation) ? translation : source ?? string.Empty;
        }

        public static void ApplyTo(IDictionary<string, string> target)
        {
            if (target == null)
            {
                return;
            }

            foreach (var pair in CategoryTranslations)
            {
                target[pair.Key] = pair.Value;
            }
        }
    }
}
