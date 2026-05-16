using UnityEngine;
using Engine;

public class RcDataTableManager : RcSingletonMono<RcDataTableManager>
{
    [SerializeField] private RcFaceSkinRegistry faceSkinRegistryRef;
    [SerializeField] private RcEdgeDataTable    edgeDataTableRef;
    [SerializeField] private RcShopDataTable    shopDataTableRef;

    public static RcFaceSkinRegistry FaceSkinRegistry => Instance.faceSkinRegistryRef;
    public static RcEdgeDataTable    EdgeDataTable     => Instance.edgeDataTableRef;
    public static RcShopDataTable    ShopDataTable     => Instance.shopDataTableRef;
}
