/* Adapted from:
   https://gram.gs/gramlog/creating-node-based-editor-unity/ */
using System;
using UnityEditor;
using UnityEngine;

public class TechnologyTreeEditorNode
{
    private Rect _rect;
    private string _title;
    private bool _isDragged;
    private bool _isSelected;

    private GUIStyle _style;
    private GUIStyle _defaultNodeStyle;
    private GUIStyle _selectedNodeStyle;

    private ConnectionPoint _inPoint;
    private ConnectionPoint _outPoint;

    private Action<TechnologyTreeEditorNode> _onRemoveNode;
    private Action<TechnologyTreeEditorNode> _onSelectNode;

    private TechnologyNodeData _node;

    public TechnologyTreeEditorNode(
        TechnologyNodeData node,
        Vector2 position,
        float width,
        float height,
        GUIStyle nodeStyle,
        GUIStyle nodeSelectedStyle,
        GUIStyle connectionStyle,
        Action<ConnectionPoint> OnClickInPoint,
        Action<ConnectionPoint> OnClickOutPoint,
        Action<TechnologyTreeEditorNode> OnClickRemoveNode,
        Action<TechnologyTreeEditorNode> OnClickSelectNode
    )
    {
        _node = node;
        _title = node.displayName;
        _rect = new Rect(position.x, position.y, width, height);
        _style = nodeStyle;
        _defaultNodeStyle = nodeStyle;
        _selectedNodeStyle = nodeSelectedStyle;
        if (_node.code == TechnologyNodeData.ROOT_NODE_CODE)
            _inPoint = null;
        else
            _inPoint = new ConnectionPoint(this, ConnectionPointType.In, connectionStyle, OnClickInPoint);
        _outPoint = new ConnectionPoint(this, ConnectionPointType.Out, connectionStyle, OnClickOutPoint);
        _onRemoveNode = OnClickRemoveNode;
        _onSelectNode = OnClickSelectNode;
    }

    public void Draw()
    {
        if (_inPoint != null)
            _inPoint.Draw();
        _outPoint.Draw();
        GUI.Box(_rect, _title, _style);
    }

    public void Drag(Vector2 delta)
    {
        _rect.position += delta;
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (_rect.Contains(e.mousePosition))
                    {
                        _isDragged = true;
                        _isSelected = true;
                        _style = _selectedNodeStyle;
                        if (_onSelectNode != null)
                            _onSelectNode(this);
                        return true;
                    }
                    else
                    {
                        _isSelected = false;
                        _style = _defaultNodeStyle;
                        return true;
                    }
                }

                if (e.button == 1 && _isSelected && _rect.Contains(e.mousePosition))
                {
                    _ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                _isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && _isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    private void _ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, _OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }

    private void _OnClickRemoveNode()
    {
        if (_onRemoveNode != null)
            _onRemoveNode(this);
    }

    public Rect Rect { get => _rect; }
    public string Title {
        get => _title;
        set
        {
            _title = value;
            _node.displayName = value;
        }
    }
    public ConnectionPoint InPoint { get => _inPoint; }
    public ConnectionPoint OutPoint { get => _outPoint; }
    public TechnologyNodeData DataNode { get => _node; }
}
