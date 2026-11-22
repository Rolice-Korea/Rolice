# Pawn 시스템
> 책은 시스템의 구성 요소와 관계 및 동작 원리를 설명하는 아키텍처이다.

## 개요
Pawn은 그리드 위를 굴러다니는 주사위 플레이어다. RcDicePawn이 이동, 면 상태, 타일 상호작용 컴포넌트를 묶는 파사드이고, 면 색 데이터와 스킨 적용 로직은 분리되어 있다.

## 구성 요소

* RcDicePawn (Assets/Scripts/Pawn/RcDicePawn.cs)
  * 하위 컴포넌트 조율 파사드. 이동 검증과 이벤트 연결
* RcDiceMovement (Assets/Scripts/Pawn/RcDiceMovement.cs)
  * 그리드 좌표 보유, 굴리기/텔레포트/막힘 기울기 연출
* RcDiceFaceController (Assets/Scripts/Pawn/RcDiceFaceController.cs)
  * 면 데이터 보유, 비주얼 갱신, 패배/승리 연출
* RcDiceFaceData (Assets/Scripts/Pawn/RcDiceFaceData.cs)
  * 6면 색 상태 구조체
* RcDiceSkinApplier (Assets/Scripts/Pawn/RcDiceSkinApplier.cs)
  * 면 색별 스킨 머티리얼 채우기. 인게임과 로비 공용
* RcDiceTileInteractor (Assets/Scripts/Pawn/RcDiceTileInteractor.cs)
  * 타일 진입/이탈/진입 가능 여부 위임

## 관계
- RcDicePawn은 RequireComponent로 FaceController, Movement, TileInteractor를 요구하고 Awake에서 초기화한다.
- RcDiceFaceController가 RcDiceFaceData 값을 보유하고, UpdateVisuals에서 RcDiceSkinApplier.Apply를 호출한다(스킨은 RcDataTableManager.FaceSkinRegistry, RcSkinSystem.ActiveFaceSkinType).
- RcDiceTileInteractor는 RcLevelManager.GetTile의 RcTileBase에 OnDiceEnter/OnDiceLeave/CanEnter를 호출한다.
- RcDicePawn은 RcGameEvents로 MoveStarted/MoveCompleted를 발행하고 GameLose를 구독한다.
- RcDicePawn.Teleport의 유일한 외부 호출자는 RcTeleportTile::OnDiceEnter이며, duration 없이 targetGrid만 전달한다.

## 동작 원리
* Move : 이동
  * RcDicePawn -> 이동 중이거나 게임 오버면 무시. 목표 타일이 없거나 진입 불가면 Lean만 실행하고 끝냄
  * RcDicePawn -> 가능하면 RcDiceMovement.Move로 굴리고 RcDiceFaceController.RotateFaces로 면 데이터를 방향별로 치환
  * RcDiceMovement -> 굴림 시작 시 OnMoveStarted, 완료 시 OnMoveCompleted 발생
  * RcDicePawn -> 시작 시 이전 타일 OnExitTile, 완료 시 새 타일 OnEnterTile 호출 후 전역 이벤트 발행
* Teleport : 텔레포트
  * RcTeleportTile -> RcDicePawn.Teleport 호출
  * RcDiceMovement -> 상승 후 목표 좌표로 이동해 하강. OnMoveStarted만 발생시키고 OnMoveCompleted는 발생시키지 않음
* Face Visual : 면 비주얼
  * RcDiceFaceController::faceData 면 색 데이터 정보 관리. 회전은 데이터만 바꿈
  * RcDiceFaceController -> InitializeFaces/ChangeFaceColor 후 UpdateVisuals가 RcDiceSkinApplier로 스킨 머티리얼을 채움. 스킨에 색이 없으면 Default 스킨으로 폴백
* Win/Lose : 승패 연출
  * RcDicePawn -> GameLose 수신 시 FadeToGray, 승리 시 외부가 FlashEmission 호출
  * RcDiceFaceController -> 머티리얼 색을 보간

## 주의
- 인자 없는 Initialize()는 비주얼을 갱신하지 않으며 InitializeFaces 호출 시 갱신된다.
- RcDicePawn.Teleport의 duration 인자는 전달되지 않고 RcDiceMovement의 teleportDuration이 쓰인다.
- Lean은 isMoving만 올리고 inputController.SetProcessing은 호출하지 않는다.
- RcDiceFaceData는 면 배열이 6개가 아니면 에러 로그 후 빈 배열로 대체하며, 바닥색은 MinusY 면이다.
- 면 배열은 RcDiceDirectionType 순서이고 OnValidate가 길이를 6으로 맞춘다.
- Apply는 None 색과 범위 밖 슬롯을 건너뛴다. BakeFaceToSlot은 에디터 전용이다.
