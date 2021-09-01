using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Parameters", menuName = "Scriptable Objects/Game Global Parameters", order = 10)]
public class GameGlobalParameters : GameParameters
{
    public override string GetParametersName() => "Global";

    public delegate int ResourceProductionFunc(float distance);

    [Header("Day and Night")]
    public bool enableDayAndNightCycle;
    public float dayLengthInSeconds;
    public float dayInitialRatio;

    [Header("Units")]
    public BuildingData initialBuilding;
    public AnimationCurve experienceEvolutionCurve;
    public AnimationCurve productionMultiplierCurve;
    public AnimationCurve attackDamageMultiplierCurve;
    public AnimationCurve attackRangeMultiplierCurve;
    public AudioClip onLevelUpSound;

    [Header("Units production")]
    public int baseGoldProduction;
    public int bonusGoldProductionPerBuilding;
    public float goldBonusRange;
    public float woodProductionRange;
    public float stoneProductionRange;
    [HideInInspector]
    public ResourceProductionFunc woodProductionFunc = (float distance) =>
    {
        return Mathf.CeilToInt(10 * 1f / distance);
    };
    [HideInInspector]
    public ResourceProductionFunc stoneProductionFunc = (float distance) =>
    {
        return Mathf.CeilToInt(2 * 1f / distance);
    };

    [Header("FOV")]
    public bool enableFOV;

    public int UnitMaxLevel()
    {
       Keyframe[] keys = experienceEvolutionCurve.keys;
       return (int)keys.Select(k => k.time).Max();
    }
}
