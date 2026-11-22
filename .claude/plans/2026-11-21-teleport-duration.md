# RcDicePawn.Teleport duration 인자 전달

> 도서관 흐름 시험용 계획서. 구현하지 않는다.

## 목표
`RcDicePawn.Teleport`의 `duration` 인자가 버려지는 문제를 고쳐, 호출 쪽이 지정한 연출 시간이 텔레포트에 실제로 쓰이게 한다. 지정하지 않으면 기존처럼 `RcDiceMovement`의 `teleportDuration` 설정값을 쓴다.

## 현재 상태
- `RcDicePawn.Teleport(targetPos, float duration = 0.5f, onComplete = null)`은 `duration`을 쓰지 않고 `movement.Teleport(targetPos, onComplete)`만 호출한다 (`RcDicePawn.cs:79-82`).
- `RcDiceMovement.Teleport(targetPos, onComplete)`는 시간 인자가 없고 (`RcDiceMovement.cs:51`), `TeleportCo`가 직렬화 필드 `teleportDuration`(기본 0.5, `RcDiceMovement.cs:14`)을 `halfDuration`으로 계산해 쓴다 (`RcDiceMovement.cs:210`).
- `Teleport`의 호출자는 `RcTeleportTile.OnDiceEnter` 하나이고 `dice.Teleport(targetGrid)`로 duration을 넘기지 않는다 (`RcTeleportTile.cs:18`).

## 접근 방식
`RcDiceMovement.Teleport`와 `TeleportCo`에 선택적 duration 인자를 추가하고 `RcDicePawn`이 그대로 넘긴다.
- `RcDicePawn.Teleport`의 기본값을 `0.5f`에서 `-1f`(미지정)로 바꾼다. 지금의 `0.5f` 기본값을 그대로 전달하면, 프리팹에서 `teleportDuration`을 바꿔 둔 경우 그 설정을 덮어쓰게 되기 때문이다.
- `RcDiceMovement`는 `duration < 0`이면 `teleportDuration`을 쓰고, 아니면 전달값을 쓴다.
- 대안(버림): `RcDicePawn`의 `duration` 인자를 삭제. 호출 시그니처가 바뀌고 의도(인자로 조절)를 포기하는 셈이라 택하지 않았다.

## 변경 파일
| 파일 | 변경 종류 | 내용 |
|------|-----------|------|
| Assets/Scripts/Pawn/RcDicePawn.cs | 수정 | `Teleport`의 `duration` 기본값을 `-1f`로, `movement.Teleport(targetPos, duration, onComplete)`로 전달 |
| Assets/Scripts/Pawn/RcDiceMovement.cs | 수정 | `Teleport(targetPos, float duration, onComplete)`와 `TeleportCo`에 duration 추가, 미지정이면 `teleportDuration` |

## 구현 단계
1. `RcDiceMovement.Teleport`와 `TeleportCo`에 `float duration = -1f`를 추가한다. 코루틴 안에서 `float total = duration >= 0f ? duration : teleportDuration;`로 `halfDuration`을 계산한다. 확인: 컴파일 에러 없음.
2. `RcDicePawn.Teleport`의 기본값을 `-1f`로 바꾸고 `movement.Teleport(targetPos, duration, onComplete)`를 호출한다. 확인: 컴파일 에러 없음.
3. 에디터에서 텔레포트 타일을 밟아 연출 시간이 이전과 같은지(0.5초) 확인한다.
4. 임시로 `dice.Teleport(targetGrid, 1.5f)`로 호출해 연출이 느려지는지 확인한 뒤 되돌린다.

## 검증 방법
- 텔레포트 관련 자동 테스트는 확인하지 못했다. 위 3, 4의 수동 확인으로 대신한다.
- 수동: 기본 호출은 연출 시간이 그대로, 명시한 값은 그 시간으로 연출되는지.

## 위험 요소 및 가정
- 현재 유일한 호출자가 duration을 넘기지 않으므로 이 수정만으로 눈에 보이는 동작은 바뀌지 않는다. (가정: 앞으로 호출자가 duration을 쓸 계획이 있어서 고치는 것.)
- `duration`이 0이면 `halfDuration`이 0이 되어 `Time.deltaTime / halfDuration`이 무한대가 된다. 이 경우 `t`는 `Mathf.Min`으로 1이 되어 즉시 끝나지만 `deltaTime`이 0인 프레임에서는 NaN이 될 수 있다. 0 이하 값을 어떻게 다룰지는 승인 때 정한다. (이 계획은 0을 허용하고 음수만 미지정으로 본다.)

## 범위 밖
- `OnMoveCompleted`가 텔레포트에서 발생하지 않는 동작.
- `rollDuration` 등 다른 연출 시간.
