using UnityEngine;

using BehaviorTree;

public class TaskFollow : Node
{
    CharacterManager _manager;
    Vector3 _lastTargetPosition;

    public TaskFollow(CharacterManager manager) : base()
    {
        _manager = manager;
        _lastTargetPosition = Vector3.zero;
    }

    public override NodeState Evaluate()
    {
        object currentTarget = GetData("currentTarget");
        Vector2 currentTargetOffset = (Vector2) GetData("currentTargetOffset");
        Vector3 targetPosition = _GetTargetPosition((Transform)currentTarget, currentTargetOffset);

        if (targetPosition != _lastTargetPosition)
        {
            _manager.MoveTo(targetPosition, false);
            _lastTargetPosition = targetPosition;
        }

        // check if the agent has reached destination
        float d = Vector3.Distance(_manager.transform.position, _manager.agent.destination);
        if (d <= _manager.agent.stoppingDistance)
        {
            ClearData("currentTarget");
            _state = NodeState.SUCCESS;
            return _state;
        }

        _state = NodeState.RUNNING;
        return _state;
    }

    private Vector3 _GetTargetPosition(Transform target, Vector2 offset)
    {
        Vector3 s = target.Find("Mesh").localScale;
        float targetSize = Mathf.Max(s.x, s.z);

        Vector3 p = _manager.transform.position;
        Vector3 t = new Vector3(target.position.x + offset.x, target.position.y, target.position.z + offset.y) - p;
        // (add a little offset to avoid bad collisions)
        float d = targetSize + _manager.Unit.Data.attackRange - 0.2f;
        float r = d / t.magnitude;
        return p + t * (1 - r);
    }
}
