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
    private static string _dataFileName = "GameData.data";

    public GamePlayerData[] players;
    public Vector3 camPosition;

    private static string _GetFilePath()
        => System.IO.Path.Combine(
            Application.persistentDataPath,
            DATA_DIRECTORY,
            "Games",
            gameUid,
            _dataFileName);

    public static void Save(GameData instance)
    {
        BinarySerializable.Save(_GetFilePath(), instance);
    }

}
