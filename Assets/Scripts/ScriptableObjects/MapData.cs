using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "Scriptable Objects/Map")]
public class MapData : ScriptableObject
{

    public string mapName;
    public float mapSize;
    public int maxPlayers;

    public string sceneName;

    public string GetMapSizeType()
    {
        if (mapSize <= 200) return "Small";
        if (mapSize <= 500) return "Medium";
        if (mapSize <= 1200) return "Large";
        return "Immense";
    }
}
