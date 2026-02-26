using UnityEngine;

public class RcDiceTileInteractor : MonoBehaviour
{
    private RcDicePawn pawn;
    private RcLevelManager LevelManager => RcLevelManager.Instance;

    public void Initialize(RcDicePawn dicePawn)
    {
        pawn = dicePawn;
    }

    public void OnEnterTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        if (tile == null) return;

        var runner = tile.TileObject.GetComponent<RcTileRuleRunner>();
        runner?.OnDiceEnter(pawn, tile);
    }

    public void OnExitTile(Vector2Int pos)
    {
        // 현재 OnExit 동작을 사용하는 Rule이 없음
    }

    public bool CanEnterTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        if (tile == null) return false;

        return tile.CanEnter(pawn);
    }
}
