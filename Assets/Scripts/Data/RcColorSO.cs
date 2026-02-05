using UnityEngine;

[CreateAssetMenu(fileName = "NewColor", menuName = "Rolice/Color Definition")]
public class RcColorSO : ScriptableObject
{
    public string DisplayName;

    [Header("Materials")]
    public Material DiceMaterial;
    public Material TileMaterial;
    public Material EffectMaterial;
}
