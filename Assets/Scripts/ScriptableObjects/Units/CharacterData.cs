using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character", order = 3)]
public class CharacterData : UnitData
{
    [Header("Unit Sounds")]
    public AudioClip onMoveValidSound;
    public AudioClip onMoveInvalidSound;

    [Header("Build")]
    public float buildRange;
    public int buildPower;
    public float buildRate;
}
