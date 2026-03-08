using System.Collections.Generic;
using UnityEngine;
using Rolice;

[CreateAssetMenu(fileName = "NewEdgeDataTable", menuName = "Rolice/Edge Data Table")]
public class RcEdgeDataTable : ScriptableObject
{
    [SerializeField] private RcEdgeData[] edgeDataEntries;

    private Dictionary<RcEdgeSkinType, RcEdgeData> edgeDatas;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        edgeDatas = new Dictionary<RcEdgeSkinType, RcEdgeData>();
        if (edgeDataEntries == null) return;
        foreach (var entry in edgeDataEntries)
            edgeDatas[entry.SkinType] = entry;
    }

    public RcEdgeData GetEdgeData(RcEdgeSkinType skinType)
    {
        if (edgeDatas == null) BuildLookup();

        if (edgeDatas.TryGetValue(skinType, out var edgeData))
            return edgeData;

        Debug.LogWarning($"[RcEdgeDataTable] EdgeData not found for SkinType: {skinType}");
        return new RcEdgeData();
    }
}
