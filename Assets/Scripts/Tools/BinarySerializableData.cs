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
        typeof(PlayerData),
        typeof(GamePlayerUnitData),
        typeof(GamePlayerData),
    };

    public Dictionary<string, object> properties;

    public BinarySerializableData(ScriptableObject obj, List<string> fieldsToSerialize, bool serializeAll = false)
    {
        properties = new Dictionary<string, object>();

        Type T = obj.GetType();
        foreach (FieldInfo field in T.GetFields())
        {
            if (!serializeAll && !fieldsToSerialize.Contains(field.Name))
                continue;

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

    public static Type GetSerializedType(FieldInfo field)
    {
        Type T = field.FieldType;
        if (_IsTypeSerializable(T))
            return T;

        object serialized;
        _SerializeValue(T, T.IsValueType ? Activator.CreateInstance(T) : null, out serialized);
        return serialized.GetType();
    }

    private static bool _SerializeValue(Type T, object inValue, out object outValue)
    {
        if (_IsTypeSerializable(T))
        {
            outValue = inValue;
            return true;
        }
        else if (_IsOfType(T, typeof(Color)))
        {
            Color c = (Color)inValue;
            outValue = new float[] { c.r, c.g, c.b, c.a };
            return true;
        }
        else if (_IsOfType(T, typeof(Vector3)))
        {
            Vector3 v = (Vector3)inValue;
            outValue = new float[] { v.x, v.y, v.z };
            return true;
        }

        outValue = null;
        return false;
    }

    public static bool Serialize(FieldInfo field, object obj, out object value)
    {
        return _SerializeValue(field.FieldType, field.GetValue(obj), out value);
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
            float[] c = (float[])data;
            value = new Color(c[0], c[1], c[2], c[3]);
            return true;
        }
        else if (_IsOfType(T, typeof(Vector3)))
        {
            float[] v = (float[])data;
            value = new Vector3(v[0], v[1], v[2]);
            return true;
        }

        value = null;
        return false;
    }

}
