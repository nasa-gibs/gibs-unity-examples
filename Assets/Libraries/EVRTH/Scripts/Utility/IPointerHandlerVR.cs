using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    internal interface IPointerHandlerVr
    {
        void PointerDown(Vector3 pointerPosition);
        void PointerUp(Vector3 pointerPosition);
        void PointerEnter(Vector3 pointerPosition);
        void PointerExit(Vector3 pointerPosition);
    }
}
