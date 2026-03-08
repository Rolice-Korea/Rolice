using UnityEngine;
using Rolice;

[CreateAssetMenu(fileName = "NewTileType", menuName = "Rolice/Tile/Type")]
public class RcTileTypeSO : ScriptableObject
{
    [Header("Visual")]
    public GameObject Prefab;

    [Header("Property Flags (에디터용)")]
    public bool bHasColor;
    public bool bHasTeleport;
}
