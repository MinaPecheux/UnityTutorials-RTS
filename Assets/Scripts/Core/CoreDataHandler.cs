using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreDataHandler : MonoBehaviour
{
    public static CoreDataHandler instance;

    private string _gameUID;
    private MapData _mapData;

    public string GameUID => _gameUID;
    public string Scene => _mapData != null ? _mapData.sceneName : null;
    public float MapSize => _mapData.mapSize;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void SetGameUID(MapData d)
    {
        _gameUID = $"{d.sceneName}__{System.Guid.NewGuid().ToString()}";
    }
    public void SetGameUID(string uid)
    {
        _gameUID = uid;
    }

    public void SetMapData(MapData d)
    {
        _mapData = d;
    }
}
