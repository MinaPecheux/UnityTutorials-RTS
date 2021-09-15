/* Adapted from:
 * https://learn.unity.com/tutorial/property-drawers-and-custom-inspectors */
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InputBinding))]
public class InputBindingDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        float rowHeight = EditorGUIUtility.singleLineHeight;
        Rect displayNameRect = new Rect(position.x, position.y, position.width, rowHeight);
        Rect keyRect = new Rect(position.x, position.y + rowHeight, 50, rowHeight);
        Rect inputEventRect = new Rect(position.x + 55, position.y + rowHeight, position.width - 55, rowHeight);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(displayNameRect, property.FindPropertyRelative("displayName"), GUIContent.none);
        EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("key"), GUIContent.none);
        EditorGUI.PropertyField(inputEventRect, property.FindPropertyRelative("inputEvent"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2f;
    }
}
