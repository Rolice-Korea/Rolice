# Unity 세팅 스텝

작업 후 Unity에서 직접 해줘야 하는 것들.

---

## [2026-03-20] RcPlayerState + UGS Cloud Save + 랜덤 생성기

### 1. UGS 패키지 설치 확인
manifest.json에 3개 패키지 추가됨. Unity 열면 자동 다운로드.
버전 오류 시 Package Manager → Add package by name으로 수동 설치:
- `com.unity.services.core` 1.12.5
- `com.unity.services.authentication` 3.3.3
- `com.unity.services.cloudsave` 3.1.1

### 2. UGS 프로젝트 연결
1. **Edit → Project Settings → Services** 에서 Unity 계정으로 프로젝트 연결
2. [dashboard.unity3d.com](https://dashboard.unity3d.com) → Cloud Save 서비스 활성화

### 3. 로비 UI — OnProgressChanged 구독
스테이지 클리어/클라우드 동기화 후 로비가 자동 갱신되려면 구독 필요.

```csharp
// 구독 (OnEnable or Start)
RcPlayerState.Instance.OnProgressChanged += RefreshStageList;

// 해제 (OnDestroy)
RcPlayerState.Instance.OnProgressChanged -= RefreshStageList;
```

OnProgressChanged가 발생하는 시점:
- 앱 시작 시 클라우드 동기화 완료 후
- 스테이지 클리어 후 (`RecordStageClear` → `Save` → `NotifyChanged`)
- `ResetAll()` 호출 후

### 4. UGS 붙이고 떼기
`RcAppBootstrap.cs` 한 줄로 제어:
```csharp
RcUgsServices.Register(new RcUgsProvider()); // 이 줄 제거 = 로컬 전용 폴백
```
UGS 없어도 게임은 정상 동작 (로컬 JSON 저장만 사용).

### 5. 새 UGS 서비스 추가 방법 (Economy, Leaderboard 등)
1. `System/Ugs/` 에 `IXxxService.cs` 인터페이스 추가
2. `IUgsProvider.cs` 에 프로퍼티 추가: `IXxxService Xxx { get; }`
3. `RcUgsProvider.cs` 에 실 구현체 추가
4. `RcUgsServices.cs` 의 `NullUgsProvider` 에 Null 구현체 추가
5. `RcUgsServices.cs` 에 접근자 추가: `public static IXxxService Xxx => Provider.Xxx;`

`Register()` 시그니처는 그대로 유지됨.

### 6. Generate 탭 사용법
레벨 에디터 Inspector → **Generate 탭**
- Preset: Full(전체격자) / Path(경로형) / Cluster(섬형)
- Size: 너비 × 높이 (3~12)
- Colors: 색상 수 (2~6)
- Fill Ratio: 타일 밀도 (0.3~1.0)
- Turn Mult: 색타일수 × 배율 = MaxTurns
- Seed: -1이면 랜덤, 숫자 고정 시 동일 맵 재현 가능
- **Generate** 클릭 → 현재 열린 레벨 SO에 즉시 적용 (Undo 가능)

별 기준은 자동 계산 (오름차순 보장):
- ★★★ : 색타일수 + 1턴 이내
- ★★  : 색타일수 × 1.25 + 1턴 이내
- ★   : MaxTurns - 1턴 이내

### 7. 데이터 흐름 요약
```
앱 시작
  └─ RcPlayerState.Initialize()
       ├─ 로컬 JSON 즉시 로드 (게임 바로 플레이 가능)
       └─ 백그라운드: UGS 인증 → Cloud Save 로드
            ├─ 클라우드 데이터 있음 → _data 교체 → 로컬 캐시 갱신 → OnProgressChanged
            └─ 클라우드 데이터 없음 → 로컬 데이터 클라우드 업로드

스테이지 클리어
  └─ RcProgressManager.RecordStageClear()
       └─ RcPlayerState.Save()
            ├─ 로컬 즉시 저장
            ├─ 백그라운드 클라우드 동기화
            └─ OnProgressChanged
```

---
