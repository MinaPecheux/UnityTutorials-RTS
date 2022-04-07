using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TechnologyNodeData))]
public class TechnologyTreeNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TechnologyNodeData tree = (TechnologyNodeData)target;
        if (GUILayout.Button("Open Editor"))
        {
            if (tree.code == TechnologyNodeData.ROOT_NODE_CODE)
                TechnologyTreeWindow.ShowTreeWindow(target);
            else
            {
                TechnologyNodeData root = TechnologyNodeData.GetRootNodeInDirectory(
                    _GetSelectedPathOrFallback());
                if (root != null)
                    TechnologyTreeWindow.ShowTreeWindow(root);
                else
                    EditorGUILayout.HelpBox(
                        $"Select the root node ({TechnologyNodeData.ROOT_NODE_CODE}) to edit the tree",
                        MessageType.Info);
            }
        }
    }

    [MenuItem("Window/Custom/Create Root Node")]
    static void CreateRootNode()
    {
        string path = _GetSelectedPathOrFallback();
        string code = TechnologyNodeData.ROOT_NODE_CODE;
        string name = code.Substring(0, 1).ToUpper() + code.Substring(1);
        TechnologyNodeData node =
            ScriptableObject.CreateInstance<TechnologyNodeData>();
        node.Initialize(code, name);

        AssetDatabase.CreateAsset(
            node,
            System.IO.Path.Combine(path, name) + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Window/Custom/Create Root Node", true)]
    static bool ValidateCreateRootNode()
    {
        TechnologyNodeData root = TechnologyNodeData.GetRootNodeInDirectory(
            _GetSelectedPathOrFallback());
        return root == null;
    }

    private static string _GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                path = System.IO.Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

}
