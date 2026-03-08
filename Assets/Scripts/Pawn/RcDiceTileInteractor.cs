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
        RcTileBase tile = LevelManager.GetTile(pos);
        if (tile == null) return;

        tile.OnDiceEnter(pawn);
    }

    public void OnExitTile(Vector2Int pos)
    {
        RcTileBase tile = LevelManager.GetTile(pos);
        if (tile == null) return;

        tile.OnDiceLeave(pawn);
    }

    public bool CanEnterTile(Vector2Int pos)
    {
        RcTileBase tile = LevelManager.GetTile(pos);
        if (tile == null) return false;

        return tile.CanEnter();
    }
}
