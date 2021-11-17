using UnityEngine;

[CreateAssetMenu(fileName = "Sound Parameters", menuName = "Scriptable Objects/Game Sound Parameters", order = 11)]
public class GameSoundParameters : GameParameters
{
    public override string GetParametersName() => "Sound";

    [Range(-80, -12)]
    public int musicVolume;

    [Range(-80, 0)]
    public int sfxVolume;

    [Header("Ambient sounds")]
    public AudioClip onDayStartSound;
    public AudioClip onNightStartSound;
    public AudioClip constructionSiteSound;
    public AudioClip onBuildingPlacedSound;
}
