using System.Reflection;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameParameters), true)]
public class GameParametersEditor : Editor
{
    public override void OnInspectorGUI()
    {
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
            EditorGUILayout.BeginVertical(GUILayout.Width(20f));
             //check for header attribute
            bool hasHeader = System.Attribute.IsDefined(field, typeof(HeaderAttribute), false);
            if (hasHeader)
                GUILayout.FlexibleSpace();
            if (GUILayout.Button(parameters.ShowsField(field.Name) ? "-" : "+", GUILayout.Width(20f)))
            {
                parameters.ToggleShowField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(16);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name), true);
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
