using System.Collections.Generic;
using UnityEngine;
using Rolice;

[CreateAssetMenu(fileName = "NewFaceDataTable", menuName = "Rolice/Face Data Table")]
public class RcFaceDataTable : ScriptableObject
{
    [SerializeField] private RcSkinBundle[] skinBundles;

    [HideInInspector, SerializeField] private RcFaceData[] faceDataEntries;

    private Dictionary<RcFaceSkinType, RcFaceData> faceDatas;

    private void OnValidate()
    {
        RebuildFromBundles();
    }

    private void OnEnable()
    {
        BuildLookup();
    }

    private void RebuildFromBundles()
    {
        if (skinBundles == null) return;
        faceDataEntries = new RcFaceData[skinBundles.Length];
        for (int i = 0; i < skinBundles.Length; i++)
            faceDataEntries[i] = RcFaceData.Build(skinBundles[i].SkinType, skinBundles[i].Colors);
        BuildLookup();
    }

    private void BuildLookup()
    {
        faceDatas = new Dictionary<RcFaceSkinType, RcFaceData>();
        if (faceDataEntries == null) return;
        foreach (var entry in faceDataEntries)
            faceDatas[entry.SkinType] = entry;
    }

    public RcFaceData GetFaceData(RcFaceSkinType skinType)
    {
        if (faceDatas == null) BuildLookup();
        if (faceDatas.TryGetValue(skinType, out var faceData))
            return faceData;
        Debug.LogWarning($"[RcFaceDataTable] FaceData not found for SkinType: {skinType}");
        return new RcFaceData();
    }
}
