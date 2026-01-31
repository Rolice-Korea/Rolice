using Engine;
using UnityEngine;

public class RcDataManager : RcSingleton<RcDataManager>
{
    public RcMaterialDataBaseSO MaterialDatabase;

    public void Initialize(RcMaterialDataBaseSO materialDatabase)
    {
        MaterialDatabase = materialDatabase;
    }
}
