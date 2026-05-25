using UnityEngine;
using Engine;

public class RcDataTableManager : RcSingletonMono<RcDataTableManager>
{
    [SerializeField] private RcFaceSkinRegistry faceSkinRegistryRef;
    [SerializeField] private RcEdgeDataTable    edgeDataTableRef;

    public static RcFaceSkinRegistry FaceSkinRegistry => Instance.faceSkinRegistryRef;
    public static RcEdgeDataTable    EdgeDataTable     => Instance.edgeDataTableRef;
}
