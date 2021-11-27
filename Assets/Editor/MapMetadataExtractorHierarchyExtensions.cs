using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class MapMetadataExtractorHierarchyExtensions
{
    private static GUIStyle _btnStyle;

    static MapMetadataExtractorHierarchyExtensions()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        if (_btnStyle == null)
        {
            _btnStyle = new GUIStyle(GUI.skin.button);
            _btnStyle.fontSize = 10;
        }

        Object obj = EditorUtility.InstanceIDToObject(instanceID);

        // if obj is null: item is a Scene in the hierarchy
        if (obj == null)
        {
            Scene s = _GetSceneFromInstanceID(instanceID);
            float width = 80f;
            Rect btnPosition = new Rect(
                selectionRect.x + selectionRect.width - width - 4,
                selectionRect.y + 1,
                width,
                selectionRect.height - 2);
            if (GUI.Button(btnPosition, "Extract", _btnStyle))
                MapMetadataExtractor.Extract(s);
        }
    }

    private static Scene _GetSceneFromInstanceID(int id)
    {
        System.Type type = typeof(UnityEditor.SceneManagement.EditorSceneManager);
        MethodInfo mi = type.GetMethod(
            "GetSceneByHandle",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
        object classInstance = System.Activator.CreateInstance(type, null);
        return (Scene)mi.Invoke(classInstance, new object[] { id });
    }

}
