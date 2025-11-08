# Changelog

## 0.1.0 (WIP)
- リポジトリ初期化、基本ディレクトリ構成を整備。
- ログ監視 / フォントパイプライン / 翻訳プロセス / テスト計画など主要ドキュメントを追加。
- Python ツール群（`scripts/extract_base.py` / `scripts/diff_localization.py` / `scripts/check_encoding.py` / `scripts/sync_mod.py`）を追加し、バニラ抽出・差分レポート・モジバケ検知・Mod 同期をクロスプラットフォーム対応させた。
- Harmony プロジェクト（QudJP）を構築し、モジュールイニシャライザ・FontManager・ModManager パッチ・ModPathResolver を実装。
- Noto Sans CJK をサブセット化して `Fonts/` に配置、ランタイムで TMP Font Asset を生成して UI へ適用。
- Localization サンプル（Books / Conversations / Commands / EmbarkModules）と Mod manifest / Workshop 用テンプレを作成。
- README / Mod 内 README / Assemblies README / translation_status を更新し、導入・開発フローを明文化。
