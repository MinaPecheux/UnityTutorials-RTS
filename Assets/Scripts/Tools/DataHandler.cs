using UnityEngine;

public static class DataHandler
{
    public static void LoadGameData()
    {
        // load building data
        Globals.BUILDING_DATA = Resources.LoadAll<BuildingData>("ScriptableObjects/Units/Buildings") as BuildingData[];

        // load game parameters
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
            parameters.LoadFromFile();
    }

    public static void SaveGameData()
    {
        // save game parameters
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
            parameters.SaveToFile();
    }
}
