using UnityEngine;


public class RcDiceTileInteractor : MonoBehaviour
{
    private RcDicePawn pawn; // 역참조 (타일 행동이 Pawn 정보를 필요로 함)
    
    private RcLevelManager LevelManager => RcLevelManager.Instance;

    public void Initialize(RcDicePawn dicePawn)
    {
        pawn = dicePawn;
    }

    // 타일에 진입할 때 호출됩니다
    public void OnEnterTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        
        if (tile == null) return;
        
        // 타일 행동 실행
        ITileBehavior behavior = tile.GetBehavior(tile.TileObject);
        behavior?.OnEnter(pawn);
    }
    
    // 타일에서 나갈 때 호출됩니다
    public void OnExitTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        
        if (tile == null) return;
        
        // 타일 행동 실행
        ITileBehavior behavior = tile.GetBehavior(tile.TileObject);
        behavior?.OnExit(pawn);
    }

    // 타일 진입 가능 여부를 확인합니다 (타일 행동 검증 포함)
    public bool CanEnterTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        
        if (tile == null) return false;
        
        // 타일 행동의 CanEnter 체크
        ITileBehavior behavior = tile.GetBehavior(tile.TileObject);
        
        return behavior == null || behavior.CanEnter(pawn);
    }
}
