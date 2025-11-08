# QudJP Assemblies

Harmony ベースの補助 DLL（`QudJP.dll`）をビルドするためのソリューションです。

## ビルド手順
1. ゲームのインストールパスを `GameDir` MSBuild プロパティで指定します。例:  
   ```powershell
   dotnet build QudJP.sln -c Release `
     /p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"
   ```
   `GameDir` を省略した場合は同じパスを既定値としてビルドします。
2. 成功すると `QudJP.dll` / `QudJP.pdb` がこのフォルダ直下に出力され、Mod 側 (`Mods/QudJP/Assemblies`) からそのまま読み込まれます。

## 依存ライブラリ
ゲーム本体の `CoQ_Data/Managed` から次の DLL を参照します。
- `0Harmony.dll`
- `Assembly-CSharp.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.UI.dll`
- `Unity.TextMeshPro.dll`
- `UnityEngine.TextRenderingModule.dll`
- `UnityEngine.TextCoreFontEngineModule.dll`
- `UnityEngine.TextCoreTextEngineModule.dll`

## 実装済みコンポーネント
- `QudJPMod` – モジュールイニシャライザで Harmony を起動し、フォント初期化を行うエントリーポイント。
- `FontManager` – サブセット化した OTF から TMP Font Asset を動的生成し、TextMeshPro / uGUI 両方に適用。
- `ModManagerInitPatch` / `TextMeshProPatches` / `UnityUITextPatch` – 起動時および各 UI コンポーネントの OnEnable で日本語フォントを強制適用。
- `ModPathResolver` – Mod フォルダの絶対パスを解決してフォントファイルやドキュメントを読み込むユーティリティ。

追加機能（自動生成テキストの日本語化など）を実装する際は、同じ `src/` 配下にクラスを追加し `dotnet build` で DLL を再生成してください。
