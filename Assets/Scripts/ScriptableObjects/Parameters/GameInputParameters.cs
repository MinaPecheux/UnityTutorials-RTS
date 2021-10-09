using UnityEngine;

[System.Serializable]
public class InputBinding
{
    public string displayName;
    public string key;
    public string inputEvent;
}

[CreateAssetMenu(fileName = "Input Parameters", menuName = "Scriptable Objects/Game Input Parameters", order = 13)]
public class GameInputParameters : GameParameters
{
    public override string GetParametersName() => "Controls";

    public InputBinding[] bindings;

    public void CheckForInput()
    {
        foreach (InputBinding i in bindings)
        {
            if (Input.GetKey(i.key))
            {
                if (i.inputEvent.Contains(":"))
                {
                    string[] e = i.inputEvent.Split(':');
                    EventManager.TriggerEvent("<Input>" + e[0], e[1]);
                }
                else
                {
                    EventManager.TriggerEvent("<Input>" + i.inputEvent);
                }
                break;
            }
        }
    }

    public (int, InputBinding) GetBindingForKey(string key)
    {
        for (int i = 0; i < bindings.Length; i++)
            if (bindings[i].key == key) return (i, bindings[i]);
        return (-1, null);
    }
}
