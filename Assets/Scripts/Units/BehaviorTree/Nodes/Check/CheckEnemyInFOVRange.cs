using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using BehaviorTree;

public class CheckEnemyInFOVRange : Node
{
    UnitManager _manager;
    float _fovRadius;
    int _unitOwner;

    Vector3 _pos;

    public CheckEnemyInFOVRange(UnitManager manager) : base()
    {
        _manager = manager;
        _fovRadius = _manager.Unit.Data.fieldOfView;
        _unitOwner = _manager.Unit.Owner;
    }

    public override NodeState Evaluate()
    {
        _pos = _manager.transform.position;
        IEnumerable<Collider> enemiesInRange =
            Physics.OverlapSphere(_pos, _fovRadius, Globals.UNIT_MASK)
            .Where(delegate (Collider c)
            {
                UnitManager um = c.GetComponent<UnitManager>();
                if (um == null) return false;
                return um.Unit.Owner != _unitOwner;
            });
        if (enemiesInRange.Any())
        {
            Parent.SetData(
                "currentTarget",
                enemiesInRange
                    .OrderBy(x => (x.transform.position - _pos).sqrMagnitude)
                    .First()
                    .transform
            );
            Parent.SetData("currentTargetOffset", Vector2.zero);
            _state = NodeState.SUCCESS;
            return _state;
        }
        _state = NodeState.FAILURE;
        return _state;
    }
}
