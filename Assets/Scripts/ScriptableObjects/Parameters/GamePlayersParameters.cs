using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class PlayerData : BinarySerializable
{
    public string name;
    public Color color;

    protected PlayerData(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[CreateAssetMenu(fileName = "Players Parameters", menuName = "Scriptable Objects/Game Players Parameters", order = 12)]
public class GamePlayersParameters : GameParameters
{
    public override string GetParametersName() => "Players";

    public PlayerData[] players;
    public int myPlayerId;
}
