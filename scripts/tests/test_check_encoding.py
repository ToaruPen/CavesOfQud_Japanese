from pathlib import Path
import sys

import pytest

REPO_ROOT = Path(__file__).resolve().parents[2]
if str(REPO_ROOT) not in sys.path:
    sys.path.insert(0, str(REPO_ROOT))

import scripts.check_encoding as check_encoding


@pytest.fixture(name="isolated_repo")
def fixture_isolated_repo(tmp_path, monkeypatch):
    """Create a fake repository layout for encoding checks."""
    repo_root = tmp_path / "repo"
    repo_root.mkdir()

    docs_dir = repo_root / "Docs"
    docs_dir.mkdir()
    mods_dir = repo_root / "Mods" / "QudJP"
    mods_dir.mkdir(parents=True)

    # Files that should be returned by the iterator
    docs_file = docs_dir / "guide.md"
    docs_file.write_text("ガイド", encoding="utf-8")
    mod_file = mods_dir / "data.xml"
    mod_file.write_text("<root />", encoding="utf-8")

    # Files that should be ignored by default
    for ignore_entry in check_encoding.DEFAULT_IGNORES:
        ignored = repo_root / ignore_entry
        ignored.parent.mkdir(parents=True, exist_ok=True)
        ignored.write_text("ignored", encoding="utf-8")

    monkeypatch.setattr(check_encoding, "REPO_ROOT", repo_root)
    monkeypatch.chdir(repo_root)

    ignore = {check_encoding.resolve_ignore(path).resolve() for path in check_encoding.DEFAULT_IGNORES}
    return repo_root, docs_file, mod_file, ignore


def test_iter_candidate_files_resolves_repo_relative_paths(isolated_repo):
    repo_root, docs_file, mod_file, ignore = isolated_repo

    paths = [Path(entry) for entry in check_encoding.DEFAULT_PATHS]
    candidates = sorted(
        check_encoding.iter_candidate_files(paths, check_encoding.DEFAULT_EXTENSIONS, ignore)
    )

    expected = [docs_file, mod_file]
    assert [candidate.resolve() for candidate in candidates] == expected
    assert [candidate.as_posix() for candidate in candidates] == [
        path.relative_to(repo_root).as_posix() for path in expected
    ]


def test_scan_file_detects_mojibake(tmp_path):
    target = tmp_path / "bad.txt"
    target.write_text("正常な行\n縺ゅ≧ 異常な行\n終わり", encoding="utf-8")

    has_issue, snippet = check_encoding.scan_file(target)

    assert has_issue is True
    assert "縺ゅ≧" in snippet


def test_scan_file_ignores_clean_text(tmp_path):
    target = tmp_path / "good.txt"
    target.write_text("ただのテキスト", encoding="utf-8")

    has_issue, snippet = check_encoding.scan_file(target)

    assert has_issue is False
    assert snippet == ""
