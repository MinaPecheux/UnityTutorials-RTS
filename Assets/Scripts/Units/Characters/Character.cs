using System.Collections.Generic;

public class Character : Unit
{
    public Character(CharacterData data, int owner) :
        base(data, owner, new List<ResourceValue>() { }) {}
}
