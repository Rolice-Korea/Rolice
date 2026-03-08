using UnityEngine;
using Rolice;

public abstract class RcTileBase : MonoBehaviour
{
    public virtual void Construct(RcTileData tileData) { }
    public virtual void OnDiceEnter(RcDicePawn pawn) { }
    public virtual void OnDiceLeave(RcDicePawn pawn) { }

    public bool IsClearable() { return bClearable; }
    public bool IsClear() { return bClear; }
    public void Clear()
    {
        bClear = true;

        var pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        RcLevelManager.Instance?.ClearColorTile(pos);
    }

    [SerializeField] protected bool bClearable = true;
    [SerializeField] protected bool bClear = false;
}
