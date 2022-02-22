using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class GamePlayerUnitData : BinarySerializable
{
    public string unitType;
    public string code;
    public string uid;
    public Vector3 position;
    public int currentHealth;
    public int level;
    public int[] production; // sorted by GAME_RESOURCES_KEYS
    public int attackDamage;
    public float attackRange;

    // building-specific
    public int constructionHP;

    public GamePlayerUnitData() { }

    protected GamePlayerUnitData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }
}
[System.Serializable]
public class GamePlayerData
{
    public GamePlayerUnitData[] units;
    public int[] resources; // sorted by GAME_RESOURCES_KEYS
}

[System.Serializable]
public class GameData : BinarySerializable
{
    public static string gameUid;
    private static GameData _instance;
    public static GameData Instance => _instance;

    public static string DATA_FILE_NAME = "GameData.data";

    public GamePlayerData[] players;
    public Vector3 camPosition;
    public string[] unlockedTechnologyNodeCodes;

    public static string GetFolderPath()
        => System.IO.Path.Combine(
            Application.persistentDataPath,
            DATA_DIRECTORY,
            "Games",
            gameUid);

    private static string _GetFilePath()
        => System.IO.Path.Combine(GetFolderPath(), DATA_FILE_NAME);

    public GameData() { }

    protected GameData(SerializationInfo info, StreamingContext context)
    {
        BinarySerializable.Deserialize(this, info, context);
    }

    public static GameData Load()
    {
        _instance = (GameData) BinarySerializable.Load(_GetFilePath());
        return _instance;
    }

    public static void Save(GameData instance)
    {
        BinarySerializable.Save(_GetFilePath(), instance);
    }

}
