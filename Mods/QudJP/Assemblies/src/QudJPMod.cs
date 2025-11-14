using HarmonyLib;
using QudJP.ConsoleUI;
using QudJP.Localization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace QudJP
{
    /// <summary>
    /// Harmony パッチと翻訳／フォント／コンソール橋渡しをまとめて初期化するモジュールエントリポイント。
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

            Translator.Instance.Initialize();
            FontManager.Instance.TryLoadFonts();
            ConsoleBridge.Instance.Initialize();
            Debug.Log("[QudJP] Harmony パッチと周辺サービスの初期化が完了しました。");
        }
    }
}


