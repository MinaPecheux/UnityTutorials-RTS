using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreDataHandler : MonoBehaviour
{
    public static CoreDataHandler instance;

    private MapData _mapData;

    public string Scene => _mapData.sceneName;
    public float MapSize => _mapData.mapSize;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void SetMapData(MapData d)
    {
        _mapData = d;
    }
}
