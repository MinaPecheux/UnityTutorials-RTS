using UnityEngine;

using BehaviorTree;

public class TaskFollow : Node
{
    CharacterManager _manager;
    Vector3 _lastTargetPosition;
    float _range;

    Transform _lastTarget;
    float _targetSize;

    public TaskFollow(CharacterManager manager) : base()
    {
        _manager = manager;
        _lastTargetPosition = Vector3.zero;
        _lastTarget = null;
    }

    public override NodeState Evaluate()
    {
        object currentTarget = GetData("currentTarget");
        Vector2 currentTargetOffset = (Vector2) GetData("currentTargetOffset");
        Transform target = (Transform)currentTarget;

        if (target != _lastTarget)
        {
            Vector3 s = target.GetComponent<UnitManager>().MeshSize;
            _targetSize = Mathf.Max(s.x, s.z);

            int targetOwner = target.GetComponent<UnitManager>().Unit.Owner;
            _range = (targetOwner != GameManager.instance.gamePlayersParameters.myPlayerId)
                ? _manager.Unit.AttackRange
                : ((CharacterData)_manager.Unit.Data).buildRange;
            _lastTarget = target;
        }

        Vector3 targetPosition = _GetTargetPosition(target, currentTargetOffset);

        if (targetPosition != _lastTargetPosition)
        {
            _manager.MoveTo(targetPosition, false);
            _lastTargetPosition = targetPosition;
        }

        // check if the agent has reached destination
        float d = Vector3.Distance(_manager.transform.position, _manager.agent.destination);
        if (d <= _manager.agent.stoppingDistance)
        {
            // if target is not mine: clear the data
            // (else keep it for the TaskBuild node)
            Unit u = ((Transform)currentTarget).GetComponent<UnitManager>().Unit;
            int targetOwner = u.Owner;
            if (targetOwner != GameManager.instance.gamePlayersParameters.myPlayerId)
            {
                ClearData("currentTarget");
                ClearData("currentTargetOffset");
            }
            else
            {
                int buildPower = ((CharacterData)_manager.Unit.Data).buildPower;
                if (u is Building b && !b.IsAlive)
                {
                    if (b.HasConstructorsFull)
                    {
                        ClearData("currentTarget");
                        ClearData("currentTargetOffset");
                    }
                    else if (
                        !_manager.IsConstructor &&
                        buildPower > 0
                    )
                    {
                        b.AddConstructor(_manager);
                        _manager.SetIsConstructor(true);
                        _manager.SetRendererVisibility(false);
                        _manager.agent.Warp(
                            target.position +
                            Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.right * _targetSize * 0.8f);
                    }
                }
            }
            _state = NodeState.SUCCESS;
            return _state;
        }

        _state = NodeState.RUNNING;
        return _state;
    }

    private Vector3 _GetTargetPosition(Transform target, Vector2 offset)
    {
        Vector3 p = _manager.transform.position;
        Vector3 t = new Vector3(target.position.x + offset.x, target.position.y, target.position.z + offset.y) - p;
        // (add a little offset to avoid bad collisions)
        float d = _targetSize + _range - 0.05f;
        float r = d / t.magnitude;
        return p + t * (1 - r);
    }
}
