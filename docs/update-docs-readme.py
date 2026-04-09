"""
update_docs_readme.py
---------------------
docs/ 폴더의 마크다운 파일을 탐색하여 /docs/README.md의 문서 목록을 자동으로 갱신합니다.

사용법 (프로젝트 루트에서):
    python docs/update_docs_readme.py           # README.md 갱신
    python docs/update_docs_readme.py --check   # 변경 필요 여부만 확인 (갱신 안 함)

동작:
    1. docs/ 폴더에서 README.md를 제외한 모든 .md 파일을 탐색
    2. 각 파일의 첫 번째 H1(#) 헤더를 제목으로 추출
    3. 파일명 기준 알파벳 순으로 정렬
    4. docs/README.md의 <!-- DOC_LIST_START --> ~ <!-- DOC_LIST_END --> 구간을 자동 업데이트
"""

import re
import sys
from pathlib import Path

# ── 설정 ──────────────────────────────────────────────────────────────────────
DOCS_DIR    = Path(__file__).parent        # 스크립트가 docs/ 안에 위치
README_PATH = DOCS_DIR / "README.md"

MARKER_START = "<!-- DOC_LIST_START -->"
MARKER_END   = "<!-- DOC_LIST_END -->"

DEFAULT_README_TEMPLATE = """\
# 문서 목록

이 폴더의 모든 문서를 자동으로 관리합니다.

{start}
{end}
"""
# ──────────────────────────────────────────────────────────────────────────────


def extract_title(md_path: Path) -> str:
    """마크다운 파일에서 첫 번째 H1 헤더 텍스트를 추출합니다.

    H1이 없으면 파일명(확장자 제거)을 반환합니다.
    """
    try:
        with open(md_path, encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line.startswith("# "):
                    return line[2:].strip()
    except (OSError, UnicodeDecodeError):
        pass
    return md_path.stem


def collect_docs(docs_dir: Path) -> list[tuple[str, str]]:
    """docs/ 폴더에서 README.md를 제외한 .md 파일을 수집하고 정렬합니다.

    Returns:
        [(filename, title), ...] 파일명 오름차순 정렬
    """
    entries = []
    for md_file in sorted(docs_dir.glob("*.md")):
        if md_file.name.lower() == "readme.md":
            continue
        title = extract_title(md_file)
        entries.append((md_file.name, title))
    return entries


def build_list_block(entries: list[tuple[str, str]]) -> str:
    """수집된 문서 목록을 마크다운 리스트 문자열로 변환합니다."""
    if not entries:
        return "_아직 등록된 문서가 없습니다._"
    lines = []
    for filename, title in entries:
        lines.append(f"- [{filename}](./{filename}): {title}")
    return "\n".join(lines)


def extract_current_block(readme_text: str) -> str | None:
    """README.md에서 마커 사이의 현재 목록을 추출합니다.

    마커가 없으면 None을 반환합니다.
    """
    pattern = re.compile(
        rf"{re.escape(MARKER_START)}\n(.*?)\n{re.escape(MARKER_END)}",
        re.DOTALL,
    )
    match = pattern.search(readme_text)
    if match:
        return match.group(1)
    return None


def is_update_needed(readme_path: Path, new_block: str) -> bool:
    """README.md 갱신이 필요한지 확인합니다.

    - README.md가 없으면 → True
    - 마커가 없으면 → True
    - 마커 사이 내용이 새 목록과 다르면 → True
    - 동일하면 → False
    """
    if not readme_path.exists():
        return True

    current_text = readme_path.read_text(encoding="utf-8")

    current_block = extract_current_block(current_text)
    if current_block is None:
        return True

    return current_block.strip() != new_block.strip()


def update_readme(readme_path: Path, new_block: str) -> None:
    """README.md의 마커 구간을 새 목록으로 교체합니다."""
    if not readme_path.exists():
        content = DEFAULT_README_TEMPLATE.format(
            start=MARKER_START, end=MARKER_END
        )
        readme_path.write_text(content, encoding="utf-8")
        print(f"[생성] {readme_path}")

    original = readme_path.read_text(encoding="utf-8")

    if MARKER_START not in original:
        appended = original.rstrip() + f"\n\n{MARKER_START}\n{MARKER_END}\n"
        readme_path.write_text(appended, encoding="utf-8")
        original = appended
        print("[안내] 마커가 없어 파일 끝에 추가했습니다.")

    pattern = re.compile(
        rf"{re.escape(MARKER_START)}.*?{re.escape(MARKER_END)}",
        re.DOTALL,
    )
    replacement = f"{MARKER_START}\n{new_block}\n{MARKER_END}"
    updated = pattern.sub(replacement, original)

    readme_path.write_text(updated, encoding="utf-8")
    print(f"[업데이트] {readme_path}")


def main() -> None:
    check_only = "--check" in sys.argv

    if not DOCS_DIR.is_dir():
        raise FileNotFoundError(f"docs 폴더를 찾을 수 없습니다: {DOCS_DIR}")

    entries = collect_docs(DOCS_DIR)
    new_block = build_list_block(entries)

    if check_only:
        if is_update_needed(README_PATH, new_block):
            print("[감지] docs/*.md 변경 있음 → README.md 갱신이 필요합니다.")
            sys.exit(1)   # 변경 있음
        else:
            print("[스킵] docs/*.md 변경 없음 → README.md는 최신 상태입니다.")
            sys.exit(0)   # 변경 없음

    # 일반 실행: 변경 여부 확인 후 갱신
    if not is_update_needed(README_PATH, new_block):
        print("[스킵] 변경 사항 없음.")
        return

    print(f"[탐색] {len(entries)}개 문서 발견:")
    for name, title in entries:
        print(f"  • {name}: {title}")

    update_readme(README_PATH, new_block)
    print("완료!")


if __name__ == "__main__":
    main()
