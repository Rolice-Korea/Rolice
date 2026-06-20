using UnityEngine;

namespace Rolice.Home
{
    /// <summary>
    /// 배치 프리뷰 고스트. 타겟 셀에 반투명 블록을 표시하고 유효성에 따라 색을 바꾼다.
    /// 색은 MaterialPropertyBlock으로 적용해 머티리얼 인스턴스 증식(GC)을 피한다.
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
            transform.position = RcHomeGrid.GridToWorld(cell);

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
