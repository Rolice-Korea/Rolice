using UnityEngine;

public class RcDiceFaceController : MonoBehaviour
{
    [Header("Face Renderers")]
    [SerializeField] private MeshRenderer[] faceRenderers = new MeshRenderer[6];

    [Header("Initial Face Colors")]
    [SerializeField] private ColorType[] initialFaces = new ColorType[6];

    private DiceFaceData faceData;

    public void Initialize()
    {
        faceData = new DiceFaceData(initialFaces);
        UpdateVisuals();
    }

    public void RotateFaces(Vector2Int direction)
    {
        faceData = faceData.Rotate(direction);
    }
    
    public ColorType GetBottomColor()
    {
        return faceData.GetBottomColor();
    }
    
    public ColorType GetFaceColor(int faceIndex)
    {
        return faceData.GetFaceColor(faceIndex);
    }
    
    
    private void UpdateVisuals()
    {
        var materialDB = RcDataManager.Instance.MaterialDatabase;
        
        for (int i = 0; i < 6; i++)
        {
            if (faceRenderers[i] == null) continue;
            
            ColorType faceColor = faceData.GetFaceColor(i);
            Material material = materialDB.GetMaterial(faceColor, MaterialUsageType.Dice);
            
            if (material != null)
            {
                faceRenderers[i].material = material;
            }
            else
            {
                Debug.LogWarning($"[DiceFaceController] ColorType {faceColor}에 대한 Material이 없습니다!");
            }
        }
    }
    
    private void OnValidate()
    {
        // 배열 크기 자동 조정
        if (faceRenderers == null || faceRenderers.Length != 6)
        {
            faceRenderers = new MeshRenderer[6];
        }
        
        if (initialFaces == null || initialFaces.Length != 6)
        {
            initialFaces = new ColorType[6];
        }
    }
}
