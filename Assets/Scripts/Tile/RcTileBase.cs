using UnityEngine;
using Rolice;

public abstract class RcTileBase : MonoBehaviour
{
    public virtual void Construct(RcTileData tileData, Vector2Int pos) { gridPos = pos; }
    public virtual void OnDiceEnter(RcDicePawn dice) { }
    public virtual void OnDiceLeave(RcDicePawn dice) { }
    public virtual bool CanEnter() { return true; }
    public virtual void OnCleared() { }

    public bool IsClearable() { return bClearable; }
    public bool IsClear() { return bClear; }
    public void Clear()
    {
        bClear = true;
        OnCleared();
        RcLevelManager.Instance?.ClearColorTile(gridPos);
    }

    protected Vector2Int gridPos;
    [SerializeField] protected bool bClearable = true;
    [SerializeField] protected bool bClear = false;
}
