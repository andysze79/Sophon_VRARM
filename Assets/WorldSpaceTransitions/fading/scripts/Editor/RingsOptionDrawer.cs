using UnityEngine;
using UnityEditor;
using System.Collections;

namespace WorldSpaceTransitions
{
    [CustomPropertyDrawer(typeof(FadingTransition.RingsOption))]
    class RingsOptionDrawer : PropertyDrawer
    {
        float height = 20;

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property. 
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            Rect boolRect = new Rect(position.x, position.y, position.width, 20);
            Rect countRect = new Rect(position.x, position.y + 20, position.width, 18);
            Rect animRect = new Rect(position.x, position.y + 40, position.width, 20);
            Rect timeRect = new Rect(position.x, position.y + 60, position.width, 18);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            SerializedProperty m_useRings = property.FindPropertyRelative("useRings");
            SerializedProperty m_count = property.FindPropertyRelative("ringsCount");
            SerializedProperty m_animate = property.FindPropertyRelative("animateRings");
            SerializedProperty m_scale = property.FindPropertyRelative("timeScale");

            EditorGUI.PropertyField(boolRect, m_useRings, new GUIContent("useRings"));//
            //EditorGUILayout.PropertyField(m_useRings, new GUIContent("useRings"));
            height = 20;
            if (m_useRings.boolValue)
            {
                height += 40;
                EditorGUI.PropertyField(countRect, m_count, new GUIContent("ringsCount"));
                EditorGUI.PropertyField(animRect, m_animate, new GUIContent("animateRings"));
                if (m_animate.boolValue)
                {
                    EditorGUI.PropertyField(timeRect, m_scale, new GUIContent("timeScale"));
                    height += 20;
                }
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return height;
            // base.GetPropertyHeight(property, label) * rows;  // assuming original is one row
        }

    }
}