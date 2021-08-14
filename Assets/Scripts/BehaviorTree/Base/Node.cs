using System.Collections.Generic;

namespace BehaviorTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public class Node
    {
        protected NodeState _state;
        public NodeState State { get => _state; }

        private Node _parent;
        protected List<Node> children = new List<Node>();
        private Dictionary<string, object> _dataContext = new Dictionary<string, object>();

        public Node()
        {
            _parent = null;
        }
        public Node(List<Node> children) : this()
        {
            SetChildren(children);
        }

        public virtual NodeState Evaluate() => NodeState.FAILURE;

        public void SetChildren(List<Node> children)
        {
            foreach (Node c in children)
                Attach(c);
        }

        public void Attach(Node child)
        {
            children.Add(child);
            child._parent = this;
        }

        public void Detach(Node child)
        {
            children.Remove(child);
            child._parent = null;
        }

        public object GetData(string key)
        {
            object value = null;
            if (_dataContext.TryGetValue(key, out value))
                return value;

            Node node = _parent;
            while (node != null)
            {
                value = node.GetData(key);
                if (value != null)
                    return value;
                node = node._parent;
            }
            return null;
        }

        public bool ClearData(string key)
        {
            if (_dataContext.ContainsKey(key))
            {
                _dataContext.Remove(key);
                return true;
            }

            Node node = _parent;
            while (node != null)
            {
                bool cleared = node.ClearData(key);
                if (cleared)
                    return true;
                node = node._parent;
            }
            return false;
        }

        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
        }

        public Node Parent { get => _parent; }
        public List<Node> Children { get => children; }
        public bool HasChildren { get => children.Count > 0; }
        public virtual bool IsFlowNode => false;
    }
}
