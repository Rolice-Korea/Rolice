using UnityEngine;
using Engine;

public class RcDataTableManager : RcSingletonMono<RcDataTableManager>
{
    [SerializeField] private RcFaceDataTable faceDataTableRef;
    [SerializeField] private RcEdgeDataTable edgeDataTableRef;
    [SerializeField] private RcShopDataTable shopDataTableRef;

    public static RcFaceDataTable FaceDataTable => Instance.faceDataTableRef;
    public static RcEdgeDataTable EdgeDataTable => Instance.edgeDataTableRef;
    public static RcShopDataTable ShopDataTable => Instance.shopDataTableRef;
}
