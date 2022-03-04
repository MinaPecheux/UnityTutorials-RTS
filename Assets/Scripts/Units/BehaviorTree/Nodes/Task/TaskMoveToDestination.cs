using UnityEngine;

using BehaviorTree;

public class TaskMoveToDestination : Node
{
    CharacterManager _manager;

    public TaskMoveToDestination(CharacterManager manager) : base()
    {
        _manager = manager;
    }

    public override NodeState Evaluate()
    {
        object destinationPoint = GetData("destinationPoint");
        Vector3 destination = (Vector3) destinationPoint;
        // check to see if the destination point was changed
        // and we need to re-update the agent's destination
        if (Vector3.Distance(destination, _manager.agent.destination) > 0.2f)
        {
            bool canMove = _manager.MoveTo(destination);
            _state = canMove ? NodeState.RUNNING : NodeState.FAILURE;
            return _state;
        }

        // check to see if the agent has reached the destination
        float d = Vector3.Distance(_manager.transform.position, _manager.agent.destination);
        if (d <= _manager.agent.stoppingDistance)
        {
            ClearData("destinationPoint");
            _manager.SetAnimatorBoolVariable("Running", false);
            _state = NodeState.SUCCESS;
            return _state;
        }

        _state = NodeState.RUNNING;
        return _state;
    }
}
