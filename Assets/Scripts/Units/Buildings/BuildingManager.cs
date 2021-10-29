using UnityEngine;

public class BuildingManager : UnitManager
{
    public AudioSource ambientSource;

    private Building _building;
    public override Unit Unit
    {
        get { return _building; }
        set { _building = value is Building ? (Building)value : null; }
    }
    private int _nCollisions = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Terrain") return;
        _nCollisions++;
        CheckPlacement();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Terrain") return;
        _nCollisions--;
        CheckPlacement();
    }

    public bool CheckPlacement()
    {
        if (_building == null) return false;
        if (_building.IsFixed) return false;
        bool validPlacement = HasValidPlacement();
        if (!validPlacement)
            _building.SetMaterials(BuildingPlacement.INVALID);
        else
            _building.SetMaterials(BuildingPlacement.VALID);
        return validPlacement;
    }

    public bool HasValidPlacement()
    {
        if (_nCollisions > 0) return false;

        // get 4 bottom corner positions
        Vector3 p = transform.position;
        Vector3 c = _collider.center;
        Vector3 e = _collider.size / 2f;
        float bottomHeight = c.y - e.y + 0.5f;
        Vector3[] bottomCorners = new Vector3[]
        {
        new Vector3(c.x - e.x, bottomHeight, c.z - e.z),
        new Vector3(c.x - e.x, bottomHeight, c.z + e.z),
        new Vector3(c.x + e.x, bottomHeight, c.z - e.z),
        new Vector3(c.x + e.x, bottomHeight, c.z + e.z)
        };
        // cast a small ray beneath the corner to check for a close ground
        // (if at least two are not valid, then placement is invalid)
        int invalidCornersCount = 0;
        foreach (Vector3 corner in bottomCorners)
        {
            if (!Physics.Raycast(
                p + corner,
                Vector3.up * -1f,
                2f,
                Globals.TERRAIN_LAYER_MASK
            ))
                invalidCornersCount++;
        }
        return invalidCornersCount < 3;
    }

    public bool Build(int buildPower)
    {
        _building.SetConstructionRatio(_building.ConstructionRatio + buildPower / 10f);
        UpdateHealthbar();
        return _building.IsAlive;
    }

    protected override bool IsActive()
    {
        return _building.IsFixed;
    }

    protected override bool IsMovable()
    {
        return false;
    }

    protected override void UpdateHealthbar()
    {
        if (!healthbar) return;
        Transform fill = healthbar.transform.Find("Fill");

        // if in construction: show current construction ratio
        if (IsActive() && !_building.IsAlive)
            fill.GetComponent<UnityEngine.UI.Image>().fillAmount = _building.ConstructionRatio;
        // else show health ratio a usual
        else
            fill.GetComponent<UnityEngine.UI.Image>().fillAmount = Unit.HP / (float)Unit.MaxHP;
    }
}
