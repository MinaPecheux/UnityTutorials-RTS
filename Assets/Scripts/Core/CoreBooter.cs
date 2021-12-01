using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreBooter : MonoBehaviour
{
    public static CoreBooter instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        LoadMap("Map1");
    }

    public void LoadMap(string mapReference)
    {
        MapData d = Resources.Load<MapData>($"ScriptableObjects/Maps/{mapReference}");
        CoreDataHandler.instance.SetMapData(d);
        string s = d.sceneName;
        SceneManager.LoadSceneAsync(s, LoadSceneMode.Additive).completed += (_) =>
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(s));
            SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
        };
    }
}
