using System.Collections.Generic;
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

        // save game scene data
        GameData.gameUid = CoreDataHandler.instance.GameUID;
        GameData.Save(SerializeGameData());
    }

    public static GameData SerializeGameData()
    {
        List<GamePlayerData> players = new List<GamePlayerData>();
        GamePlayersParameters playerParameters = GameManager.instance.gamePlayersParameters;
        for (int p = 0; p < playerParameters.players.Length; p++)
        {
            GamePlayerData d = new GamePlayerData();

            // get resources
            int[] resources = new int[Globals.GAME_RESOURCE_KEYS.Length];
            for (int i = 0; i < Globals.GAME_RESOURCE_KEYS.Length; i++)
            {
                InGameResource r = Globals.GAME_RESOURCE_KEYS[i];
                resources[i] = Globals.GAME_RESOURCES[p][r].Amount;
            }
            d.resources = resources;

            // get units
            List<GamePlayerUnitData> units = new List<GamePlayerUnitData>();
            foreach (Unit unit in Unit.UNITS_BY_OWNER[p])
            {
                int[] production = new int[Globals.GAME_RESOURCE_KEYS.Length];
                for (int i = 0; i < Globals.GAME_RESOURCE_KEYS.Length; i++)
                {
                    InGameResource r = Globals.GAME_RESOURCE_KEYS[i];
                    production[i] = unit.Production.ContainsKey(r)
                        ? unit.Production[r]
                        : 0;
                }
                GamePlayerUnitData u = new GamePlayerUnitData()
                {
                    unitType = (unit is Building) ? "building" : "character",
                    uid = unit.Uid,
                    code = unit.Code,
                    position = unit.Transform.position,
                    currentHealth = unit.HP,
                    level = unit.Level,
                    production = production,
                    attackDamage = unit.AttackDamage,
                    attackRange = unit.AttackRange,
                };
                if (u.unitType == "building")
                    u.constructionHP = ((Building)unit).ConstructionHP;
                units.Add(u);
            }
            d.units = units.ToArray();

            players.Add(d);
        }

        GameData data = new GameData();
        data.players = players.ToArray();
        data.camPosition = Camera.main.transform.position;
        return data;
    }
}
