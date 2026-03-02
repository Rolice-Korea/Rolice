using UnityEngine;

/// 타일 타입 정의 SO — 프리팹, 규칙, 클리어 추적 여부를 한 곳에서 관리
/// LevelManager가 이 SO를 기반으로 타일을 생성하고 RcTileRuleRunner를 구성한다
[CreateAssetMenu(fileName = "NewTileType", menuName = "Rolice/Tile/Type")]
public class RcTileTypeSO : ScriptableObject
{
    [Header("Visual")]
    [Tooltip("타일 메쉬 프리팹 (RcTileRuleRunner 없어도 됨, 런타임에 동적 추가)")]
    public GameObject Prefab;

    [Header("Data Template")]
    [Tooltip("타일 데이터 프로토타입. RcColorTileData 할당 시 색상 타일로 취급. null이면 기본 RcTileData 사용.")]
    [SerializeReference] public RcTileData TileDataTemplate;

    [Header("Rules")]
    [Tooltip("이 타일에 적용할 Rule 목록")]
    public RcTileRuleSO[] Rules;

    [Header("Clear Tracking")]
    [Tooltip("이 타일이 레벨 클리어 조건에 포함되는지 여부")]
    public bool RequiresClearTracking;
}
