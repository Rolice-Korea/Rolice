using UnityEngine;
using UnityEngine.EventSystems;

namespace Engine.UI
{
    public class RcSwipeArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private float minSwipeDistance = 50f;
        private Vector2 startPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // IDragHandler is required for IBeginDragHandler/IEndDragHandler to work on UI elements
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - startPos;

            if (delta.magnitude < minSwipeDistance)
                return;

            // 카메라 yaw에 맞춰 스와이프 벡터를 역회전하여 방향 판정 영역을 보정
            if (RcDiceCamera.Instance != null)
            {
                delta = RotateVector2(delta, -RcDiceCamera.Instance.CurrentYaw);
            }

            Vector2Int dir;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                dir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                dir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            RcInputController.Instance?.TriggerMove(dir);
        }

        /// <summary>
        /// 2D 벡터를 주어진 각도(도)만큼 회전
        /// </summary>
        private static Vector2 RotateVector2(Vector2 v, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
