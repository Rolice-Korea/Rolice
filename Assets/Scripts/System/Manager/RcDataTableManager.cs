using UnityEngine;
using Engine;

public class RcDataTableManager : RcSingletonMono<RcDataTableManager>
{
    [SerializeField] private RcFaceSkinRegistry  faceSkinRegistryRef;
    [SerializeField] private RcEdgeSkinDataTable edgeSkinDataTableRef;

    public static RcFaceSkinRegistry  FaceSkinRegistry  => Instance.faceSkinRegistryRef;
    public static RcEdgeSkinDataTable EdgeSkinDataTable => Instance.edgeSkinDataTableRef;
}
