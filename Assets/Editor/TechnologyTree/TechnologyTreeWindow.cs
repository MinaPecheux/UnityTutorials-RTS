/* Adapted from:
   https://gram.gs/gramlog/creating-node-based-editor-unity/ */
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TechnologyTreeWindow : EditorWindow
{
    private static string _TREE_ASSET_NAME = "Technology Tree";

    private string _treeAssetsPath;
    private List<TechnologyTreeEditorNode> _nodes = new List<TechnologyTreeEditorNode>();
    private List<Connection> _connections = new List<Connection>();

    private int _newNodeLastIndex = 0;

    private GUIStyle _nodeStyle;
    private GUIStyle _nodeSelectedStyle;
    private GUIStyle _connectionStyle;
    private GUIStyle _sidePanelStyle;
    private GUIStyle _resizerStyle;
    private GUIStyle _assetNameStyle;
    private int _nodeWidth = 100;
    private int _nodeHeight = 50;

    private bool _isResizing;
    private Rect _resizer;
    private float _sidePanelWidthRatio = 0.3f;

    private bool _selectingNode;
    private TechnologyTreeEditorNode _selectedNode;
    private ConnectionPoint _selectedInPoint;
    private ConnectionPoint _selectedOutPoint;

    private Vector2 _drag;
    private Vector2 _offset;

    [MenuItem("Window/Custom/Technology Tree Editor")]
    public static void ShowTreeWindow()
    {
        Object[] selection = Selection.GetFiltered(typeof(TechnologyNodeData), SelectionMode.Assets);
        if (selection.Length > 0)
        {
            TechnologyNodeData t = selection[0] as TechnologyNodeData;
            if (t != null && t.code == TechnologyNodeData.ROOT_NODE_CODE)
            {
                TechnologyTreeWindow window = GetWindow<TechnologyTreeWindow>();
                window.titleContent = new GUIContent("Technology Tree Editor");
                window.CreateGraph(t);
            }
        }
    }

    public static void ShowTreeWindow(Object target)
    {
        TechnologyTreeWindow window = GetWindow<TechnologyTreeWindow>();
        window.titleContent = new GUIContent("Technology Tree Editor");
        window.CreateGraph(target as TechnologyNodeData);
    }

    public void CreateGraph(object objRoot)
    {
        _newNodeLastIndex = 0;
        TechnologyNodeData root = (TechnologyNodeData)objRoot;
        _treeAssetsPath = System.IO.Path.GetDirectoryName(
            AssetDatabase.GetAssetPath(root));

        float paddingX = (_sidePanelWidthRatio * position.width) + 50f, paddingY = 50f;
        float offsetX = 20f, offsetY = 20f;
        float nodeXSpacing = _nodeWidth + offsetX * 2;
        float nodeYSpacing = _nodeHeight + offsetY * 2;

        Dictionary<string, TechnologyTreeEditorNode> nodesMapping = new Dictionary<string, TechnologyTreeEditorNode>();
        if (_nodes == null) _nodes = new List<TechnologyTreeEditorNode>();
        else _nodes.Clear();
        if (_connections == null) _connections = new List<Connection>();
        else _connections.Clear();
        List<TechnologyNodeData> currentNodes = new List<TechnologyNodeData>() { root };
        int x = 0, y = 0;
        TechnologyTreeEditorNode editorNode;
        List<(TechnologyNodeData, TechnologyNodeData)> edges =
            new List<(TechnologyNodeData, TechnologyNodeData)>();
        while (currentNodes.Count > 0)
        {
            List<TechnologyNodeData> newNodes = new List<TechnologyNodeData>();
            foreach (TechnologyNodeData n in currentNodes)
            {
                if (n == null) continue;
                editorNode = new TechnologyTreeEditorNode(
                    n,
                    new Vector2(paddingX + x * nodeXSpacing, paddingY + y * nodeYSpacing),
                    _nodeWidth,
                    _nodeHeight,
                    _nodeStyle,
                    _nodeSelectedStyle,
                    _connectionStyle,
                    _OnClickInPoint,
                    _OnClickOutPoint,
                    _OnClickRemoveNode,
                    _OnClickSelectNode
                );
                nodesMapping[n.code] = editorNode;
                _nodes.Add(editorNode);

                List<TechnologyNodeData> children = new List<TechnologyNodeData>(n.children);
                foreach (TechnologyNodeData child in children)
                {
                    if (child == null)
                    {
                        n.children.Remove(child);
                        continue;
                    }
                    edges.Add((n, child));
                    newNodes.Add(child);
                }
                x++;
            }
            currentNodes = newNodes;
            y++;
            x = 0;
        }

        foreach ((TechnologyNodeData source, TechnologyNodeData target) in edges)
        {
            _connections.Add(new Connection(
                source,
                target,
                nodesMapping[target.code].InPoint,
                nodesMapping[source.code].OutPoint,
                _OnClickRemoveConnection
            ));
        }

        // re-add unlinked nodes
        string[] nodeAssetPaths = System.IO.Directory.GetFiles(_treeAssetsPath);
        if (nodeAssetPaths.Length > _nodes.Count * 2)
        {
            foreach (string p in nodeAssetPaths)
            {
                if (p.EndsWith(".asset"))
                {
                    TechnologyNodeData n = AssetDatabase.LoadAssetAtPath<TechnologyNodeData>(p);
                    if (!nodesMapping.ContainsKey(n.code))
                    {
                        editorNode = new TechnologyTreeEditorNode(
                            n,
                            new Vector2(paddingX + x * nodeXSpacing, paddingY + y * nodeYSpacing),
                            _nodeWidth,
                            _nodeHeight,
                            _nodeStyle,
                            _nodeSelectedStyle,
                            _connectionStyle,
                            _OnClickInPoint,
                            _OnClickOutPoint,
                            _OnClickRemoveNode,
                            _OnClickSelectNode
                        );
                        nodesMapping[n.code] = editorNode;
                        _nodes.Add(editorNode);
                        x++;
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        _nodeStyle = new GUIStyle();
        _nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        _nodeStyle.normal.textColor = Color.white;
        _nodeStyle.border = new RectOffset(12, 12, 12, 12);
        _nodeStyle.alignment = TextAnchor.MiddleCenter;

        _nodeSelectedStyle = new GUIStyle();
        _nodeSelectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        _nodeSelectedStyle.normal.textColor = Color.white;
        _nodeSelectedStyle.border = new RectOffset(12, 12, 12, 12);
        _nodeSelectedStyle.alignment = TextAnchor.MiddleCenter;

        _connectionStyle = new GUIStyle();
        _connectionStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn.png") as Texture2D;
        _connectionStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn on.png") as Texture2D;
        _connectionStyle.border = new RectOffset(4, 4, 12, 12);

        _sidePanelStyle = new GUIStyle();
        _sidePanelStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/darkviewbackground.png") as Texture2D;
        _sidePanelStyle.normal.textColor = Color.white;
        _sidePanelStyle.border = new RectOffset(12, 12, 12, 12);

        _resizerStyle = new GUIStyle();
        _resizerStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/toolbar button on.png") as Texture2D;

        _assetNameStyle = new GUIStyle();
        _assetNameStyle.alignment = TextAnchor.LowerRight;
        _assetNameStyle.normal.textColor = Color.white;
        _assetNameStyle.fontStyle = FontStyle.Bold;
    }

    private void OnGUI()
    {
        _DrawGrid(20, 0.2f, Color.gray);
        _DrawGrid(100, 0.4f, Color.gray);

        _DrawNodes();
        _DrawConnections();
        _DrawConnectionLine(Event.current);

        _DrawSidePanel();
        _DrawResizer();

        EditorGUI.LabelField(
            new Rect(position.width - 220f, position.height - 50f, 200f, 30f),
            _TREE_ASSET_NAME,
            _assetNameStyle
        );

        _ProcessNodeEvents(Event.current);
        _ProcessEvents(Event.current);

        if (GUI.changed)
            Repaint();
    }

    private void _DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        _offset += _drag * 0.5f;
        Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(
                new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                new Vector3(gridSpacing * i, position.height, 0f) + newOffset
            );
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(
                new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                new Vector3(position.width, gridSpacing * j, 0f) + newOffset
            );
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void _DrawNodes()
    {
        if (_nodes != null)
        {
            for (int i = 0; i < _nodes.Count; i++)
                _nodes[i].Draw();
        }
    }

    private void _DrawConnections()
    {
        if (_connections != null)
        {
            for (int i = 0; i < _connections.Count; i++)
                _connections[i].Draw();
        }
    }

    private void _DrawConnectionLine(Event e)
    {
        if (_selectedInPoint != null && _selectedOutPoint == null)
        {
            Handles.DrawAAPolyLine(new Vector3[]
            {
                _selectedInPoint.Rect.center,
                e.mousePosition
            });
            GUI.changed = true;
        }

        if (_selectedOutPoint != null && _selectedInPoint == null)
        {
            Handles.DrawAAPolyLine(new Vector3[]
            {
                _selectedOutPoint.Rect.center,
                e.mousePosition
            });
            GUI.changed = true;
        }
    }

    private void _DrawSidePanel()
    {
        float sidePanelWidth = _sidePanelWidthRatio * position.width;

        // draw background
        GUI.Box(new Rect(0f, 0f, sidePanelWidth, position.height), "", _sidePanelStyle);

        // prepare content area
        GUILayout.BeginArea(new Rect(5f, 5f, sidePanelWidth - 10f, position.height - 10f));
        EditorGUILayout.LabelField("Node Settings", EditorStyles.boldLabel);

        if (_selectedNode == null)
        {
            EditorGUILayout.HelpBox("Select a node", MessageType.Info);
            GUILayout.EndArea();
            return;
        }

        TechnologyNodeData node = _selectedNode.DataNode;
        if (!node)
        {
            _selectedNode = null;
            GUILayout.EndArea();
            return;
        }

        SerializedObject so = new SerializedObject(node);
        // can't edit parameters for root node
        if (node.code == TechnologyNodeData.ROOT_NODE_CODE)
        {
            EditorGUILayout.HelpBox("Can't edit root node!", MessageType.Warning);
        }
        else
        {
            FieldInfo[] fields = node.GetType().GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic) continue;
                if (field.Name == "children") continue; // children are edited via edges
                if (field.Name == "code")
                {
                    string newCode = EditorGUILayout.DelayedTextField(
                        "Code", node.code);
                    if (node.code != newCode)
                    {
                        // update code
                        node.code = newCode;
                        EditorUtility.SetDirty(node);
                        AssetDatabase.SaveAssets();

                        // rename asset to match the code
                        string path = AssetDatabase.GetAssetPath(node);
                        AssetDatabase.RenameAsset(path, _ToCamelCase(newCode));
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
                else if (field.Name == "displayName")
                {
                    _selectedNode.Title = EditorGUILayout.TextField(
                        "Name", _selectedNode.Title);
                }
                else
                {
                    EditorGUILayout.PropertyField(so.FindProperty(field.Name), true);
                    so.ApplyModifiedProperties();
                }
            }
        }

        GUILayout.EndArea();
    }

    private void _DrawResizer()
    {
        _resizer = new Rect((position.width * _sidePanelWidthRatio) - 4f, 0, 10f, position.height);

        GUILayout.BeginArea(new Rect(_resizer.position, new Vector2(4, position.height)), _resizerStyle);
        GUILayout.EndArea();

        EditorGUIUtility.AddCursorRect(_resizer, MouseCursor.ResizeHorizontal);
    }

    private void _ProcessEvents(Event e)
    {
        _drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    _ClearConnectionSelection();
                    // if a node is currently active (but not being activated)
                    // and we're clicking outside of side panel: deselect
                    if (_selectedNode != null && !_selectingNode)
                    {
                        if (e.mousePosition.x > _sidePanelWidthRatio * position.width)
                            _selectedNode = null;
                    }

                    if (e.button == 0 && _resizer.Contains(e.mousePosition))
                    {
                        _isResizing = true;
                    }
                }

                if (e.button == 1)
                    _ProcessContextMenu(e.mousePosition);
                break;

            case EventType.MouseUp:
                _selectingNode = false;
                _isResizing = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && !_isResizing)
                    _OnDrag(e.delta);
                break;
        }

        if (_isResizing)
            _Resize(e);
    }

    private void _ProcessNodeEvents(Event e)
    {
        if (_nodes != null)
        {
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = _nodes[i].ProcessEvents(e);
                if (guiChanged)
                    GUI.changed = true;
            }
        }
    }

    private void _ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => _OnClickAddNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void _OnDrag(Vector2 delta)
    {
        _drag = delta;
        foreach (TechnologyTreeEditorNode node in _nodes)
            node.Drag(delta);
        GUI.changed = true;
    }

    private void _Resize(Event e)
    {
        _sidePanelWidthRatio = e.mousePosition.x / position.width;
        GUI.changed = true;
    }

    private void _OnClickAddNode(Vector2 mousePosition)
    {
        string code = $"Untitled_{_newNodeLastIndex.ToString("00")}";
        TechnologyNodeData node =
            ScriptableObject.CreateInstance<TechnologyNodeData>();
        node.Initialize(code);

        AssetDatabase.CreateAsset(
            node,
            System.IO.Path.Combine(_treeAssetsPath, code) + ".asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _newNodeLastIndex++;
        _nodes.Add(new TechnologyTreeEditorNode(
            node,
            mousePosition,
            _nodeWidth,
            _nodeHeight,
            _nodeStyle,
            _nodeSelectedStyle,
            _connectionStyle,
            _OnClickInPoint,
            _OnClickOutPoint,
            _OnClickRemoveNode,
            _OnClickSelectNode
        ));
    }

    private void _OnClickInPoint(ConnectionPoint inPoint)
    {
        _selectedInPoint = inPoint;

        if (_selectedOutPoint != null)
        {
            if (_selectedOutPoint.Node != _selectedInPoint.Node)
            {
                _CreateConnection();
                _ClearConnectionSelection();
            }
            else
            {
                _ClearConnectionSelection();
            }
        }
    }

    private void _OnClickOutPoint(ConnectionPoint outPoint)
    {
        _selectedOutPoint = outPoint;

        if (_selectedInPoint != null)
        {
            if (_selectedOutPoint.Node != _selectedInPoint.Node)
            {
                _CreateConnection();
                _ClearConnectionSelection();
            }
            else
            {
                _ClearConnectionSelection();
            }
        }
    }

    private void _OnClickRemoveConnection(Connection connection)
    {
        connection.Source.children.Remove(connection.Target);
        _connections.Remove(connection);

        EditorUtility.SetDirty(connection.Source);
        AssetDatabase.SaveAssets();
    }

    private void _OnClickSelectNode(TechnologyTreeEditorNode node)
    {
        _selectedNode = node;
        _selectingNode = true;
        GUI.changed = true;
    }

    private void _OnClickRemoveNode(TechnologyTreeEditorNode node)
    {
        List<Connection> connectionsToRemove = new List<Connection>();

        foreach (Connection c in _connections)
        {
            if (c.InPoint == node.InPoint || c.OutPoint == node.OutPoint)
                connectionsToRemove.Add(c);
        }

        foreach (Connection c in connectionsToRemove)
        {
            c.Source.children.Remove(c.Target);
            _connections.Remove(c);
        }

        if (_selectedNode == node)
            _selectedNode = null;
        _nodes.Remove(node);

        string path = AssetDatabase.GetAssetPath(node.DataNode);
        AssetDatabase.DeleteAsset(path);
    }

    private void _CreateConnection()
    {
        TechnologyNodeData source = _selectedOutPoint.Node.DataNode;
        TechnologyNodeData target = _selectedInPoint.Node.DataNode;
        source.children.Add(target);

        _connections.Add(new Connection(
            source,
            target,
            _selectedInPoint,
            _selectedOutPoint,
            _OnClickRemoveConnection
        ));

        EditorUtility.SetDirty(source);
        AssetDatabase.SaveAssets();
    }

    private void _ClearConnectionSelection()
    {
        _selectedInPoint = null;
        _selectedOutPoint = null;
    }

    private string _ToCamelCase(string code)
    {
        string[] tmp = code.Split('_');
        System.Text.StringBuilder sb = new System.Text.StringBuilder(tmp.Length);
        foreach (string s in tmp)
            sb.Append(s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower());
        return sb.ToString();
    }
}
