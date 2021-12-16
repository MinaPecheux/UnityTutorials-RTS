using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "Scriptable Objects/Map")]
public class MapData : ScriptableObject
{

    public string mapName;
    public float mapSize;
    public int maxPlayers;

    public string sceneName;
}
