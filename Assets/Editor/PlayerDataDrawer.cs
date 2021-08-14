/* Adapted from:
 * https://learn.unity.com/tutorial/property-drawers-and-custom-inspectors */
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PlayerData))]
public class PlayerDataDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        label.text = label.text.Replace("Element", "Player");
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        float fullWidth = EditorGUIUtility.labelWidth;
        float nameWidth = fullWidth * 0.7f;
        float colorWidth = fullWidth * 0.3f;
        Rect nameRect = new Rect(position.x, position.y, nameWidth, position.height);
        Rect colorRect = new Rect(position.x + nameWidth + 5, position.y, colorWidth, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
        EditorGUI.PropertyField(colorRect, property.FindPropertyRelative("color"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
