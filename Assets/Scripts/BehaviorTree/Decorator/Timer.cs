using UnityEngine;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class Timer : Node
    {
        private float _delay;
        private float _time;

        public delegate void TickEnded();
        public event TickEnded onTickEnded;

        public Timer(float delay, TickEnded onTickEnded = null) : base()
        {
            _delay = delay;
            _time = _delay;
            this.onTickEnded = onTickEnded;
        }
        public Timer(float delay, List<Node> children, TickEnded onTickEnded = null)
            : base(children)
        {
            _delay = delay;
            _time = _delay;
            this.onTickEnded = onTickEnded;
        }

        public override bool IsFlowNode => true;

        public override NodeState Evaluate()
        {
            if (!HasChildren) return NodeState.FAILURE;
            if (_time <= 0)
            {
                _time = _delay;
                _state = children[0].Evaluate();
                if (onTickEnded != null)
                    onTickEnded();
                _state = NodeState.SUCCESS;
            }
            else
            {
                _time -= Time.deltaTime;
                _state = NodeState.RUNNING;
            }
            return _state;
        }
    }
}
