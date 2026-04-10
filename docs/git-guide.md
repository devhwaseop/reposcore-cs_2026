# git 기초
## Git의 3가지 상태 및 영역
1. Working Directory에서 파일을 수정.

2. 수정된 파일 중 기록할 대상만 Staging Area에 올림.

3. Staging Area에 있는 파일들을 하나의 버전으로 묶어 Repository에 반영.
```
[ Working Directory ] --(Stage)--> [ Staging Area ] --(Commit)--> [ Repository ]
```
---
## 필수 git 명령어 요약 (터미널)
### 저장소 관리
- git init: 현재 디렉토리를 Git 저장소로 초기화. 프로젝트를 시작 시 최소 1회만 사용.
- git status: 현재 작업중인 파일들의 상태(수정, 스테이징 여부 등)를 확인.

### 변경 사항 기록
- git add <파일 경로>: 수정된 특정 파일을 스테이징 영역에 추가.
  - git add . :현재 디렉터리의 모든 파일을 스테이징 시 사용.
    - 본 프로젝트에서는 git add . 의 사용을 금지함 (하단 규칙 참고)
- git commit -m "메시지": 스테이징 영역의 파일들을 하나의 버전으로 기록.

 ### 협업 및 공유
 - git remote add origin <URL>: 내 로컬 저장소를 원격 저장소(GitHub)와 연결.
 - git push origin <브랜치>: 로컬 저장소의 기록을 원격 저장소로 Push(업로드).
 - git pull origin <브랜치>: 원격 저장소의 최신 내용을 Pull(불러오기)해서 합침.

---
## git 표준 구성 요소
  협업과 배포를 위해 깃허브 저장소에 기본적으로 갖추어야 할 구성 파일 정리

### README.md
> 해당 프로젝트에 대한 상세 정보를 제공하는 마크다운(Markdown) 형식의 설명 문서입니다.

- 저장소의 메인 페이지에 추가되며, 프로젝트의 목적, 설치 방법, 사용법 및 기여 가이드를 명시합니다.
- 협업자 및 사용자에게 프로젝트의 전체적인 내용을 제공하고, 코드 분석 전 필수 지식을 전달하여 진입 장벽을 낮춥니다.
- 관련 문서: [GitHub Docs - README 정보 및 작성 가이드](https://docs.github.com/ko/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes)

### LICENSE
> 소프트웨어의 이용 조건 및 저작권 범위를 규정하는 법적 문서입니다. (MIT License 등)

- 오픈소스 프로젝트의 경우, 라이선스가 명시되지 않으면 제3자의 코드 재사용 및 수정이 법적으로 제한될 수 있습니다.
- 관련 문서: [GitHub Docs - 저장소 라이선스 지정 가이드](https://docs.github.com/ko/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository)

### .gitignore
> 프로젝트의 버전 관리 시스템(Git)에서 의도적으로 제외할 파일 및 디렉토리 목록을 정의하는 설정 파일입니다.

- 로컬 환경의 임시 파일, 종속성 라이브러리, 보안 민감 정보(API Key 등)가 원격 저장소에 업로드되지 않도록 차단합니다.
- 관련 문서: [GitHub Docs - 파일 무시(.gitignore) 설정 방법](https://docs.github.com/ko/get-started/git-basics/ignoring-files)

---
## 스테이징 시 주의사항
- 본 프로젝트에서는 의도하지 않은 파일 커밋을 방지하기 위해
일괄 스테이징 방식을 금지합니다.

### GUI 사용 시
- "모든 변경 사항 스테이징(+)" 버튼 사용 금지
- 반드시 개별 파일 단위로 스테이징

### CLI 사용 시
- `git add .` 사용 금지
- `git add *` 사용 금지
- `git add <파일 경로>` 방식만 사용