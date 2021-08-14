using System.Collections.Generic;
using UnityEngine;

public enum BuildingPlacement
{
    VALID,
    INVALID,
    FIXED
};

public class Building : Unit
{

    private BuildingManager _buildingManager;
    private BuildingPlacement _placement;
    private List<Material> _materials;

    public Building(BuildingData data, int owner) : this(data, owner, new List<ResourceValue>() { }) { }
    public Building(BuildingData data, int owner, List<ResourceValue> production) :
        base(data, owner, production)
    {
        _buildingManager = _transform.GetComponent<BuildingManager>();
        _materials = new List<Material>();
        foreach (Material material in _transform.Find("Mesh").GetComponent<Renderer>().materials)
        {
            _materials.Add(new Material(material));
        }

        _placement = BuildingPlacement.VALID;
        SetMaterials();

        if (data.ambientSound != null)
        {
            if (_buildingManager.ambientSource != null)
            {
                _buildingManager.ambientSource.clip = data.ambientSound;
                _buildingManager.ambientSource.Play();
            }
            else
                Debug.LogWarning($"'{data.unitName}' prefab is missing an ambient audio source!");
        }
    }

    public void SetMaterials() { SetMaterials(_placement); }
    public void SetMaterials(BuildingPlacement placement)
    {
        List<Material> materials;
        if (placement == BuildingPlacement.VALID)
        {
            Material refMaterial = Resources.Load("Materials/Valid") as Material;
            materials = new List<Material>();
            for (int i = 0; i < _materials.Count; i++)
                materials.Add(refMaterial);
        }
        else if (placement == BuildingPlacement.INVALID)
        {
            Material refMaterial = Resources.Load("Materials/Invalid") as Material;
            materials = new List<Material>();
            for (int i = 0; i < _materials.Count; i++)
                materials.Add(refMaterial);
        }
        else if (placement == BuildingPlacement.FIXED)
            materials = _materials;
        else
            return;
        _transform.Find("Mesh").GetComponent<Renderer>().materials = materials.ToArray();
    }

    public override void Place()
    {
        base.Place();
        // set placement state
        _placement = BuildingPlacement.FIXED;
        // change building materials
        SetMaterials();
        // activate particles if there are any
        Transform particlesChild = _transform.Find("Particles");
        if (particlesChild)
            particlesChild.gameObject.SetActive(true);
    }

    public void CheckValidPlacement()
    {
        if (_placement == BuildingPlacement.FIXED) return;
        _placement = _buildingManager.CheckPlacement()
            ? BuildingPlacement.VALID
            : BuildingPlacement.INVALID;
    }

    public bool HasValidPlacement { get => _placement == BuildingPlacement.VALID; }
    public bool IsFixed { get => _placement == BuildingPlacement.FIXED; }
    public int DataIndex
    {
        get
        {
            for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
                if (Globals.BUILDING_DATA[i].code == _data.code)
                    return i;
            return -1;
        }
    }
}
