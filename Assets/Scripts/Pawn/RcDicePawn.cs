using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ScriptableObject 기반 색상 관리를 사용하는 주사위 Pawn
/// 색상 추가/변경 시 RcColorSettings만 수정하면 됨
/// </summary>
public class RcDicePawn : MonoBehaviour
{
    // === 면 인덱스 상수 ===
    private const int TOP = 0, BOTTOM = 1, FRONT = 2, BACK = 3, LEFT = 4, RIGHT = 5;

    // === 회전 맵 (방향별 면 재배치) ===
    private static readonly int[][] RollMaps = {
        new[] { BACK, FRONT, TOP, BOTTOM, LEFT, RIGHT },    // UP
        new[] { FRONT, BACK, BOTTOM, TOP, LEFT, RIGHT },    // DOWN
        new[] { LEFT, RIGHT, FRONT, BACK, BOTTOM, TOP },    // RIGHT
        new[] { RIGHT, LEFT, FRONT, BACK, TOP, BOTTOM }     // LEFT
    };

    // === 데이터 ===
    [Header("Face Colors")]
    [SerializeField] private ColorType[] faces = new ColorType[6];
    
    [Header("References")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private RcInputController inputController;
    [SerializeField] private MeshRenderer[] faceRenderers = new MeshRenderer[6];

    [Header("Settings")]
    [SerializeField] private float rollDuration = 0.3f;

    private Vector2Int gridPos;
    private bool isRolling;

    // 매니저 참조
    private RcLevelManager LevelManager => RcLevelManager.Instance;
    
    private void Awake()
    {
        gridPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        
        if (modelTransform == null)
            modelTransform = transform.GetChild(0);
            
        UpdateVisuals();
    }

    private void Start()
    {
        // 초기 위치의 타일 진입 처리
        OnEnterTile(gridPos);
    }

    // ========================================
    // 외부 인터페이스
    // ========================================
    
    public ColorType GetBottomColor() => faces[BOTTOM];
    public Vector2Int GetGridPos() => gridPos;
    
    public void Move(Vector2Int dir)
    {
        if (isRolling) return;

        Vector2Int targetPos = gridPos + dir;
        
        if (!IsValidMove(targetPos))
        {
            Debug.Log($"[DicePawn] 이동 불가: {targetPos}");
            return;
        }

        StartCoroutine(RollCo(dir, targetPos));
    }

    // ========================================
    // 이동 검증
    // ========================================
    
    private bool IsValidMove(Vector2Int targetPos)
    {
        RcTileData targetTile = LevelManager.GetRuntimeTile(targetPos);
        
        // 타일이 없으면 불가
        if (targetTile == null || string.IsNullOrEmpty(targetTile.TileID))
            return false;
        
        // 진입 불가 타일
        if (!targetTile.bCanEnter)
            return false;
        
        // 타일 행동의 CanEnter 체크
        ITileBehavior behavior = targetTile.GetBehavior(targetTile.TileObject);
        if (behavior != null && !behavior.CanEnter(this))
            return false;
        
        return true;
    }

    // ========================================
    // 애니메이션
    // ========================================
    
    private IEnumerator RollCo(Vector2Int dir, Vector2Int targetPos)
    {
        isRolling = true;
        inputController?.SetProcessing(true);

        // 현재 타일에서 나가기
        OnExitTile(gridPos);

        // 데이터 업데이트
        Vector2Int previousPos = gridPos;
        gridPos = targetPos;
        RotateFaces(dir);

        // 시작/끝 상태
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPos.x, 1f, targetPos.y);
        Quaternion startRot = modelTransform.localRotation;

        // 방향별 회전축/이동벡터 (월드 기준)
        Vector3 axis, moveDir;
        if (dir.x != 0) // 좌우
        {
            axis = dir.x > 0 ? Vector3.back : Vector3.forward;
            moveDir = Vector3.right * dir.x;
        }
        else // 앞뒤
        {
            axis = dir.y < 0 ? Vector3.left : Vector3.right;
            moveDir = Vector3.forward * dir.y;
        }

        Quaternion endRot = Quaternion.AngleAxis(90f, axis) * startRot;
        Vector3 pivot = startPos + moveDir * 0.5f + Vector3.down * 0.5f;

        // 애니메이션 루프
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / rollDuration, 1f);
            float ease = 1f - Mathf.Pow(1f - t, 3); // Ease out cubic

            float angle = ease * 90f;
            Quaternion rot = Quaternion.AngleAxis(angle, axis);
            
            transform.position = pivot + rot * (startPos - pivot);
            modelTransform.localRotation = Quaternion.Slerp(startRot, endRot, ease);

            yield return null;
        }

        // 최종 위치
        transform.position = endPos;
        modelTransform.localRotation = endRot;

        // 새 타일 진입
        OnEnterTile(targetPos);

        isRolling = false;
        inputController?.SetProcessing(false);
    }

    // ========================================
    // 면 데이터 회전
    // ========================================
    
    private void RotateFaces(Vector2Int dir)
    {
        int mapIndex = dir == Vector2Int.up ? 0 :
                       dir == Vector2Int.down ? 1 :
                       dir == Vector2Int.right ? 2 : 3;

        int[] map = RollMaps[mapIndex];
        ColorType[] old = (ColorType[])faces.Clone();

        for (int i = 0; i < 6; i++)
            faces[i] = old[map[i]];
    }

    // ========================================
    // 타일 상호작용
    // ========================================
    
    /// <summary>
    /// 타일에 진입할 때 호출됩니다
    /// </summary>
    private void OnEnterTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        
        if (tile == null) return;
        
        // 타일 행동 실행
        ITileBehavior behavior = tile.GetBehavior(tile.TileObject);
        behavior?.OnEnter(this);
    }
    
    /// <summary>
    /// 타일에서 나갈 때 호출됩니다
    /// </summary>
    private void OnExitTile(Vector2Int pos)
    {
        RcTileData tile = LevelManager.GetRuntimeTile(pos);
        
        if (tile == null) return;
        
        // 타일 행동 실행
        ITileBehavior behavior = tile.GetBehavior(tile.TileObject);
        behavior?.OnExit(this);
    }

    // ========================================
    // 비주얼 (ScriptableObject 사용)
    // ========================================
    
    private void UpdateVisuals()
    {
        for (int i = 0; i < 6; i++)
        {
            if (faceRenderers[i] == null) continue;
            
            ColorType faceColor = faces[i];
            Material material = RcDataManager.Instance.MaterialDatabase.GetMaterial(faceColor, MaterialUsageType.Dice);
            
            if (material != null)
            {
                faceRenderers[i].material = material;
            }
            else
            {
                Debug.LogWarning($"[DicePawn] ColorType {faceColor}에 대한 Material이 없습니다!");
            }
        }
    }
}