using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskTrySetDestinationOrTarget : Node
{
    CharacterManager _manager;

    private Ray _ray;
    private RaycastHit _raycastHit;

    private const float _samplingRange = 12f;
    private const float _samplingRadius = 1.8f;

    public TaskTrySetDestinationOrTarget(CharacterManager manager) : base()
    {
        _manager = manager;
    }

    public override NodeState Evaluate()
    {
        if (_manager.IsSelected && Input.GetMouseButtonUp(1))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(
                _ray,
                out _raycastHit,
                1000f,
                Globals.UNIT_MASK
            ))
            {
                UnitManager um = _raycastHit.collider.GetComponent<UnitManager>();
                if (um != null)
                {
                    Parent.Parent.SetData("currentTarget", _raycastHit.transform);
                    if (_manager.SelectIndex == 0)
                    {
                        List<Vector2> targetOffsets = _ComputeFormationTargetOffsets();
                        EventManager.TriggerEvent("TargetFormationOffsets", targetOffsets);
                    }
                    _state = NodeState.SUCCESS;
                    return _state;
                }
            }

            else if (Physics.Raycast(
                _ray,
                out _raycastHit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            ))
            {
                if (_manager.SelectIndex == 0)
                {
                    List<Vector3> targetPositions = _ComputeFormationTargetPositions(_raycastHit.point);
                    EventManager.TriggerEvent("TargetFormationPositions", targetPositions);
                }
                _state = NodeState.SUCCESS;
                return _state;
            }
        }
        _state = NodeState.FAILURE;
        return _state;
    }

    private List<Vector2> _ComputeFormationTargetOffsets()
    {
        int nSelectedUnits = Globals.SELECTED_UNITS.Count;
        List<Vector2> offsets = new List<Vector2>(nSelectedUnits);
        // leader unit goes to the exact target point
        offsets.Add(Vector2.zero);
        if (nSelectedUnits == 1) // (abort early if no other unit is selected)
            return offsets;

        // next units have offsets computed with the chosen formation pattern:
        // - None -> "random" using Poisson disc sampling
        // - Line
        // - Grid
        // - XCross
        if (Globals.UNIT_FORMATION_TYPE == UnitFormationType.None)
            offsets.AddRange(Utils.SampleOffsets(
                nSelectedUnits - 1, _samplingRadius, _samplingRange * Vector2.one));
        else
        {
            Vector3 dir = _raycastHit.point - _manager.transform.position;
            if (Globals.UNIT_FORMATION_TYPE == UnitFormationType.Line)
                offsets = UnitsFormation.GetLineOffsets(nSelectedUnits, _samplingRadius, dir);
            else if (Globals.UNIT_FORMATION_TYPE == UnitFormationType.Grid)
                offsets = UnitsFormation.GetGridOffsets(nSelectedUnits, _samplingRadius, dir);
            else if (Globals.UNIT_FORMATION_TYPE == UnitFormationType.XCross)
                offsets = UnitsFormation.GetXCrossOffsets(nSelectedUnits, _samplingRadius, dir);
        }
        return offsets;
    }

    private List<Vector3> _ComputeFormationTargetPositions(Vector3 hitPoint)
    {
        int nSelectedUnits = Globals.SELECTED_UNITS.Count;
        List<Vector3> positions = new List<Vector3>(nSelectedUnits);
        // leader unit goes to the exact target point
        positions.Add(hitPoint);
        if (nSelectedUnits == 1) // (abort early if no other unit is selected)
            return positions;

        // next units have positions computed with the chosen formation pattern:
        // - None -> "random" using Poisson disc sampling
        // - Line
        // - Grid
        // - XCross
        if (Globals.UNIT_FORMATION_TYPE == UnitFormationType.None)
            positions.AddRange(Utils.SamplePositions(
                nSelectedUnits - 1, _samplingRadius, _samplingRange * Vector2.one, hitPoint));
        else
        {
            Vector3 dir = hitPoint - _manager.transform.position;
            if (Globals.UNIT_FORMATION_TYPE == UnitFormationType.Line)
                positions = UnitsFormation.GetLinePositions(nSelectedUnits, _samplingRadius, dir, hitPoint);
            else if (Globals.UNIT_FORMATION_TYPE == UnitFormationType.Grid)
                positions = UnitsFormation.GetGridPositions(nSelectedUnits, _samplingRadius, dir, hitPoint);
            else if (Globals.UNIT_FORMATION_TYPE == UnitFormationType.XCross)
                positions = UnitsFormation.GetXCrossPositions(nSelectedUnits, _samplingRadius, dir, hitPoint);
        }
        return positions;
    }

    public void SetFormationTargetOffset(List<Vector2> targetOffsets, Transform targetTransform = null)
    {
        int i = _manager.SelectIndex;
        if (i < 0) return; // (unit is not selected anymore)
        ClearData("destinationPoint");
        Parent.Parent.SetData("currentTargetOffset", targetOffsets[i]);
        if (targetTransform != null)
            Parent.Parent.SetData("currentTarget", targetTransform);
        _manager.SetAnimatorBoolVariable("Running", true);
    }

    public void SetFormationTargetPosition(List<Vector3> targetPositions)
    {
        int i = _manager.SelectIndex;
        if (i < 0) return; // (unit is not selected anymore)
        ClearData("currentTarget");
        ClearData("currentTargetOffset");
        Parent.Parent.SetData("destinationPoint", targetPositions[i]);
        _manager.SetAnimatorBoolVariable("Running", true);
    }
}
