using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitsSelection : MonoBehaviour
{
    public UIManager uiManager;

    private bool _isDraggingMouseBox = false;
    private Vector3 _dragStartPosition;

    Ray _ray;
    RaycastHit _raycastHit;

    private Dictionary<int, List<UnitManager>> _selectionGroups = new Dictionary<int, List<UnitManager>>();

    private void Update()
    {
        if (GameManager.instance.gameIsPaused) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // start dragging
        if (Input.GetMouseButtonDown(0))
        {
            _isDraggingMouseBox = true;
            _dragStartPosition = Input.mousePosition;
        }

        // stop dragging
        if (Input.GetMouseButtonUp(0))
            _isDraggingMouseBox = false;

        // while dragging
        if (_isDraggingMouseBox && _dragStartPosition != Input.mousePosition)
            _SelectUnitsInDraggingBox();

        // on deselect (either <Escape> pressed or clicking on the ground)
        if (Globals.SELECTED_UNITS.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                _DeselectAllUnits();
            if (Input.GetMouseButtonDown(0))
            {
                _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(
                    _ray,
                    out _raycastHit,
                    1000f
                ))
                {
                    if (_raycastHit.transform.tag == "Terrain")
                        _DeselectAllUnits();
                }
            }
        }

        // manage selection groups with alphanumeric keys
        if (Input.anyKeyDown)
        {
            int alphaKey = Utils.GetAlphaKeyValue(Input.inputString);
            if (alphaKey != -1)
            {
                if (
                    Input.GetKey(KeyCode.LeftControl) ||
                    Input.GetKey(KeyCode.RightControl) ||
                    Input.GetKey(KeyCode.LeftApple) ||
                    Input.GetKey(KeyCode.RightApple)
                )
                    _CreateSelectionGroup(alphaKey);
                else
                    _ReselectGroup(alphaKey);
            }
        }
    }

    void OnGUI()
    {
        if (_isDraggingMouseBox)
        {
            // Create a rect from both mouse positions
            var rect = Utils.GetScreenRect(_dragStartPosition, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.5f, 1f, 0.4f, 0.2f));
            Utils.DrawScreenRectBorder(rect, 1, new Color(0.5f, 1f, 0.4f));
        }
    }

    public void SelectUnitsGroup(int groupIndex)
    {
        _ReselectGroup(groupIndex);
    }

    private void _SelectUnitsInDraggingBox()
    {
        Bounds selectionBounds = Utils.GetViewportBounds(
            Camera.main,
            _dragStartPosition,
            Input.mousePosition
        );
        GameObject[] selectableUnits = GameObject.FindGameObjectsWithTag("Unit");
        bool inBounds;
        foreach (GameObject unit in selectableUnits)
        {
            inBounds = selectionBounds.Contains(
                Camera.main.WorldToViewportPoint(unit.transform.position)
            );
            if (inBounds)
                unit.GetComponent<UnitManager>().Select();
            else
                unit.GetComponent<UnitManager>().Deselect();
        }
    }

    private void _DeselectAllUnits()
    {
        List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        foreach (UnitManager um in selectedUnits)
            um.Deselect();
    }

    private void _CreateSelectionGroup(int groupIndex)
    {
        // check there are units currently selected
        if (Globals.SELECTED_UNITS.Count == 0)
        {
            if (_selectionGroups.ContainsKey(groupIndex))
                _RemoveSelectionGroup(groupIndex);
            return;
        }
        List<UnitManager> groupUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
        _selectionGroups[groupIndex] = groupUnits;
        uiManager.ToggleSelectionGroupButton(groupIndex, true);
    }

    private void _RemoveSelectionGroup(int groupIndex)
    {
        _selectionGroups.Remove(groupIndex);
        uiManager.ToggleSelectionGroupButton(groupIndex, false);
    }

    private void _ReselectGroup(int groupIndex)
    {
        // check the group actually is defined
        if (!_selectionGroups.ContainsKey(groupIndex)) return;
        _DeselectAllUnits();
        foreach (UnitManager um in _selectionGroups[groupIndex])
            um.Select();
    }

}

