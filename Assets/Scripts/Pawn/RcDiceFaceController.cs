using UnityEngine;

public class RcDiceFaceController : MonoBehaviour
{
    [Header("Dice Renderer")]
    [SerializeField] private MeshRenderer diceRenderer;

    [Header("Material Slot Mapping")]
    [Tooltip("각 면(TOP, BOTTOM, FRONT, BACK, LEFT, RIGHT)에 대응하는 Material 슬롯 인덱스")]
    [SerializeField] private int[] faceToSlot = { 0, 1, 2, 3, 4, 5 };

    [Header("Initial Face Colors")]
    [SerializeField] private RcColorSO[] initialFaces = new RcColorSO[6];

    private RcDiceFaceData faceData;

    public void Initialize()
    {
        faceData = new RcDiceFaceData(initialFaces);
        UpdateVisuals();
    }

    public void RotateFaces(Vector2Int direction)
    {
        faceData = faceData.Rotate(direction);
    }

    public RcColorSO GetBottomColor()
    {
        return faceData.GetBottomColor();
    }

    public RcColorSO GetFaceColor(int faceIndex)
    {
        return faceData.GetFaceColor(faceIndex);
    }

    public void UpdateVisuals()
    {
        if (diceRenderer == null) return;

        var mats = diceRenderer.materials;

        for (int i = 0; i < 6; i++)
        {
            int slot = faceToSlot[i];
            if (slot < 0 || slot >= mats.Length) continue;

            RcColorSO faceColor = faceData.GetFaceColor(i);

            if (faceColor != null && faceColor.DiceMaterial != null)
            {
                mats[slot] = faceColor.DiceMaterial;
            }
        }

        diceRenderer.materials = mats;
    }

    private void OnValidate()
    {
        if (initialFaces == null || initialFaces.Length != 6)
        {
            initialFaces = new RcColorSO[6];
        }

        if (faceToSlot == null || faceToSlot.Length != 6)
        {
            faceToSlot = new int[] { 0, 1, 2, 3, 4, 5 };
        }
    }
    
#if UNITY_EDITOR
    private int[] BuildFaceToSlotByNormal(MeshFilter mf)
    {
        Mesh mesh = mf.sharedMesh;
        Vector3[] normals = mesh.normals;

        int[] result = new int[6];

        for (int sub = 0; sub < mesh.subMeshCount; sub++)
        {
            int[] tris = mesh.GetTriangles(sub);
            Vector3 avg = Vector3.zero;

            foreach (int t in tris)
                avg += normals[t];

            avg.Normalize();
            Vector3 world = mf.transform.TransformDirection(avg);

            int face = GetFaceIndex(world);
            result[face] = sub;
        }

        return result;
    }

    private int GetFaceIndex(Vector3 n)
    {
        n.Normalize();

        if (Vector3.Dot(n, Vector3.up) > 0.9f) return RcDiceFaceData.TOP;
        if (Vector3.Dot(n, Vector3.down) > 0.9f) return RcDiceFaceData.BOTTOM;
        if (Vector3.Dot(n, Vector3.forward) > 0.9f) return RcDiceFaceData.FRONT;
        if (Vector3.Dot(n, Vector3.back) > 0.9f) return RcDiceFaceData.BACK;
        if (Vector3.Dot(n, Vector3.left) > 0.9f) return RcDiceFaceData.LEFT;
        if (Vector3.Dot(n, Vector3.right) > 0.9f) return RcDiceFaceData.RIGHT;
        
        return RcDiceFaceData.TOP;
    }
#endif
    
#if UNITY_EDITOR
    [ContextMenu("Bake FaceToSlot From Mesh")]
    private void BakeFaceToSlot()
    {
        var mf = GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("MeshFilter 없음");
            return;
        }

        faceToSlot = BuildFaceToSlotByNormal(mf);
        UnityEditor.EditorUtility.SetDirty(this);

        Debug.Log("Dice faceToSlot baked 완료");
    }
#endif
}
