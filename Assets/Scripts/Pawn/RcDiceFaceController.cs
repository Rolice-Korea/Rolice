using DG.Tweening;
using UnityEngine;
using Rolice;
using Rolice.System;

public class RcDiceFaceController : MonoBehaviour
{
    [Header("Dice Renderer")]
    [SerializeField] private MeshRenderer diceRenderer;

    [Header("Material Slot Mapping")]
    [Tooltip("각 면(TOP, BOTTOM, FRONT, BACK, LEFT, RIGHT)에 대응하는 Material 슬롯 인덱스")]
    [SerializeField] private int[] faceToSlot = { 0, 1, 2, 3, 4, 5 };

    [Header("Initial Face Colors")]
    [SerializeField] private RcColorType[] initialFaces = new RcColorType[6];

    [Header("Defeat Effect")]
    [SerializeField] private float grayFadeDuration = 0.6f;

    [Header("Victory Effect")]
    [SerializeField] private float flashIntensity = 3f;
    [SerializeField] private float flashDuration = 0.5f;

    private RcDiceFaceData faceData;
    private Tween grayTween;
    private Tween flashTween;

    public void Initialize()
    {
        faceData = new RcDiceFaceData(initialFaces);
        // faces가 주입되기 전 상태이므로 UpdateVisuals 호출하지 않음
        // 실제 비주얼은 Bootstrap이 InitializeFaces()를 호출할 때 갱신됨
    }

    public void Initialize(RcColorType[] faces)
    {
        faceData = new RcDiceFaceData(faces);
        UpdateVisuals();
    }

    public void RotateFaces(Vector2Int direction)
    {
        faceData = faceData.Rotate(direction);
    }

    public void ChangeFaceColor(RcColorType targetColorType, RcColorType newColorType)
    {
        faceData = faceData.ChangeColor(targetColorType, newColorType);
        UpdateVisuals();
    }

    public RcColorType GetBottomColor()
    {
        return faceData.GetBottomColor();
    }

    public RcColorType GetFaceColor(int faceIndex)
    {
        return faceData.GetFaceColor(faceIndex);
    }

    public void UpdateVisuals()
    {
        if (diceRenderer == null) return;

        var mats = diceRenderer.materials;
        var registry = RcDataTableManager.FaceSkinRegistry;
        if (registry == null) return;

        var skinData = registry.GetFaceData(RcSkinSystem.ActiveFaceSkinType);

        for (int i = 0; i < 6; i++)
        {
            int slot = faceToSlot[i];
            if (slot < 0 || slot >= mats.Length) continue;

            // 현재 주사위 면의 색상 정보를 가져옴 (this.faceData 사용)
            RcColorType faceColor = faceData.GetFaceColor(i);
            if (faceColor == RcColorType.None) continue;

            Material skinMat = skinData?.GetFaceMaterial(faceColor);
            if (skinMat != null)
            {
                mats[slot] = skinMat;
            }
        }

        diceRenderer.materials = mats;
    }

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int GlowColorID = Shader.PropertyToID("_GlowColor");

    public void FadeToGray()
    {
        if (diceRenderer == null) return;

        var mats = diceRenderer.materials;
        var originalBaseColors = new Color[mats.Length];
        var originalEmissionColors = new Color[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            originalBaseColors[i] = mats[i].GetColor(BaseColorID);
            originalEmissionColors[i] = mats[i].GetColor(GlowColorID);
        }

        Color grayTarget = new Color(0.25f, 0.25f, 0.25f, 1f);

        float progress = 0f;
        grayTween = DOTween.To(() => progress, x =>
        {
            progress = x;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetColor(GlowColorID,
                    Color.Lerp(originalEmissionColors[i], Color.black, progress));
                mats[i].SetColor(BaseColorID,
                    Color.Lerp(originalBaseColors[i], grayTarget, progress));
            }
        }, 1f, grayFadeDuration).SetEase(Ease.OutQuad);
    }

    public void FlashEmission(System.Action onComplete = null)
    {
        if (diceRenderer == null)
        {
            onComplete?.Invoke();
            return;
        }

        var mats = diceRenderer.materials;
        var originalEmissionColors = new Color[mats.Length];

        for (int i = 0; i < mats.Length; i++)
            originalEmissionColors[i] = mats[i].GetColor(GlowColorID);

        float progress = 0f;
        flashTween = DOTween.To(() => progress, x =>
        {
            progress = x;
            float curve = 1f - Mathf.Abs(2f * progress - 1f);
            for (int i = 0; i < mats.Length; i++)
            {
                Color boosted = originalEmissionColors[i] * (1f + flashIntensity * curve);
                mats[i].SetColor(GlowColorID, boosted);
            }
        }, 1f, flashDuration)
        .SetEase(Ease.Linear)
        .OnComplete(() => onComplete?.Invoke());
    }

    private void OnDestroy()
    {
        grayTween?.Kill();
        flashTween?.Kill();
    }

    private void OnValidate()
    {
        if (initialFaces == null || initialFaces.Length != 6)
        {
            initialFaces = new RcColorType[6];
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
    }
#endif
}
