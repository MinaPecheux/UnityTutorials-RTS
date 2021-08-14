using BehaviorTree;

public class CheckUnitIsMine: Node
{
    private bool _unitIsMine;

    public CheckUnitIsMine(UnitManager manager) : base()
    {
        _unitIsMine = manager.Unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerId;
    }

    public override NodeState Evaluate()
    {
        _state = _unitIsMine ? NodeState.SUCCESS : NodeState.FAILURE;
        return _state;
    }
}
