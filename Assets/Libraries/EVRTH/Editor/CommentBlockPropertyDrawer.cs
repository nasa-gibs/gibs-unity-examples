using EVRTH.Scripts.DemoHelpers;
using UnityEditor;
using UnityEngine;

namespace EVRTH.Editor
{
    [CustomPropertyDrawer(typeof(InspectorCommentBlock))]
    public class CommentBlockPropertyDrawer : PropertyDrawer
    {
        private const float height = 150;
        private float indent = 30;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorProperty(position, label, property))
            {
                EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),
                    new GUIContent(label.text));
                SerializedProperty valProperty = property.FindPropertyRelative("comment");
                float labelHeight = base.GetPropertyHeight(property, label);
                Rect region = new Rect(position.x + indent,position.y + labelHeight,position.width - indent - 10, position.height - indent - 10);
                EditorGUI.BeginChangeCheck();
                string newValue= EditorGUI.TextArea(region, valProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    valProperty.stringValue = newValue;
                }
            }
        }
    }
}
