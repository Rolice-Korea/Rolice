using UnityEngine;
using Engine;

public class RcDataTableManager : RcSingletonMono<RcDataTableManager>
{
    [SerializeField] private RcFaceDataTable faceDataTableRef;
    [SerializeField] private RcEdgeDataTable edgeDataTableRef;

    public static RcFaceDataTable FaceDataTable => Instance.faceDataTableRef;
    public static RcEdgeDataTable EdgeDataTable => Instance.edgeDataTableRef;
}
