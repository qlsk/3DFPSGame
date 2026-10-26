# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

Unity 6 (`6000.3.22f1`), URP 기반 1인칭 슈팅 게임. 씬은 `Assets/01.Scenes/GameScene.unity` 하나이며, 게임 코드는 전부 `Assets/02.Scripts/`에 있다. `Assets/Plugins/CW`(Lean Pool 등), `BloodDecalsAndEffects`, `Workshop`, `TextMesh Pro`는 외부 에셋이므로 수정하지 않는다.

## 명령어

빌드·실행·테스트는 Unity 에디터에서 한다. 자동화된 테스트는 없다 (`02.Scripts/Test/`는 C# 문법 연습용 예제다).

- 포맷 검사 (pre-commit 훅과 동일):
  ```
  dotnet format 3DFPSGame.sln --verify-no-changes --severity warn --include Assets/02.Scripts/
  ```
  `--verify-no-changes`를 빼면 자동 수정된다. `.sln`/`.csproj`는 Unity가 생성하며 git에 포함되지 않으므로 에디터에서 한 번 열어 생성된 상태여야 한다. 코드 스타일은 `.editorconfig`를 따른다.
- 훅 도구 설치: `dotnet tool restore` (Husky.Net)

## 커밋 규칙

`.husky/commit-msg` 훅이 강제한다.

```
<type>[(scope)][!]: <설명>

<세부 내용 최소 한 줄>
```

- 제목과 본문 사이 빈 줄 필수, **본문 필수**.
- type: `feat fix docs style refactor perf test build ci chore revert` + 게임용 `gameplay asset level balance ui audio vfx anim content`
- 기존 커밋은 한국어 설명을 사용한다 (예: `feat: 미니맵 확대 축소`).

## 아키텍처

- **게임 상태 게이트**: `GameManager`는 싱글톤(`GameManager.Instance`)이며 `GameState`(Ready → Playing → GameOver)를 가진다. 입력·AI를 처리하는 모든 `Update()`는 맨 앞에서 `State != GameState.Playing`이면 return한다. 새 게임플레이 스크립트도 같은 가드를 넣는다.
- **커서 = 입력 차단**: `CursorManager`가 Left Alt를 누르는 동안 커서를 해제한다. 사격·폭탄·카메라 회전 스크립트는 `Cursor.visible`이 true이면 입력을 무시한다.
- **데미지 시스템**: 피해를 받는 모든 것은 `IDamageable.TakeDamage(DamageInfo)`를 구현한다 (`Player`, `Zombie`, `Drum`). `DamageInfo`는 값·피격 지점·노멀을 담은 struct. 공격 측은 `GetComponent<IDamageable>()?.TakeDamage(...)`로 호출한다. 범위 피해(`Bomb`, `Drum`)는 `Physics.OverlapSphere`에 `"Drum"`, `"Monster"` 레이어 마스크를 사용하므로, 범위 피해를 받아야 하는 새 오브젝트는 레이어 설정이 필요하다.
- **플레이어는 기능별 컴포넌트로 분리**: `Player`(체력), `PlayerMove`(CharacterController 이동·점프·스태미나), `PlayerGunFire`(카메라 기준 레이캐스트 사격·재장전), `PlayerBombFire`(차지 후 Rigidbody 투척), `PlayerRotate`. UI는 데이터를 끌어오는 방식이다: `UI_PlayerStats`가 같은 오브젝트의 플레이어 컴포넌트들을 `GetComponent`로 잡고 `LateUpdate`에서 공개 프로퍼티를 읽어 슬라이더를 갱신한다.
- **좀비 AI**: `Zombie`는 enum 기반 FSM (`Idle/Patrol/Trace/Comeback/Attack/Hit/Death`)을 `Update`의 switch로 돌린다. Hit/Death는 코루틴으로 처리하며, `TakeDamage`에서 `StopAllCoroutines()` 후 상태를 전이한다. 플레이어는 `"Player"` 태그로 찾는다. 체력바는 `ZombieHealthBar`로 분리되어 있다.
- **카메라**: 메인 카메라는 `CameraFollow`(위치)와 `CameraRotate`(마우스 누적 회전)로 분리. `MinimapCamera`는 플레이어 위에서 직교 투영으로 따라가며 Render Texture로 UI에 출력되고, 줌은 키(0/9)와 `UI_Minimap` 버튼 양쪽에서 호출된다.
- **오브젝트 풀링**: 피격·폭발 파티클은 Lean Pool(`LeanPool.Spawn/Despawn`)을 사용하고, 재사용 시 `Stop(true, StopEmittingAndClear)` 후 `Play()`한다. 폭탄·드럼 폭발 이펙트·혈흔은 아직 `Instantiate/Destroy`를 쓴다.
- **입력**: Active Input Handling이 Both이지만 코드는 레거시 `Input` API를 사용한다. 커스텀 축 `Sprint`, `Reload`가 Input Manager에 정의되어 있다.
- **로그**: `Debug.Log` 대신 `GameLogger.Log`를 쓴다. `[Conditional("ENABLE_LOG")]`라서 `ENABLE_LOG` 심볼(현재 Standalone에 정의됨)이 없으면 호출 자체가 컴파일에서 제거된다. Warning/Error는 항상 출력된다.

## 코드 컨벤션

- private 필드는 `_camelCase`, 인스펙터 노출은 `[SerializeField] private` + 읽기 전용 프로퍼티(`public float MaxHealth => _maxHealth;`).
- 인스펙터 필드는 `[Header("스텟")]`, `[Header("UI")]` 등으로 묶는다.
- 코루틴 메서드명은 `xxx_Coroutine` 접미사, UI 스크립트는 `UI_` 접두사, 버튼 콜백은 `OnClickXxx`.
- 주석은 한국어로 작성하며, 처리 단계를 `// 1. ... // 2. ...` 식으로 적는 스타일이 많다.
- `[SerializeField]` 필드 이름을 바꾸면 씬/프리팹 참조가 끊기므로 `[FormerlySerializedAs]`를 고려한다.

## 코드 리뷰

리뷰 요청 시 `Docs/CodeReviewGuide.md`의 기준(Critical/High/Medium/Low 중요도, 오탐 방지 규칙, 결과 형식: 총평 → 발견된 문제 → 설계 개선 제안 → 추가 검증 필요 사항 → 잘 작성된 점 → 최종 요약)을 따른다. 핵심은 실제로 발생 가능한 버그를 우선하고, 불필요한 추상화나 리팩터링을 강요하지 않는 것이다.

<!-- imported-from: gemini:project:instructions -->
﻿# 프로젝트 지침

이 저장소는 Unity와 C#으로 제작하는 게임 프로젝트이다.

코드 리뷰를 시작하기 전에 다음 문서를 읽고 해당 규칙을 반드시 적용한다.

@./Docs/CodeReviewGuide.md

문서를 읽을 수 없다면 규칙을 임의로 추정하지 말고 해당 사실을 알린다.

## 기본 원칙

- 코드 리뷰는 기본적으로 읽기 전용으로 수행한다.
- 요청받지 않았다면 코드를 수정하거나 Commit, Push하지 않는다.
- 리뷰와 관련 없는 파일은 수정하지 않는다.
- 변경된 코드의 의도와 주변 코드를 함께 확인한다.
- 변경 사항으로 인해 영향을 받을 수 있는 호출부와 의존 관계를 검토한다.
- 기존 문제와 이번 변경으로 발생한 문제를 구분한다.
- 확실하지 않은 문제를 버그라고 단정하지 않는다.
- 실행하지 않은 테스트나 확인하지 않은 결과를 검증했다고 표현하지 않는다.
- 답변은 한국어로 작성한다.

## 리뷰 우선순위

다음 항목을 우선적으로 검토한다.

1. 실제 기능 오류 및 런타임 예외
2. 게임 상태 및 오브젝트 생명주기 문제
3. Unity 직렬화 및 에셋 참조 문제
4. 메모리 누수 및 심각한 성능 문제
5. 유지보수성과 설계 구조 문제

단순한 스타일 차이와 근거 없는 최적화 제안은 지양한다.

## 리뷰 범위 및 Commit 처리

- 여러 Commit이 한 번에 Push된 경우 해당 Push에 포함된 모든 Commit을 리뷰 대상으로 포함한다.
- 마지막 Commit만 확인하지 않고, Push 이전 Commit과 Push 이후 Commit 사이의 전체 변경 사항을 확인한다.
- 각 Commit의 변경 내용과 의도를 확인하고, Commit 간 상호작용으로 발생할 수 있는 문제도 검토한다.
- 개별 Commit에서 발생한 문제가 이후 Commit에서 수정되었는지 확인한다.
- 최종 코드에 이미 해결된 문제를 현재 존재하는 버그처럼 보고하지 않는다.
- 리뷰 결과는 최종 변경 사항을 기준으로 작성하고, 필요한 경우 문제가 발생한 Commit을 함께 명시한다.
- 리뷰 범위를 확인할 수 없다면 마지막 Commit만 임의로 리뷰하지 않고 확인 가능한 범위와 한계를 명시한다.

## 리뷰 결과

- `CodeReviewGuide.md`에 정의된 Critical, High, Medium, Low 기준을 사용한다.
- 발견한 문제는 중요도가 높은 순서대로 정리한다.
- 각 문제에 파일 위치, 원인, 발생 조건, 예상 영향, 수정 방향을 포함한다.
- 확인되지 않은 사항은 별도의 검증 필요 항목으로 구분한다.
- 문제가 발견되지 않았다면 억지로 지적하지 않는다.
- 수정 방안은 제안하되 요청받지 않았다면 직접 수정하지 않는다.
