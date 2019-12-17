using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace EVRTH.Scripts.Utility
{
    /// <summary>
    /// Lets you interact with uGUI systems without using the mouse by passing events to the underlying uGUI interfaces
    /// Can also have its own OnClick events, letting a vr user have additional events trigger when using the UI element vs a non-vr user
    /// </summary>
    public class VRButton : MonoBehaviour,IPointerHandlerVr
    {
        /// <summary>
        /// Event that will only be triggered by this script and not normal ui interaction
        /// </summary>
        public UnityEvent onClick;

        #region uGUI event passthrough

        private IPointerEnterHandler pointerEnter;
        private IPointerExitHandler pointerExit;
        private IPointerClickHandler pointerClick;
        #endregion

        private void Awake()
        {
            pointerEnter = GetComponent<IPointerEnterHandler>();
            pointerExit = GetComponent<IPointerExitHandler>();
            pointerClick = GetComponent<IPointerClickHandler>();

        }

        public void Click(Vector3 pointerPosition)
        {
            if (onClick != null)
            {
                onClick.Invoke();
            }

            if (pointerClick != null)
            {
                pointerClick.OnPointerClick(new PointerEventData(EventSystem.current){position = Camera.main.WorldToScreenPoint(pointerPosition)});
            }

        }

        public void PointerDown(Vector3 pointerPosition)
        {
            Click(pointerPosition);
        }

        public void PointerUp(Vector3 pointerPosition)
        {
        }

        public void PointerEnter(Vector3 pointerPosition)
        {
            if (pointerEnter != null)
            {
                pointerEnter.OnPointerEnter(new PointerEventData(EventSystem.current){position = Camera.main.WorldToScreenPoint(pointerPosition) });
            }
        }

        public void PointerExit(Vector3 pointerPosition)
        {
            if (pointerExit != null)
            {
                pointerExit.OnPointerExit(new PointerEventData(EventSystem.current) { position = Camera.main.WorldToScreenPoint(pointerPosition) });
            }
        }
    }
}
