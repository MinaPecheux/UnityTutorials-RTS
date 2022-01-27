using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using UnityEngine;

[Serializable]
public class BinarySerializable : ISerializable
{
#if UNITY_EDITOR
    public static string DATA_DIRECTORY = "Data_Dev";
#else
    public static string DATA_DIRECTORY = "Data";
#endif

    public BinarySerializable() { }

    protected static void Deserialize(
        BinarySerializable instance,
        SerializationInfo info,
        StreamingContext context)
    {
        Type T = instance.GetType();
        foreach (FieldInfo field in T.GetFields())
        {
            if (field.IsStatic) continue;

            Type serializedType = BinarySerializableData.GetSerializedType(field);
            if (serializedType.IsArray)
            {
                Type serializedElementType = serializedType.GetElementType();
                int nItems = info.GetInt32($"{field.Name}__count");
                Array deserializedValue = Array.CreateInstance(serializedElementType, nItems);
                for (int i = 0; i < nItems; i++)
                    deserializedValue.SetValue(info.GetValue($"{field.Name}__{i}", serializedElementType), i);

                object value;
                if (BinarySerializableData.Deserialize(field, deserializedValue, out value))
                    field.SetValue(instance, value);
                else
                    Debug.LogWarning($"Could not serialize field: {field.Name} - getting default values");
            }
            else
            {
                object deserializedValue = info.GetValue(field.Name, field.FieldType);
                object value;
                if (BinarySerializableData.Deserialize(field, deserializedValue, out value))
                    field.SetValue(instance, value);
                else
                    Debug.LogWarning($"Could not serialize field: {field.Name} - getting default values");
            }
        }
    }

    protected BinarySerializable(SerializationInfo info, StreamingContext context)
    {
        Deserialize(this, info, context);
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        Type T = GetType();
        foreach (FieldInfo field in T.GetFields())
        {
            if (field.IsStatic) continue;

            object value;
            if (BinarySerializableData.Serialize(field, this, out value))
            {
                if (value.GetType().IsArray)
                {
                    IEnumerator enm = ((IEnumerable)value).GetEnumerator();
                    // write the items and get the count at the same time
                    // (since SerializationInfo uses key-value pairs)
                    int i = 0;
                    while (enm.MoveNext())
                        info.AddValue($"{field.Name}__{i++}", enm.Current);
                    info.AddValue($"{field.Name}__count", i);
                }
                else
                {
                    info.AddValue(field.Name, value);
                }
            }
            else
            {
                Debug.LogWarning($"Could not serialize field: {field.Name} - ignoring");
            }
        }
    }

    public static BinarySerializable Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"File \"{filePath}\" not found! Getting default values.");
            return null;
        }

        IFormatter formatter = new BinaryFormatter();
        FileStream s = new FileStream(filePath, FileMode.Open);
        BinarySerializable d = (BinarySerializable)formatter.Deserialize(s);
        s.Close();
        return d;
    }

    public static void Save(string filePath, BinarySerializable instance)
    {
        IFormatter formatter = new BinaryFormatter();
        FileStream s = new FileStream(filePath, FileMode.Create);
        formatter.Serialize(s, instance);
        s.Close();
    }
}
