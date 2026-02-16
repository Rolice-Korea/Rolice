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

        ITileBehavior behavior = tile.GetBehavior(tile.TileObject);
        behavior?.OnEnter(pawn);
    }

    public void OnExitTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        if (tile == null) return;

        ITileBehavior behavior = tile.GetBehavior(tile.TileObject);
        behavior?.OnExit(pawn);
    }

    public bool CanEnterTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        if (tile == null) return false;

        ITileBehavior behavior = tile.GetBehavior(tile.TileObject);
        return behavior == null || behavior.CanEnter(pawn);
    }
}
