using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class RcTileData
{
    [Header("Basic Info")]
    public string TileID;

    public bool bCanEnter = true;
    
    [Header("Special Behavior (optional)")]
    public RcTileBehaviorSO BehaviorSO;
    
    [NonSerialized] private ITileBehavior behaviorInstance;
    
    public GameObject TileObject;

    public void Setup(GameObject tileObject)
    {
        this.TileObject = tileObject;
    }
    
    public ITileBehavior GetBehavior(GameObject tileObject)   
    {
        if (behaviorInstance == null && BehaviorSO != null)
        {
            behaviorInstance = BehaviorSO.CreateBehavior(tileObject, this);
        }
        return behaviorInstance;
    }
    
    public bool CanEnter(RcDicePawn pawn)
    {
        return bCanEnter;
    }
    
    public RcTileData Clone()
    {
        return new RcTileData
        {
            TileID = this.TileID,
            bCanEnter = this.bCanEnter,
            BehaviorSO = this.BehaviorSO
        };
    }
}
