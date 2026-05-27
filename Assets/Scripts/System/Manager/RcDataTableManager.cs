using UnityEngine;
using Engine;

public class RcDataTableManager : RcSingletonMono<RcDataTableManager>
{
    [SerializeField] private RcFaceSkinRegistry  faceSkinRegistryRef;
    [SerializeField] private RcEdgeSkinDataTable edgeSkinDataTableRef;
    [SerializeField] private RcShopDataTable     shopDataTableRef;

    public static RcFaceSkinRegistry  FaceSkinRegistry  => Instance.faceSkinRegistryRef;
    public static RcEdgeSkinDataTable EdgeSkinDataTable  => Instance.edgeSkinDataTableRef;
    public static RcShopDataTable     ShopDataTable      => Instance.shopDataTableRef;
}
