// Put in Editor folder

using UnityEditor;
using UnityEngine;

namespace EVRTH.Editor
{
    public class UIHelp : MonoBehaviour
    {
        [MenuItem("UITools/SetAnchorsToCorners #x")]
        [ExecuteInEditMode]
        static void SetAnchorsToCorners()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();

            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    RectTransform pt = Selection.activeTransform.parent as RectTransform;

                    if (pt == null) return;

                    Vector2 newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / pt.rect.width,
                                                        t.anchorMin.y + t.offsetMin.y / pt.rect.height);
                    Vector2 newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / pt.rect.width,
                                                        t.anchorMax.y + t.offsetMax.y / pt.rect.height);

                    t.anchorMin = newAnchorsMin;
                    t.anchorMax = newAnchorsMax;
                    t.offsetMin = t.offsetMax = new Vector2(0, 0);
                } 
            }
        }

        [MenuItem("UITools/NudgeRight #RIGHT")]
        [ExecuteInEditMode]
        static void NudgeRight()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();
            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    t.localPosition += Vector3.right;
                }
            }
        }
        [MenuItem("UITools/SmallNudgeRight #%RIGHT")]
        [ExecuteInEditMode]
        static void SmallNudgeRight()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();
            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    t.localPosition += Vector3.right * 0.1f;
                }
            }
        }

        [MenuItem("UITools/NudgeLeft #LEFT")]
        [ExecuteInEditMode]
        static void NudgeLeft()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();
            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    t.localPosition += Vector3.left;
                }
            }
        }
        [MenuItem("UITools/SmallNudgeLeft #%LEFT")]
        [ExecuteInEditMode]
        static void SmallNudgeLeft()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();
            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    t.localPosition += Vector3.left * 0.1f;
                }
            }
        }

        [MenuItem("UITools/NudgeUp #UP")]
        [ExecuteInEditMode]
        static void NudgeUp()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();
            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    t.localPosition += Vector3.up;
                }
            }
        }
        [MenuItem("UITools/SmallNudgeUp #%UP")]
        [ExecuteInEditMode]
        static void SmallNudgeUp()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();
            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    t.localPosition += Vector3.up * 0.1f;
                }
            }
        }

        [MenuItem("UITools/NudgeDown #DOWN")]
        [ExecuteInEditMode]
        static void NudgeDown()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();
            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    t.localPosition += Vector3.down;
                }
            }
        }
        [MenuItem("UITools/SmallNudgeDown #%DOWN")]
        [ExecuteInEditMode]
        static void SmallNudgeDown()
        {
            //var t = Selection.activeGameObject.GetComponent<RectTransform>();
            foreach (var item in Selection.gameObjects)
            {
                var t = item.GetComponent<RectTransform>();
                if (t != null)
                {
                    t.localPosition += Vector3.down * 0.1f;
                }
            }
        }
    }
}
