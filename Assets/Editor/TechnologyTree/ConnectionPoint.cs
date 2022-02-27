/* Adapted from:
   https://gram.gs/gramlog/creating-node-based-editor-unity/ */
using System;
using UnityEngine;

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
    private Rect _rect;
    private ConnectionPointType _type;
    private TechnologyTreeEditorNode _node;
    private GUIStyle _style;
    private Action<ConnectionPoint> _OnClickConnectionPoint;

    public ConnectionPoint(
        TechnologyTreeEditorNode node,
        ConnectionPointType type,
        GUIStyle style,
        Action<ConnectionPoint> OnClickConnectionPoint
    )
    {
        _node = node;
        _type = type;
        _style = style;
        _OnClickConnectionPoint = OnClickConnectionPoint;
        _rect = new Rect(0, 0, 20f, 12f);
    }

    public void Draw()
    {
        _rect.x = _node.Rect.x + (_node.Rect.width * 0.5f) - _rect.width * 0.5f;

        switch (_type)
        {
            case ConnectionPointType.In:
                _rect.y = _node.Rect.y - _rect.height + 8f;
                break;

            case ConnectionPointType.Out:
                _rect.y = _node.Rect.y + _node.Rect.height - 8f;
                break;
        }

        if (GUI.Button(_rect, "", _style))
        {
            if (_OnClickConnectionPoint != null)
                _OnClickConnectionPoint(this);
        }
    }

    public Rect Rect { get => _rect; }
    public ConnectionPointType Type { get => _type; }
    public TechnologyTreeEditorNode Node { get => _node; }
    public GUIStyle Style { get => _style; }
    public Action<ConnectionPoint> OnClickConnectionPoint { get => _OnClickConnectionPoint; }
}
