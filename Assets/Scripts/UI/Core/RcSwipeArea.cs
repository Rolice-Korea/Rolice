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
    }
}
