using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Serializable]
public class BinarySerializableData
{
    private static List<Type> _serializableTypes = new List<Type>()
    {
        typeof(int),
        typeof(float),
        typeof(bool),
        typeof(string),
        typeof(InputBinding),
    };

    public Dictionary<string, object> properties;

    public BinarySerializableData(ScriptableObject obj)
    {
        properties = new Dictionary<string, object>();

        Type T = obj.GetType();
        foreach (FieldInfo field in T.GetFields())
        {
            object value;
            if (Serialize(field, obj, out value))
                properties[field.Name] = value;
        }
    }

    private static bool _IsTypeSerializable(Type tested)
    {
        return
            _serializableTypes.Contains(tested) ||
            tested.IsArray && _serializableTypes.Contains(tested.GetElementType());
    }

    private static bool _IsOfType(Type tested, Type reference)
    {
        return
            tested == reference ||
            tested.IsArray && tested.GetElementType() == reference;
    }

    public static bool Serialize(FieldInfo field, object obj, out object value)
    {
        Type T = field.FieldType;
        if (_IsTypeSerializable(T))
        {
            value = field.GetValue(obj);
            return true;
        }
        else if (_IsOfType(T, typeof(Color)))
        {
            Color c = (Color) field.GetValue(obj);
            value = new float[] { c.r, c.g, c.b, c.a };
            return true;
        }

        value = null;
        return false;
    }

    public static bool Deserialize(FieldInfo field, object data, out object value)
    {
        Type T = field.FieldType;
        if (_IsTypeSerializable(T))
        {
            value = data;
            return true;
        }
        else if (_IsOfType(T, typeof(Color)))
        {
            float[] c = (float[]) data;
            value = new Color(c[0], c[1], c[2], c[3]);
            return true;
        }

        value = null;
        return false;
    }

}
