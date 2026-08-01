해당 프로젝트는 골드메탈님의 유튜브에 업로드 된 "[유니티 게임개발] 심플한🏃플랫포머.U18C2" 강좌를 기반으로 작성되었음을 알립니다.

첫 유니티 프로젝트입니다.

---

# Simple 2D Platformer Game

유니티 2D 물리 엔진과 C# 스크립팅을 활용하여 플레이어 제어, 착지/몬스터 밟기 판정, 적 패턴, 스테이지/UI 관리 및 효과음 시스템을 구현한 기초 2D 플랫폼 프로젝트입니다.

---

##  주요 핵심 기능 및 배운점 (Core Features)

### 1. 플레이어 조작 및 착지 (`PlayerMove.cs`)
- BoxCast 기반 바닥 감지: 단일 Raycast 대신 Physics2D.BoxCast로 캐릭터 좌우 전체 폭을 커버
- 가변 점프 높이 제어: Space 키 입력 시간에 따라 linearVelocity.y를 감쇄시켜 숏점프/하이점프 조작감 구현
- 피격 무적 및 넉백 판정: 피격 시 PlayerDamaged 레이어 전환 및 Lighting 함수 인보크를 통한 점멸 효과, 적 위치 기준 반대 방향 넉백 구현.
- 낙사 및 위치 리셋: Y 좌표가 일정 이하로 떨어질 경우 체력 차감 및 안전 구역 지정 위치로 플레이어 리셋.

### 2. 적 밟기 및 피격 판정 (`PlayerMove.cs`, `EnemyMove.cs`)
- 상단 밟기 공격: 플레이어의 Y 속도가 음수(linearVelocity.y < 0)이고 적보다 높은 위치에서 충돌 시 OnAttack 함수 실행 (적 제거 + 플레이어 반동 점프).
- 레이어 충돌 무시 제어: Physics2D.IgnoreLayerCollision을 활용하여 플레이어가 상단에서 내려오는 상황에만 감지 레이어를 유연하게 제어함으로써 피격 무적시에도 적 공격 가능하도록 구현.
- 적 AI (지형 감지): EnemyMove에서 Raycast로 절벽을 감지하여 자동으로 방향 전환, Invoke 기반 랜덤 이동 패턴 구현.

### 3. 게임 루프 및 데이터 관리 (`GameManager.cs`)
- 스테이지 전환: 피니시 라인 도달 시 다음 스테이지 오브젝트 활성화 및 플레이어 위치 초기화.
- 점수 및 체력 UI 연동: 스테이지별 획득 점수(stagePoint)와 누적 점수(totalPoint)를 계산하여 TextMeshPro에 실시간 반영.
- 게임 오버 및 클리어 처리: 체력 소진 또는 최종 스테이지 클리어 시 Time.timeScale = 0 정지 및 결과 UI / 재시도 버튼 활성화.

### 4. 사운드 관리 시스템 (`PlayerMove.cs`)
- 점프, 공격, 피격, 아이템 획득, 사망, 스테이지 클리어로 6가지 상황에 맞춰 AudioSource.Play() 실행.

---

##  기술 스택 (Tech Stack)

| 구분 | 내용 |
| :--- | :--- |
| **Engine** | Unity 2D (Physics2D, UI, Audio) |
| **Language** | C# |
| **UI System** | Unity UI, TextMeshPro (TMP) |

---

## 📂 스크립트 구조 및 역할 (Architecture)

```text
Assets/Scripts/
├── PlayerMove.cs     # 플레이어 이동/점프/피격/공격/사운드 로직
├── EnemyMove.cs      # 적 AI 랜덤 이동/절벽 감지/사망 애니메이션
├── GameManager.cs    # 스테이지 전환/점수 합산/UI 및 게임오버 관리
└── Item.cs           # 아이템 점수 데이터 구조
