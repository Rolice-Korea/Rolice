using UnityEngine;

/// 행동: 타일 색상을 랜덤으로 변경하고 StoneCount를 리셋
[CreateAssetMenu(fileName = "RandomizeColorAction", menuName = "Rolice/Tile/Action/Randomize Color")]
public class RcRandomizeColorActionSO : RcTileActionSO
{
    [Header("랜덤 색상 풀")]
    [Tooltip("랜덤으로 선택될 색상 목록")]
    public RcColorSO[] AvailableColors;

    [Header("초기 Material")]
    [Tooltip("색상 변경 후 적용할 기본 Material (비어있으면 새 Color의 TileMaterial 사용)")]
    public Material DefaultMaterial;

    public override void Execute(RcDicePawn pawn, RcTileData tileData)
    {
        if (AvailableColors == null || AvailableColors.Length == 0)
        {
            Debug.LogWarning("[RandomizeColorAction] AvailableColors가 비어있습니다.");
            return;
        }

        if (tileData is not RcColorTileData colorTile)
        {
            Debug.LogWarning("[RandomizeColorAction] RcColorTileData가 아닌 타일에 적용되었습니다.");
            return;
        }

        // 현재 색상과 다른 색상 선택
        RcColorSO newColor = PickRandomColor(colorTile.Color);
        colorTile.Color = newColor;

        // HitCount 리셋 (스톤 타일인 경우)
        if (tileData is RcStoneTileData stoneTile)
            stoneTile.HitCount = 0;

        // Material 적용
        ApplyMaterial(tileData, newColor);
    }

    private RcColorSO PickRandomColor(RcColorSO currentColor)
    {
        // 색상이 1개뿐이면 그대로 반환
        if (AvailableColors.Length == 1)
            return AvailableColors[0];

        // 현재 색상과 다른 색상 선택 시도
        for (int i = 0; i < 10; i++)
        {
            RcColorSO candidate = AvailableColors[Random.Range(0, AvailableColors.Length)];
            if (candidate != currentColor)
                return candidate;
        }

        // 10번 시도 후에도 같은 색이면 그냥 반환
        return AvailableColors[Random.Range(0, AvailableColors.Length)];
    }

    private void ApplyMaterial(RcTileData tileData, RcColorSO newColor)
    {
        MeshRenderer renderer = tileData.TileObject.GetComponentInChildren<MeshRenderer>();
        if (renderer == null) return;

        if (DefaultMaterial != null)
            renderer.material = DefaultMaterial;
        else if (newColor.TileMaterial != null)
            renderer.material = newColor.TileMaterial;
    }
}
