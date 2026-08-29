using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 배치 프리뷰 고스트. 타겟 셀에 반투명 블록을 표시하고 유효성에 따라 색을 바꾼다.
    /// 색은 MaterialPropertyBlock으로 적용해 머티리얼 인스턴스 증식(GC)을 피한다.
    ///
    /// 배치: <b>섬 루트 하위에 두어야 한다</b>. 셀 좌표가 섬 로컬 기준이라 localPosition으로 놓으며,
    /// 그래야 로비에서 섬이 떠다니고 회전해도 고스트가 함께 따라간다(스포너와 동일 규약).
    /// </summary>
    public class RcHomeBlockCursor : MonoBehaviour
    {
        [SerializeField] private GameObject ghost;          // 반투명 프리뷰 메시 루트
        [SerializeField] private Renderer   ghostRenderer;  // 색을 바꿀 렌더러
        [SerializeField] private string     colorProperty = "_BaseColor"; // URP Lit 기준
        [SerializeField] private Color      validColor   = new(0.3f, 1f, 0.4f, 0.4f);
        [SerializeField] private Color      invalidColor = new(1f, 0.3f, 0.3f, 0.4f);

        private MaterialPropertyBlock mpb;
        private int                   colorId;

        private void Awake()
        {
            mpb     = new MaterialPropertyBlock();
            colorId = Shader.PropertyToID(colorProperty);
            Hide();
        }

        public void Show(Vector3Int cell, bool valid)
        {
            if (ghost == null) return;

            if (!ghost.activeSelf) ghost.SetActive(true);
            transform.localPosition = RcHomeGrid.GridToWorld(cell);
            transform.localRotation = Quaternion.identity;

            if (ghostRenderer != null)
            {
                ghostRenderer.GetPropertyBlock(mpb);
                mpb.SetColor(colorId, valid ? validColor : invalidColor);
                ghostRenderer.SetPropertyBlock(mpb);
            }
        }

        public void Hide()
        {
            if (ghost != null && ghost.activeSelf)
                ghost.SetActive(false);
        }
    }
}
