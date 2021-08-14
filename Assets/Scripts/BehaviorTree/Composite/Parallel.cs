using System.Collections.Generic;

namespace BehaviorTree
{
    public class Parallel : Node
    {
        public Parallel() : base() { }
        public Parallel(List<Node> children) : base(children) { }

        public override bool IsFlowNode => true;

        public override NodeState Evaluate()
        {
            bool anyChildIsRunning = false;
            int nFailedChildren = 0;
            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        nFailedChildren++;
                        continue;
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.RUNNING:
                        anyChildIsRunning = true;
                        continue;
                    default:
                        _state = NodeState.SUCCESS;
                        return _state;
                }
            }
            if (nFailedChildren == children.Count)
                _state = NodeState.FAILURE;
            else
                _state = anyChildIsRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return _state;
        }
    }
}
