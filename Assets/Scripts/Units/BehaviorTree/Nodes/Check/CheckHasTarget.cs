using UnityEngine;

using BehaviorTree;

public class CheckHasTarget: Node
{
    public override NodeState Evaluate()
    {
        object currentTarget = Parent.GetData("currentTarget");
        if (currentTarget == null)
        {
            _state = NodeState.FAILURE;
            return _state;
        }
        object currentTargetOffset = Parent.GetData("currentTargetOffset");
        if (currentTargetOffset == null)
        {
            _state = NodeState.FAILURE;
            return _state;
        }

        // (in case the target object is gone - for example it died
        // and we haven't cleared it from the data yet)
        if (!((Transform) currentTarget))
        {
            Parent.ClearData("currentTarget");
            Parent.ClearData("currentTargetOffset");
            _state = NodeState.FAILURE;
            return _state;
        }

        _state = NodeState.SUCCESS;
        return _state;
    }
}
