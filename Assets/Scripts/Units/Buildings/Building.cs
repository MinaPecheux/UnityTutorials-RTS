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

    private MeshFilter _rendererMesh;
    private Mesh[] _constructionMeshes;
    private int _constructionHP;
    private List<CharacterManager> _constructors;
    private List<Transform> _smokeVfx;
    private BuildingBT _bt;

    private AudioClip _ambientSound;

    private bool _isAlive;

    public Building(BuildingData data, int owner) : this(data, owner, new List<ResourceValue>() { }) { }
    public Building(BuildingData data, int owner, List<ResourceValue> production) :
        base(data, owner, production)
    {
        _buildingManager = _transform.GetComponent<BuildingManager>();
        _bt = _transform.GetComponent<BuildingBT>();
        _bt.enabled = false;

        _constructionHP = 0;
        _constructors = new List<CharacterManager>();
        _smokeVfx = new List<Transform>();
        _isAlive = false;

        _materials = new List<Material>();
        foreach (Material material in _buildingManager.meshRenderer.materials)
            _materials.Add(new Material(material));
        SetMaterials();
        _placement = BuildingPlacement.VALID;

        _rendererMesh = _buildingManager.meshRenderer.GetComponent<MeshFilter>();
        _constructionMeshes = data.constructionMeshes;

        if (_buildingManager.ambientSource != null)
        {
            if (_owner == GameManager.instance.gamePlayersParameters.myPlayerId)
            {
                _buildingManager.ambientSource.clip =
                    GameManager.instance.gameSoundParameters.constructionSiteSound;
                _ambientSound = data.ambientSound;
            }
            else
            {
                _buildingManager.ambientSource.enabled = false;
            }
        }
        else
            Debug.LogWarning($"'{data.unitName}' prefab is missing an ambient audio source!");
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
        _buildingManager.meshRenderer.materials = materials.ToArray();
    }

    public override void Place(bool fromSavedData = false)
    {
        base.Place(fromSavedData);
        // set placement state
        _placement = BuildingPlacement.FIXED;
        // change building materials
        SetMaterials();
        // change building construction ratio
        SetConstructionHP(0);
    }

    public void SetConstructionHP(int constructionHP, bool fromSavedData = false)
    {
        if (_isAlive) return;

        _constructionHP = constructionHP;
        float constructionRatio = _constructionHP / (float) MaxHP;

        int meshIndex = Mathf.Max(
            0,
            (int)(_constructionMeshes.Length * constructionRatio) - 1);
        Mesh m = _constructionMeshes[meshIndex];
        _rendererMesh.sharedMesh = m;

        if (constructionRatio >= 1)
            _SetAlive(fromSavedData);
    }

    public void CheckValidPlacement()
    {
        if (_placement == BuildingPlacement.FIXED) return;
        _placement = _buildingManager.CheckPlacement()
            ? BuildingPlacement.VALID
            : BuildingPlacement.INVALID;
    }

    public void AddConstructor(CharacterManager m)
    {
        _constructors.Add(m);
        EventManager.TriggerEvent("UpdatedConstructors", this);

        // when adding first constructor, add some smoke VFX
        // + play construction sound
        if (_constructors.Count == 1)
        {
            List<Vector2> vfxPositions = Utils.SampleOffsets(3, 1.5f, Vector2.one * 4f);
            foreach (Vector2 offset in vfxPositions)
                _smokeVfx.Add(VFXManager.instance.Spawn(
                    VfxType.Smoke,
                    Transform.position + new Vector3(offset.x, 0, offset.y)));

            _buildingManager.ambientSource.Play();
        }
    }

    public void RemoveConstructor(int index)
    {
        CharacterBT bt = _constructors[index].GetComponent<CharacterBT>();
        bt.StopBuildingConstruction();
        _constructors.RemoveAt(index);
        EventManager.TriggerEvent("UpdatedConstructors", this);

        // when removing last constructor:
        if (_constructors.Count == 0)
        {
            // - remove smoke VFX
            foreach (Transform vfx in _smokeVfx)
                VFXManager.instance.Unspawn(VfxType.Smoke, vfx);
            _smokeVfx.Clear();
            // - stop construction sound
            _buildingManager.ambientSource.Pause();
        }
    }

    private void _SetAlive(bool fromSavedData = false)
    {
        _isAlive = true;
        _bt.enabled = true;
        ComputeProduction();
        Globals.UpdateNavMeshSurface();

        // when finishing construction, remove smoke VFX
        foreach (Transform vfx in _smokeVfx)
            VFXManager.instance.Unspawn(VfxType.Smoke, vfx);
        _smokeVfx.Clear();
        if (_owner == GameManager.instance.gamePlayersParameters.myPlayerId)
        {
            if (!fromSavedData)
                EventManager.TriggerEvent("PlaySoundByName", "onBuildingPlacedSound");
            // replace construction sound
            if (_ambientSound)
            {
                _buildingManager.ambientSource.clip = _ambientSound;
                _buildingManager.ambientSource.Play();
            }
            else
            {
                _buildingManager.ambientSource.Stop();
            }
        }
    }

    public int ConstructionHP { get => _constructionHP; }
    public List<CharacterManager> Constructors { get => _constructors; }
    public bool HasConstructorsFull { get => _constructors.Count == 3; }
    public bool HasValidPlacement { get => _placement == BuildingPlacement.VALID; }
    public bool IsFixed { get => _placement == BuildingPlacement.FIXED; }
    public override bool IsAlive { get => _isAlive; }
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
