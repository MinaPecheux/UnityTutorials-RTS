using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameParameters), true)]
public class GameParametersEditor : Editor
{
    private GUIStyle _buttonsStyle;

    public override void OnInspectorGUI()
    {
        if (_buttonsStyle == null)
        {
            _buttonsStyle = new GUIStyle(GUI.skin.button);
            _buttonsStyle.padding.right = 0;
            _buttonsStyle.padding.left = 0;
        }

        serializedObject.Update();

        GameParameters parameters = (GameParameters)target;

        EditorGUILayout.LabelField($"Name: {parameters.GetParametersName()}", EditorStyles.boldLabel);

        System.Type ParametersType = parameters.GetType();
        FieldInfo[] fields = ParametersType.GetFields();
        foreach (FieldInfo field in fields)
        {
            //check for "hide in inspector" attribute:
            // if there is one, cancel the display for this field
            if (System.Attribute.IsDefined(field, typeof(HideInInspector), false))
                continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(40f));
             //check for header attribute
            bool hasHeader = System.Attribute.IsDefined(field, typeof(HeaderAttribute), false);
            if (hasHeader)
                GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(
                parameters.ShowsField(field.Name)
                    ? EditorGUIUtility.IconContent("animationvisibilitytoggleon@2x")
                    : EditorGUIUtility.IconContent("d_animationvisibilitytoggleon@2x"),
                _buttonsStyle,
                GUILayout.Width(20f),
                GUILayout.Height(20f)
            ))
            {
                parameters.ToggleShowField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button(
                parameters.SerializesField(field.Name)
                    ? EditorGUIUtility.IconContent("SaveAs@2x")
                    : EditorGUIUtility.IconContent("d_SaveAs@2x"),
                _buttonsStyle,
                GUILayout.Width(20f),
                GUILayout.Height(20f)
            ))
            {
                parameters.ToggleSerializeField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(16);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name), true);
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
