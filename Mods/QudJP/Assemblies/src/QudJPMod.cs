using HarmonyLib;
using System.Reflection;

namespace QudJP
{
    /// <summary>
    /// エントリポイントとなる Harmony 初期化クラス。
    /// ゲームの Mod ローダーから呼び出される方法は後で紐付ける。
    /// </summary>
    public static class QudJPMod
    {
        private static Harmony? _harmony;

        public static void Initialize()
        {
            if (_harmony != null)
            {
                return;
            }

            _harmony = new Harmony("jp.toarupen.qudjp");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
