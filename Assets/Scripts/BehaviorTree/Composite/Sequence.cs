using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree
{
    public class Sequence : Node
    {
        private bool _isRandom;

        public Sequence() : base() { _isRandom = false; }
        public Sequence(bool isRandom) : base() { _isRandom = isRandom; }
        public Sequence(List<Node> children, bool isRandom = false) : base(children)
        {
            _isRandom = isRandom;
        }

        public override bool IsFlowNode => true;

        public static List<T> Shuffle<T>(List<T> list)
        {
            System.Random r = new System.Random();
            return list.OrderBy(x => r.Next()).ToList();
        }

        public override NodeState Evaluate()
        {
            bool anyChildIsRunning = false;
            if (_isRandom)
                children = Shuffle(children);

            foreach (Node node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        _state = NodeState.FAILURE;
                        return _state;
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
            _state = anyChildIsRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return _state;
        }
    }
}
