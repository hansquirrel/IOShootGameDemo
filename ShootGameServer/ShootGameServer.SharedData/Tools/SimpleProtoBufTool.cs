using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ShootGameServer.SharedData
{
    public interface ByteArrayAble<T> where T : ByteArrayAble<T>
    {
        T FromBytes(byte[] data, int offset, int size);
        byte[] ToBytes();
    }

    public abstract class AbstractBAA<T> : ByteArrayAble<T> where T : ByteArrayAble<T>
    {
        public T FromBytes(byte[] data, int offset, int size)
        {
            return DirectProtoBufTools.Deserialize<T>(data, offset, size);
        }
        public byte[] ToBytes()
        {
            return DirectProtoBufTools.Serialize(this);
        }
    }

    public static class DirectProtoBufTools
    {
        public static byte[] Serialize(object obj)
        {
            using (var memory = new MemoryStream())
            {
                Serializer.Serialize(memory, obj);
                return memory.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] data)
        {
            using (var memory = new MemoryStream(data))
                return Serializer.Deserialize<T>(memory);
        }

        public static T Deserialize<T>(byte[] data, int offset, int size)
        {
            using (var memory = new MemoryStream(data, offset, size))
                return Serializer.Deserialize<T>(memory);
        }

#if HSFRAMEWORK_NET_ABOVE_4_5
        public static T Deserialize<T>(SmartBuffer sb)
        {
        return Deserialize<T>(sb.Data, sb.Offset, sb.Size);
        }
#endif
    }
}