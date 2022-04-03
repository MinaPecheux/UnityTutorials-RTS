using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class DataHandler : MonoBehaviour
{
    public void Start()
    {
        DeserializeGameData();
    }

    public static void LoadGameData()
    {
        // load building data
        Globals.BUILDING_DATA = Resources.LoadAll<BuildingData>("ScriptableObjects/Units/Buildings") as BuildingData[];
        CharacterData[] characterData = Resources.LoadAll<CharacterData>("ScriptableObjects/Units/Characters") as CharacterData[];
        foreach (CharacterData d in characterData)
            Globals.CHARACTER_DATA[d.code] = d;

        // load game parameters
        string gameUid = CoreDataHandler.instance.GameUID;

        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        foreach (GameParameters parameters in gameParametersList)
        {
            if (parameters is GamePlayersParameters pp)
                pp.LoadFromFile($"Games/{gameUid}/PlayerParameters");
            else
                parameters.LoadFromFile();
        }

        // prepare technology tree
        TechnologyNodeData.LoadTechnologyTree();

        // load game scene data
        GameData.gameUid = gameUid;
        GameData.Load();
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
        // save game scene current minimap
        Camera minimapCamera = GameObject.Find("CameraMinimap").GetComponent<Camera>();
        MinimapManager.IS_ENABLED = false;
        MinimapCapture.TakeScreenshot(
            "minimap",
            new Vector2Int(512, 512),
            minimapCamera,
            GameData.GetFolderPath());
        MinimapManager.IS_ENABLED = true;
    }

    public static void DeserializeGameData()
    {
        GameData data = GameData.Instance;
        if (data == null)
        {
            Object.FindObjectOfType<CameraManager>().InitializeBounds();
            EventManager.TriggerEvent("LoadedScene");
            return;
        }

        TechnologyNodeData.SetUnlockedNodes(data.unlockedTechnologyNodeCodes);

        GamePlayersParameters playerParameters = GameManager.instance.gamePlayersParameters;
        for (int p = 0; p < playerParameters.players.Length; p++)
        {
            GamePlayerData d = data.players[p];

            // restore resources
            for (int i = 0; i < d.resources.Length; i++)
            {
                InGameResource r = Globals.GAME_RESOURCE_KEYS[i];
                Globals.GAME_RESOURCES[p][r].Amount = d.resources[i];
            }

            // restore units
            foreach (GamePlayerUnitData unit in d.units)
            {
                Unit u;
                string type = unit.unitType;
                List<ResourceValue> production = new List<ResourceValue>();
                for (int i = 0; i < unit.production.Length; i++)
                {
                    InGameResource r = Globals.GAME_RESOURCE_KEYS[i];
                    production.Add(new ResourceValue(r, unit.production[i]));
                }
                if (type == "building")
                {
                    BuildingData bd = Globals.BUILDING_DATA
                        .Where((BuildingData x) => x.code == unit.code)
                        .First();
                    u = new Building(bd, p, production);
                    u.SetPosition(unit.position);
                    u.Place(true);
                    ((Building)u).SetConstructionHP(unit.constructionHP, true);
                }
                else
                {
                    CharacterData cd = Globals.CHARACTER_DATA[unit.code];
                    u = new Character(cd, p);
                    u.ComputeProduction();
                    u.Transform.GetComponent<NavMeshAgent>().Warp(unit.position);
                }

                u.Uid = unit.uid;
                u.HP = unit.currentHealth;
                u.Level = unit.level - 1;
                u.LevelUp(true);
                u.AttackDamage = unit.attackDamage;
                u.AttackRange = unit.attackRange;
            }
        }

        Camera.main.transform.position = data.camPosition;
        Object.FindObjectOfType<CameraManager>().InitializeBounds();
        EventManager.TriggerEvent("UpdatedResources");
        EventManager.TriggerEvent("LoadedScene");
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
                if (!unit.Transform) continue;

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
        data.unlockedTechnologyNodeCodes = TechnologyNodeData.GetUnlockedNodeCodes();
        return data;
    }

    public static List<(string, System.DateTime)> GetGamesList()
    {
        string rootPath = Path.Combine(
            Application.persistentDataPath,
            BinarySerializable.DATA_DIRECTORY,
            "Games");
        if (!Directory.Exists(rootPath))
            return null;
        string[] gameDirs = Directory.GetDirectories(rootPath);

        // filter to keep only game folders with a game save
        IEnumerable<string> validGameDirs = gameDirs
            .Where((string d)
                => File.Exists(Path.Combine(d, GameData.DATA_FILE_NAME)));

        // extract the last modification time
        List<(string, System.DateTime)> games = new List<(string, System.DateTime)>();
        foreach (string dir in validGameDirs)
        {
            games.Add((
                dir,
                File.GetLastWriteTime(Path.Combine(dir, GameData.DATA_FILE_NAME))
            ));
        }

        // sort by reverse chronological order (most recent first)
        return games
            .OrderByDescending(((string, System.DateTime) x) => x.Item2)
            .ToList();
    }
}
