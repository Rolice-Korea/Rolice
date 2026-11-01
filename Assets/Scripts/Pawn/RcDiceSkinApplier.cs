using UnityEngine;
using Rolice;
using Rolice.System;

/// <summary>
/// 주사위 면 스킨 적용 공용 로직. 인게임(RcDiceFaceController)과 로비(RcLobbyCube)가 공유한다.
/// faces / faceToSlot 은 모두 RcDiceDirectionType 순서의 6칸 배열.
/// 머티리얼 배열 획득/반영(materials vs sharedMaterials)은 호출부 책임.
/// </summary>
public static class RcDiceSkinApplier
{
    /// <summary>
    /// mats 배열에 스킨 머티리얼을 채운다. 스킨에 해당 색이 없으면 Default 스킨으로 폴백, 그것도 없으면 기존 유지.
    /// </summary>
    public static void Apply(Material[] mats, int[] faceToSlot, RcColorType[] faces, RcFaceSkinDataTable skin)
    {
        if (mats == null || faceToSlot == null || faces == null) return;

        var registry = RcDataTableManager.FaceSkinRegistry;
        var fallback = registry != null ? registry.GetFaceData(RcFaceSkinType.Default) : null;

        int count = Mathf.Min(RcDiceDirectionTypeExt.Count, Mathf.Min(faceToSlot.Length, faces.Length));
        for (int i = 0; i < count; i++)
        {
            int slot = faceToSlot[i];
            if (slot < 0 || slot >= mats.Length) continue;

            var color = faces[i];
            if (color == RcColorType.None) continue;

            if (TryGetFaceMaterial(skin, color, out var mat) ||
                (fallback != skin && TryGetFaceMaterial(fallback, color, out mat)))
                mats[slot] = mat;
        }
    }

    private static bool TryGetFaceMaterial(RcFaceSkinDataTable skin, RcColorType color, out Material mat)
    {
        mat = null;
        return skin != null && skin.TryGetFaceMaterial(color, out mat);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 메시 서브메시 평균 노멀로 방향 → 슬롯 매핑을 계산한다.
    /// 노멀은 root 로컬 공간 기준이라 root 의 현재 회전과 무관하다.
    /// </summary>
    public static int[] BakeFaceToSlot(Transform root, MeshFilter mf)
    {
        var result = new int[RcDiceDirectionTypeExt.Count];
        for (int i = 0; i < result.Length; i++) result[i] = -1;

        Mesh mesh = mf.sharedMesh;
        Vector3[] normals = mesh.normals;

        for (int sub = 0; sub < mesh.subMeshCount; sub++)
        {
            int[] tris = mesh.GetTriangles(sub);
            Vector3 avg = Vector3.zero;
            foreach (int t in tris)
                avg += normals[t];

            Vector3 local = root.InverseTransformDirection(mf.transform.TransformDirection(avg.normalized));
            var dir = RcDiceDirectionTypeExt.FromNormal(local);
            if (result[(int)dir] >= 0)
                Debug.LogWarning($"[RcDiceSkinApplier] {dir} 방향에 서브메시가 중복 매핑됨 ({result[(int)dir]} → {sub})");
            result[(int)dir] = sub;
        }

        for (int i = 0; i < result.Length; i++)
            if (result[i] < 0) Debug.LogWarning($"[RcDiceSkinApplier] {(RcDiceDirectionType)i} 방향에 매핑된 서브메시 없음");

        return result;
    }

    public static MeshFilter FindFaceMeshFilter(Transform root, int minSubMeshCount)
    {
        foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
            if (mf.sharedMesh != null && mf.sharedMesh.subMeshCount >= minSubMeshCount)
                return mf;
        return null;
    }
#endif
}
