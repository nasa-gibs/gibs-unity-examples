using UnityEngine;

namespace EVRTH.Scripts.Utility
{
    public static class TransformExtensions
    {
        // Sets the parent of a transform, propagating position, rotation, and scale from the parent
        // to the child.
        public static void SetParentClearRelativeTransform(this Transform transform, Transform parent)
        {
            transform.SetParentClearRelativeTransform(parent, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        public static void SetParentClearRelativeTransform(this Transform transform, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            transform.parent = parent;
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = localScale;
        }
    }
}