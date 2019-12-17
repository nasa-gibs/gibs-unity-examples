using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EVRTH.Scripts.Utility
{
    public class ProxyButton : MonoBehaviour, IPointerHandlerVr
    {
        public bool isInteractable;
        public Image targetImage;
        public Color defaultColor;
        public Color highlightedColor;
        public Color pressedColor;
        public Color disabledColor;
        public UnityEvent onClick;
        private bool isPressed;
        private Color currColor;

        private void Awake()
        {
            currColor = isInteractable ? defaultColor : disabledColor;
            targetImage.color = currColor;
        }

        public void PointerDown(Vector3 pointerPosition)
        {
            if (isInteractable)
            {
                StopAllCoroutines();
                StartCoroutine(TransitionColor(pressedColor));
                isPressed = true;
            }
        }

        public void PointerUp(Vector3 pointerPosition)
        {
            if (isInteractable && isPressed)
            {
                StopAllCoroutines();
                StartCoroutine(TransitionColor(highlightedColor));
                isPressed = false;
                onClick.Invoke();
            }
        }

        public void PointerEnter(Vector3 pointerPosition)
        {
            if (isInteractable)
            {
                StopAllCoroutines();
                StartCoroutine(TransitionColor(highlightedColor));
            }
        }

        public void PointerExit(Vector3 pointerPosition)
        {
            if (isInteractable)
            {
                StopAllCoroutines();
                StartCoroutine(TransitionColor(defaultColor));
                isPressed = false;
            }
        }

        private IEnumerator TransitionColor(Color newColor)
        {
            WaitForEndOfFrame w = new WaitForEndOfFrame();
            while (currColor != newColor)
            {
                currColor = Color.Lerp(currColor, newColor, 10 * Time.deltaTime);
                targetImage.color = currColor;
                yield return w;
            }
        }
    }
}
