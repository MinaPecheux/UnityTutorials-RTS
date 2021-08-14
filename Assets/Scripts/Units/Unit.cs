using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit
{

    protected string _uid;
    protected UnitData _data;
    protected Transform _transform;
    protected int _currentHealth;
    protected int _level;
    protected float _fieldOfView;
    protected Dictionary<InGameResource, int> _production;
    protected List<SkillManager> _skillManagers;
    protected int _owner;

    public Unit(UnitData data, int owner) : this(data, owner, new List<ResourceValue>() { }) { }
    public Unit(UnitData data, int owner, List<ResourceValue> production)
    {
        _uid = System.Guid.NewGuid().ToString();
        _data = data;
        _currentHealth = data.healthpoints;
        _level = 1;
        _production = production.ToDictionary(rv => rv.code, rv => rv.amount);
        _fieldOfView = data.fieldOfView;
        _owner = owner;

        GameObject g = GameObject.Instantiate(data.prefab) as GameObject;
        _transform = g.transform;
        _transform.GetComponent<UnitManager>().SetOwnerMaterial(owner);

        _skillManagers = new List<SkillManager>();
        SkillManager sm;
        foreach (SkillData skill in _data.skills)
        {
            sm = g.AddComponent<SkillManager>();
            sm.Initialize(skill, g);
            _skillManagers.Add(sm);
        }

        _transform.GetComponent<UnitManager>().Initialize(this);
    }

    public void SetPosition(Vector3 position)
    {
        _transform.position = position;
    }

    public Dictionary<InGameResource, int> ComputeProduction()
    {
        if (_data.canProduce.Length == 0) return null;

        GameGlobalParameters globalParams = GameManager.instance.gameGlobalParameters;
        GamePlayersParameters playerParams = GameManager.instance.gamePlayersParameters;
        Vector3 pos = _transform.position;

        if (_data.canProduce.Contains(InGameResource.Gold))
        {
            int bonusBuildingsCount =
                Physics.OverlapSphere(pos, globalParams.goldBonusRange, Globals.UNIT_MASK)
                .Where(delegate (Collider c) {
                    BuildingManager m = c.GetComponent<BuildingManager>();
                    if (m == null) return false;
                    return m.Unit.Owner == playerParams.myPlayerId;
                })
                .Count();

            _production[InGameResource.Gold] =
                globalParams.baseGoldProduction + bonusBuildingsCount * globalParams.bonusGoldProductionPerBuilding;
        }

        if (_data.canProduce.Contains(InGameResource.Wood))
        {
            int treesScore =
                Physics.OverlapSphere(pos, globalParams.woodProductionRange, Globals.TREE_MASK)
                .Select((c) => globalParams.woodProductionFunc(Vector3.Distance(pos, c.transform.position)))
                .Sum();
            _production[InGameResource.Wood] = treesScore;
        }

        if (_data.canProduce.Contains(InGameResource.Stone))
        {
            int rocksScore =
                Physics.OverlapSphere(pos, globalParams.woodProductionRange, Globals.ROCK_MASK)
                .Select((c) => globalParams.stoneProductionFunc(Vector3.Distance(pos, c.transform.position)))
                .Sum();
            _production[InGameResource.Stone] = rocksScore;
        }

        return _production;
    }

    public virtual void Place()
    {
        // remove "is trigger" flag from box collider to allow
        // for collisions with units
        _transform.GetComponent<BoxCollider>().isTrigger = false;

        if (_owner == GameManager.instance.gamePlayersParameters.myPlayerId)
        {
            _transform.GetComponent<UnitManager>().EnableFOV(_fieldOfView);

            // update game resources: remove the cost of the building
            // from each game resource
            foreach (ResourceValue resource in _data.cost)
            {
                Globals.GAME_RESOURCES[resource.code].AddAmount(-resource.amount);
            }
        }
    }

    public bool CanBuy()
    {
        return _data.CanBuy();
    }

    public void LevelUp()
    {
        _level += 1;
    }

    public void ProduceResources()
    {
        foreach (KeyValuePair<InGameResource, int> resource in _production)
            Globals.GAME_RESOURCES[resource.Key].AddAmount(resource.Value);
    }

    public void TriggerSkill(int index, GameObject target = null)
    {
        _skillManagers[index].Trigger(target);
    }

    public string Uid { get => _uid; }
    public UnitData Data { get => _data; }
    public string Code { get => _data.code; }
    public Transform Transform { get => _transform; }
    public int HP { get => _currentHealth; set => _currentHealth = value; }
    public int MaxHP { get => _data.healthpoints; }
    public int Level { get => _level; }
    public Dictionary<InGameResource, int> Production { get => _production; }
    public List<SkillManager> SkillManagers { get => _skillManagers; }
    public int Owner { get => _owner; }
}
