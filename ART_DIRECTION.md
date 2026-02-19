# Rolice Art Direction Guide

비주얼 퀄리티 폴리싱을 위한 아트 디렉션 레퍼런스.

## Core Identity

**컨셉 키워드**: Neon / Dark / Modern / Minimal / Simple

어둠 속에서 빛나는 다이스와 타일. 불필요한 장식 없이 빛과 색상 자체가 비주얼의 핵심.

## Color System

### Background & Base
| 용도 | Color (Linear) | Hex 근사 | 비고 |
|------|----------------|----------|------|
| 배경 | (0.019, 0.019, 0.019) | `#050505` | 거의 순수 블랙 |
| 바닥 기본색 | (0.05, 0.05, 0.12) | `#0D0D1F` | 약간의 블루 틴트 |
| 다이스/타일 베이스 | (0.102, 0.102, 0.102) | `#1A1A1A` | 다크 그레이 |
| UI 베이스 | (0.15, 0.15, 0.15) | `#262626` | UI 패널 배경 |

### Neon Palette (HDR Emission)
게임 핵심 컬러. 모두 HDR(>1.0) 값으로 블룸과 함께 발광.

| 컬러 | Emission (R, G, B) | 느낌 | SO |
|-------|-------------------|------|-----|
| Cyan | (0.312, 2.5, 2.5) | 시원한 전자광 | `color_cyan` |
| Red | (2.0, 0.066, 0.0) | 강렬한 경고 | `color_red` |
| Yellow | (2.2, 2.5, 0.15) | 따뜻한 골드 | `color_yellow` |
| Orange | (2.5, 0.7, 0.08) | 활기찬 에너지 | `color_orange` |
| Purple | (1.25, 0.312, 2.5) | 신비로운 보라 | `color_purple` |
| Lime | (0.416, 2.5, 0.312) | 생기 있는 그린 | `color_lime` |
| Gray | (0.3, 0.3, 0.3) | 비활성/중립 | - |

### UI Accent Colors
| 용도 | Color (HDR) | 비고 |
|------|-------------|------|
| 그래디언트 A | (1.0, 0.4, 0.2) | 오렌지 (따뜻한 측) |
| 그래디언트 B | (0.3, 1.0, 0.8) | 시안 (차가운 측) |
| 바닥 프레넬 | (0.3, 0.4, 0.8) | 블루 엣지 글로우 |
| 텍스트 글로우 | (1.98, 1.98, 1.98) | 밝은 화이트 HDR |

## Visual Systems

### 1. 3D Neon Glow
- **방식**: Material Emission (HDR) + Post-Processing Bloom
- **다이스**: 다크 그레이 베이스(Metallic=1, Smoothness=0.8) + 색상별 강한 Emission
- **타일**: 동일 구조, 다이스보다 낮은 Emission (약 1/4~1/5 강도)
- **바닥**: NeonFloor 셰이더 — 프레넬 엣지 글로우 + 라이트 블리드

### 2. Post-Processing Stack
| 효과 | 설정 | 목적 |
|------|------|------|
| Bloom | Threshold=1.2, Intensity=1.5, Scatter=0.3 | 네온 발광의 핵심 |
| Tonemapping | ACES | 하이라이트 압축, 영화적 톤 |
| Color Adj. | Contrast=+10, Saturation=-5 | 대비 강화, 채도 약간 억제 |
| Vignette | Intensity=0.15 | 화면 가장자리 어둡게 |
| SSAO | Intensity=0.4 | 깊이감 |

### 3. UI Glow System
- **RcUIHDRImage**: HDR Color + Intensity(0~20) → 블룸으로 자연스러운 UI 발광
- **RcUIGradient**: 코너별 HDR 컬러 그래디언트 + Glow Intensity + Bloom Spread
- **UI-HDR-OutlineGlow 셰이더**: Border + Softness + HDR 글로우 컬러
- **TMP Glow Material**: 텍스트 자체가 발광하는 효과

### 4. Animation Language
DOTween 기반 모션 시스템. 모든 연출은 `RcTweenAnimator` 시퀀스로 구성.
- **Move/Scale/Rotate/Fade/Color/Shake** Config 조합
- Ease 함수로 모션 성격 결정
- Named Sequence → 재사용 가능

### 5. Particle & VFX
- `RcParticleEffect` + `RcParticleEffectFactory` (오브젝트 풀링)
- 결과 화면: 별 팝 파티클 (0.3초 간격 순차 재생)

## Design Principles

### DO
- 어둠을 적극 활용 — 빈 공간도 디자인의 일부
- 빛으로 위계 표현 — 중요한 것일수록 밝게
- HDR + Bloom 조합으로 실제 발광감 연출
- 애니메이션은 짧고 명확하게 (0.2~0.5초)
- 색상은 6색 팔레트 안에서 운용

### DON'T
- 밝은 배경이나 그래디언트 배경 사용 금지
- 장식적 요소(테두리, 패턴, 그림자 레이어) 과다 사용 금지
- 네온 컬러 외 저채도 파스텔/어스톤 사용 금지
- 동시에 3가지 이상 색상 혼합 금지 (시각 노이즈)
- 불필요한 텍스트/라벨 추가 금지

## File Reference

### Shaders
| 셰이더 | 경로 | 용도 |
|--------|------|------|
| NeonFloor | `Assets/Shaders/NeonFloor.shader` | 바닥 프레넬 글로우 |
| UI-HDR-Image | `Assets/Shaders/UI-HDR-Image.shader` | UI HDR 이미지 |
| UI-HDR-OutlineGlow | `Assets/Shaders/UI-HDR-OutlineGlow.shader` | UI 글로우+아웃라인 |
| UI-HDR-Default | `Assets/Shaders/UI-HDR-Default.shader` | UI 기본 HDR |

### Materials
| 종류 | 경로 패턴 |
|------|---------|
| 다이스 | `Assets/ArtAsset/Dice/d_*.mat` |
| 타일 | `Assets/ArtAsset/Tile/t_*.mat` |
| UI | `Assets/ArtAsset/UI/` |
| TMP 글로우 | `Assets/ScriptableObjects/UI/InGame/LiberationSans SDF - InGame Glow.mat` |

### Color SO
`Assets/ScriptableObjects/Color/color_*.asset` — 다이스/타일 머터리얼 매핑

### Post-Processing
`Assets/Scenes/MainScene/MainScene Global Volume Profile.asset`

### UI Scripts
| 스크립트 | 용도 |
|---------|------|
| `Assets/Scripts/UI/Core/RcUIHDRImage.cs` | HDR 컬러 이미지 |
| `Assets/Scripts/UI/Core/RcUIGradient.cs` | 코너 그래디언트 |
| `Assets/Scripts/UI/Core/RcUIManager.cs` | UI 라이프사이클 |

### Animation
| 경로 | 용도 |
|------|------|
| `Assets/Scripts/Animation/Runtime/Core/RcTweenAnimator.cs` | 시퀀스 오케스트레이터 |
| `Assets/Scripts/Animation/Runtime/Implementations/` | Config별 트윈 구현체 |

## Polish Checklist (진행 추적)
- [ ] 타일 클리어 이펙트 강화
- [ ] 다이스 굴림 연출 개선
- [ ] UI 전환 애니메이션 통일
- [ ] 사운드 + 비주얼 싱크
- [ ] 로비 씬 비주얼 정리
- [ ] 파티클 SFX 추가
- [ ] 추가 셰이더 이펙트 (필요시)
