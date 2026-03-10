using UnityEngine;
using Rolice;

public class RcColorTile : RcTileBase
{
    private RcColorType colorType;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        // 타일의 시각적 요소를 담당하는 자식 렌더러를 찾습니다. (일반적으로 프리팹 내부에 MeshRenderer가 하나 존재함)
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public override void Construct(RcTileData tileData)
    {
        bClearable = false; // 클리어 대상이 아님
        bClear = false;

        ChangeColor(tileData.colorType);
    }

    public override void OnDiceEnter(RcDicePawn dice)
    {
        RcColorType diceColorType = dice.GetBottomColor();
        
        if (diceColorType == colorType) 
            return;

        dice.ChangeFaceColor(diceColorType, colorType);

        ChangeColor(diceColorType);
    }

    private void ChangeColor(RcColorType newColorType)
    {
        colorType = newColorType;
        
        if (meshRenderer == null) return;
        if (RcDataTableManager.FaceDataTable == null) return;

        // 현재 사용 중인 스킨 데이터를 가져옴
        var skinData = RcDataTableManager.FaceDataTable.GetFaceData(RcFaceSkinType.Default);
        if (skinData == null) return;

        Material skinMat = skinData.GetFaceMaterial(colorType);
        if (skinMat != null)
        {
            var mats = meshRenderer.materials;
            if (mats.Length > 0)
            {
                mats[0] = skinMat; // 기본적으로 타일 면은 첫 번째 슬롯 머티리얼을 사용한다고 가정
                meshRenderer.materials = mats;
            }
        }
    }
}
