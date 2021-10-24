/* Adapted from this video tutorial from Game Dev Guide:
 * https://www.youtube.com/watch?v=VzOEM-4A2OM */
using System;
using System.Collections.Generic;

public class DebugCommandBase
{
    public static Dictionary<string, DebugCommandBase> DebugCommands;

    private string _id;
    private string _description;
    private string _format;

    public DebugCommandBase(string id, string description, string format)
    {
        _id = id;
        _description = description;
        _format = format;

        if (DebugCommands == null)
            DebugCommands = new Dictionary<string, DebugCommandBase>();
        string mainKeyword = format.Split(' ')[0];
        DebugCommands[mainKeyword] = this;
    }

    public string Id => _id;
    public string Description => _description;
    public string Format => _format;

}

public class DebugCommand : DebugCommandBase
{
    private Action _action;

    public DebugCommand(string id, string description, string format, Action action)
        : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke()
    {
        _action.Invoke();
    }
}

public class DebugCommand<T> : DebugCommandBase
{
    private Action<T> _action;

    public DebugCommand(string id, string description, string format, Action<T> action)
        : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke(T value)
    {
        _action.Invoke(value);
    }
}

public class DebugCommand<T1, T2> : DebugCommandBase
{
    private Action<T1, T2> _action;

    public DebugCommand(string id, string description, string format, Action<T1, T2> action)
        : base(id, description, format)
    {
        _action = action;
    }

    public void Invoke(T1 v1, T2 v2)
    {
        _action.Invoke(v1, v2);
    }
}
