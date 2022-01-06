using UnityEngine;

public static class DataHandler
{
    public static void LoadGameData()
    {
        // load building data
        Globals.BUILDING_DATA = Resources.LoadAll<BuildingData>("ScriptableObjects/Units/Buildings") as BuildingData[];
        CharacterData[] characterData = Resources.LoadAll<CharacterData>("ScriptableObjects/Units/Characters") as CharacterData[];
        foreach (CharacterData d in characterData)
            Globals.CHARACTER_DATA[d.code] = d;

        // load game parameters
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
        {
            if (parameters is GamePlayersParameters pp)
                pp.LoadFromFile($"Games/{CoreDataHandler.instance.GameUID}/PlayerParameters");
            else
                parameters.LoadFromFile();
        }
    }

    public static void SaveGameData()
    {
        // save game parameters
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
            parameters.SaveToFile();
    }
}
