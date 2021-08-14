using BehaviorTree;

public class CheckHasDestination : Node
{
    public override NodeState Evaluate()
    {
        object destinationPoint = GetData("destinationPoint");
        if (destinationPoint == null)
        {
            _state = NodeState.FAILURE;
            return _state;
        }
        _state = NodeState.SUCCESS;
        return _state;
    }
}
