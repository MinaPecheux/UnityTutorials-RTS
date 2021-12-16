using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class MapMetadataExtractor
{
    private static readonly string _mapDataFolder = "Resources/ScriptableObjects/Maps";

    public static void Extract(Scene s)
    {
        // check the map metadata Scriptable Objects folder exists,
        // or create it
        if (!System.IO.Directory.Exists(Application.dataPath + $"/{_mapDataFolder}"))
            System.IO.Directory.CreateDirectory(Application.dataPath + $"/{_mapDataFolder}");

        // get the scene name + set it as active
        string sceneName = s.name;
        EditorSceneManager.SetActiveScene(s);

        // try to get the "Terrain" object
        GameObject[] objs = s.GetRootGameObjects();
        Terrain terrain = null;
        foreach (GameObject g in objs)
        {
            terrain = g.GetComponent<Terrain>();
            if (terrain != null)
                break;
        }
        if (terrain == null)
        {
            Debug.LogWarning("There is no 'Terrain' component in this scene!");
            return;
        }

        // try and get the map metadata Scriptable Object,
        // or create and save it
        string assetPath = $"Assets/{_mapDataFolder}/{sceneName}.asset";
        MapData data = AssetDatabase.LoadAssetAtPath<MapData>(assetPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(data, assetPath);
            data.mapName = sceneName;
            data.sceneName = sceneName;
        }

        // get the map size
        Bounds bounds = terrain.terrainData.bounds;
        data.mapSize = bounds.size.x;

        // get the max number of players = number of spawnpoints
        data.maxPlayers = GameObject.Find("Spawnpoints").transform.childCount;

        // update the Scriptable Object
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }
}
