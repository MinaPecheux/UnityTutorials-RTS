using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Scriptable Objects/Building", order = 2)]
public class BuildingData : UnitData
{

    [Header("Building Sounds")]
    public AudioClip ambientSound;

    [Header("Construction")]
    public Mesh[] constructionMeshes;

}
