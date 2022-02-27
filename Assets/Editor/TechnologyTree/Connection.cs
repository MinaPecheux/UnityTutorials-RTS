/* Adapted from:
   https://gram.gs/gramlog/creating-node-based-editor-unity/ */
using System;
using UnityEngine;
using UnityEditor;

public class Connection
{
    private ConnectionPoint _inPoint;
    private ConnectionPoint _outPoint;
    private Action<Connection> _onClickRemoveConnection;

    private TechnologyNodeData _source;
    private TechnologyNodeData _target;

    public Connection(
        TechnologyNodeData source,
        TechnologyNodeData target,
        ConnectionPoint inPoint,
        ConnectionPoint outPoint,
        Action<Connection> OnClickRemoveConnection
    )
    {
        _source = source;
        _target = target;
        _inPoint = inPoint;
        _outPoint = outPoint;
        _onClickRemoveConnection = OnClickRemoveConnection;
    }

    public void Draw()
    {
        Handles.DrawAAPolyLine(
            new Vector3[] { _inPoint.Rect.center, _outPoint.Rect.center });

        if (Handles.Button((
            _inPoint.Rect.center + _outPoint.Rect.center) * 0.5f,
            Quaternion.identity,
            4,
            8,
            Handles.RectangleHandleCap
        ))
        {
            if (_onClickRemoveConnection != null)
                _onClickRemoveConnection(this);
        }
    }

    public ConnectionPoint InPoint { get => _inPoint; }
    public ConnectionPoint OutPoint { get => _outPoint; }
    public Action<Connection> OnClickRemoveConnection { get => _onClickRemoveConnection; }
    public TechnologyNodeData Source { get => _source; }
    public TechnologyNodeData Target { get => _target; }
}
