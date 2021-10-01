using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Reflection;

[Serializable]
public class BinarySerializable : ISerializable
{
    public BinarySerializable() {}

    protected BinarySerializable(SerializationInfo info, StreamingContext context)
    {
        Type T = GetType();
        foreach (FieldInfo field in T.GetFields())
        {
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
                    field.SetValue(this, value);
                else
                    UnityEngine.Debug.LogWarning($"Could not serialize field: {field.Name} - getting default values");
            }
            else
            {
                object deserializedValue = info.GetValue(field.Name, field.FieldType);
                object value;
                if (BinarySerializableData.Deserialize(field, deserializedValue, out value))
                    field.SetValue(this, value);
                else
                    UnityEngine.Debug.LogWarning($"Could not serialize field: {field.Name} - getting default values");
            }
        }
    }

    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        Type T = GetType();
        foreach (FieldInfo field in T.GetFields())
        {
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
                UnityEngine.Debug.LogWarning($"Could not serialize field: {field.Name} - ignoring");
            }
        }
    }
}
