using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Scriptable Objects/Unit", order = 1)]
[System.Serializable]
public class UnitData : ScriptableObject
{
    public string code;
    public string unitName;
    public string description;
    public int healthpoints;
    public GameObject prefab;
    public Sprite sprite;
    public List<ResourceValue> cost;
    public InGameResource[] canProduce;
    public List<SkillData> skills = new List<SkillData>();
    public float fieldOfView;

    [Header("Attack")]
    public float attackRange;
    public int attackDamage;
    public float attackRate;

    [Header("General Sounds")]
    public AudioClip onSelectSound;

    public bool CanBuy(int owner)
    {
        return Globals.CanBuy(owner, cost);
    }
}
