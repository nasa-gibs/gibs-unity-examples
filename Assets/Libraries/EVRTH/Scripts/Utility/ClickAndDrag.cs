using UnityEngine;
using UnityEngine.EventSystems;

namespace EVRTH.Scripts.Utility
{
    /// <summary>
    /// Lets you grab and move the ui window to interact with it in VR
    /// </summary>
    public class ClickAndDrag : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler,IPointerHandlerVr
    {
        public Transform followTarget;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (followTarget)
            {
                transform.parent  = followTarget;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.parent = null;
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }

        public void OnTriggerEnter(Collider otherCollider)
        {
            followTarget = otherCollider.transform;
        }

        public void OnTriggerExit(Collider otherCollider)
        {
            followTarget = null;
        }

        public void PointerDown(Vector3 pointerPosition)
        {
            OnPointerDown(new PointerEventData(EventSystem.current) { position = Camera.main.WorldToScreenPoint(pointerPosition) });
        }

        public void PointerUp(Vector3 pointerPosition)
        {
            OnPointerUp(new PointerEventData(EventSystem.current) { position = Camera.main.WorldToScreenPoint(pointerPosition) });
        }

        public void PointerEnter(Vector3 pointerPosition)
        {
            OnPointerEnter(new PointerEventData(EventSystem.current) { position = Camera.main.WorldToScreenPoint(pointerPosition) });
        }

        public void PointerExit(Vector3 pointerPosition)
        {
            OnPointerExit(new PointerEventData(EventSystem.current) { position = Camera.main.WorldToScreenPoint(pointerPosition) });
        }
    }
}
