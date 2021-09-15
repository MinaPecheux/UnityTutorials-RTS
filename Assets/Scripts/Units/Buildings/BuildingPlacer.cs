using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    private Building _placedBuilding = null;

    private Ray _ray;
    private RaycastHit _raycastHit;
    private Vector3 _lastPlacementPosition;

    private void Start()
    {
        // instantiate our headquarters
        SpawnBuilding(
            GameManager.instance.gameGlobalParameters.initialBuilding,
            GameManager.instance.gamePlayersParameters.myPlayerId,
            GameManager.instance.startPosition
        );
        //// instantiate our headquarters
        //SpawnBuilding(
        //    GameManager.instance.gameGlobalParameters.initialBuilding,
        //    1 - GameManager.instance.gamePlayersParameters.myPlayerId,
        //    GameManager.instance.startPosition - Vector3.right * 32f
        //);
    }

    void Update()
    {
        if (GameManager.instance.gameIsPaused) return;

        if (_placedBuilding != null)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _CancelPlacedBuilding();
                return;
            }

            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(
                _ray,
                out _raycastHit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            ))
            {
                _placedBuilding.SetPosition(_raycastHit.point);
                if (_lastPlacementPosition != _raycastHit.point)
                {
                    _placedBuilding.CheckValidPlacement();
                    Dictionary<InGameResource, int> prod = _placedBuilding.ComputeProduction();
                    EventManager.TriggerEvent("UpdatePlacedBuildingProduction", new object[] { prod, _raycastHit.point });
                }
                _lastPlacementPosition = _raycastHit.point;
            }

            if (
                _placedBuilding.HasValidPlacement &&
                Input.GetMouseButtonUp(0) &&
                !EventSystem.current.IsPointerOverGameObject()
            )
            {
                _PlaceBuilding();
            }

            if (Input.mouseScrollDelta.y != 0f)
                _placedBuilding.Transform.Rotate(
                    Vector3.up,
                    Mathf.Sign(Input.mouseScrollDelta.y) * 15f,
                    Space.World
                );
        }
    }

    private void OnEnable()
    {
        EventManager.AddListener("<Input>Build", _OnBuildInput);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("<Input>Build", _OnBuildInput);
    }

    private void _OnBuildInput(object data)
    {
        string buildingCode = (string)data;
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            if (Globals.BUILDING_DATA[i].code == buildingCode)
            {
                SelectPlacedBuilding(i);
                return;
            }
        }
    }

    public void SpawnBuilding(BuildingData data, int owner, Vector3 position)
    {
        SpawnBuilding(data, owner, position, new List<ResourceValue>() { });
    }
    public void SpawnBuilding(BuildingData data, int owner, Vector3 position, List<ResourceValue> production)
    {
        // keep a reference to the previously placed building, if there is one
        Building prevPlacedBuilding = _placedBuilding;

        // instantiate building
        _placedBuilding = new Building(data, owner, production);
        _placedBuilding.SetPosition(position);
        // finish up the placement
        _PlaceBuilding(false);

        // restore the previous state
        _placedBuilding = prevPlacedBuilding;
    }

    void _PreparePlacedBuilding(int buildingDataIndex)
    {
        // destroy the previous "phantom" if there is one
        if (_placedBuilding != null && !_placedBuilding.IsFixed)
        {
            Destroy(_placedBuilding.Transform.gameObject);
        }
        Building building = new Building(
            Globals.BUILDING_DATA[buildingDataIndex],
            GameManager.instance.gamePlayersParameters.myPlayerId
        );
        _placedBuilding = building;
        _lastPlacementPosition = Vector3.zero;
        EventManager.TriggerEvent("PlaceBuildingOn");
    }

    void _CancelPlacedBuilding()
    {
        // destroy the "phantom" building
        Destroy(_placedBuilding.Transform.gameObject);
        _placedBuilding = null;
        EventManager.TriggerEvent("PlaceBuildingOff");
    }

    void _PlaceBuilding(bool canChain = true)
    {
        _placedBuilding.ComputeProduction();
        _placedBuilding.Place();
        if (canChain)
        {
            if (_placedBuilding.CanBuy())
                _PreparePlacedBuilding(_placedBuilding.DataIndex);
            else
            {
                EventManager.TriggerEvent("PlaceBuildingOff");
                _placedBuilding = null;
            }
        }
        EventManager.TriggerEvent("PlaySoundByName", "onBuildingPlacedSound");
        EventManager.TriggerEvent("UpdateResourceTexts");
        EventManager.TriggerEvent("CheckBuildingButtons");
        Globals.UpdateNavMeshSurface();
    }

    public void SelectPlacedBuilding(int buildingDataIndex)
    {
        _PreparePlacedBuilding(buildingDataIndex);
    }
}
