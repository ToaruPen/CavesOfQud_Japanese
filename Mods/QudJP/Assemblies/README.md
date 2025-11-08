# QudJP Assemblies

## ビルド手順
1. `GameDir` MSBuild プロパティを自分の環境に合わせて指定する（例: `dotnet build /p:GameDir="C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud"`）。
2. `dotnet build QudJP.sln -c Release` を実行すると `QudJP.dll` がこのフォルダに出力される。

## 依存関係
- `0Harmony.dll`
- `Assembly-CSharp.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.UI.dll`
- `Unity.TextMeshPro.dll`

いずれもゲーム本体の `CoQ_Data/Managed` 内に存在。`GameDir` プロパティで参照パスを切り替える仕組みにしてある。

## TODO
- Mod ローダー（XRL.ModManager）に合わせたエントリポイント実装
- フォント適用ロジック、翻訳支援ユーティリティの追加
