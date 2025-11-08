using HarmonyLib;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace QudJP
{
    /// <summary>
    /// Harmony の初期化と共通サービスのブートストラップを担当。
    /// モジュールイニシャライザでゲーム起動時に自動実行する。
    /// </summary>
    public static class QudJPMod
    {
        private static bool _initialized;
        private static Harmony? _harmony;

        [ModuleInitializer]
        public static void ModuleInitialize()
        {
            EnsureInitialized();
        }

        public static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            _harmony = new Harmony("jp.toarupen.qudjp");
            _harmony.PatchAll();

            FontManager.Instance.TryLoadFonts();
            Debug.Log("[QudJP] Harmony パッチとフォント初期化を実行しました。");
        }
    }
}
