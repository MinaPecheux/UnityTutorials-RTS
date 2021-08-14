using System.Collections.Generic;

namespace BehaviorTree
{
    public class Inverter : Node
    {
        public Inverter() : base() { }
        public Inverter(List<Node> children) : base(children) { }

        public override bool IsFlowNode => true;

        public override NodeState Evaluate()
        {
            if (!HasChildren) return NodeState.FAILURE;
            switch (children[0].Evaluate())
            {
                case NodeState.FAILURE:
                    _state = NodeState.SUCCESS;
                    return _state;
                case NodeState.SUCCESS:
                    _state = NodeState.FAILURE;
                    return _state;
                case NodeState.RUNNING:
                    _state = NodeState.RUNNING;
                    return _state;
                default:
                    _state = NodeState.FAILURE;
                    return _state;
            }
        }
    }
}
