using UnityEngine;
using UnityEngine.AI;

public enum SkillType
{
    INSTANTIATE_CHARACTER,
    INSTANTIATE_BUILDING
}

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill", order = 4)]
public class SkillData : ScriptableObject
{
    public string code;
    public string skillName;
    public string description;
    public SkillType type;
    public UnitData unitReference;
    public float castTime;
    public float cooldown;
    public Sprite sprite;

    public AudioClip onStartSound;
    public AudioClip onEndSound;

    public void Trigger(GameObject source, GameObject target = null)
    {
        switch (type)
        {
            case SkillType.INSTANTIATE_CHARACTER:
                {
                    BoxCollider coll = source.GetComponent<BoxCollider>();
                    Vector3 instantiationPosition = new Vector3(
                        source.transform.position.x + coll.size.x,
                        source.transform.position.y,
                        source.transform.position.z - coll.size.z
                    );
                    CharacterData d = (CharacterData) unitReference;
                    UnitManager sourceUnitManager = source.GetComponent<UnitManager>();
                    if (sourceUnitManager == null)
                        return;
                    Character c = new Character(d, sourceUnitManager.Unit.Owner);
                    c.ComputeProduction();
                    c.Transform.GetComponent<NavMeshAgent>().Warp(instantiationPosition);
                }
                break;
            case SkillType.INSTANTIATE_BUILDING:
                {
                    UnitManager sourceUnitManager = source.GetComponent<UnitManager>();
                    if (sourceUnitManager == null)
                        return;
                    BuildingPlacer.instance.SelectPlacedBuilding(
                        (BuildingData) unitReference, sourceUnitManager);
                }
                break;
            default:
                break;
        }
    }
}
