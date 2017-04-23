using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    public interface INetBufferSerializable
    {
        void Serialize(NetBuffer msg);
        void Deserialize(NetBuffer msg);
    }

    public static class NetSerialize
    {
        public static void WritePackedVectors(NetBuffer buffer, ushort[] objectState)
        {
            if (objectState != null && objectState.Length > 0)
            {
                buffer.Write(objectState.Length);

                foreach (ushort f in objectState)
                    buffer.Write(f);
            }
            else
            {
                buffer.Write((int)0);
            }
        }

        public static ushort[] ReadPackedVectors(NetBuffer buffer)
        {
            ushort[] data = null;

            int len = buffer.ReadInt32();

            if (len > 0)
            {
                data = new ushort[len];

                for (int i = 0; i < len; i++)
                {
                    data[i] = buffer.ReadUInt16();
                }
            }

            return data;
        }

        public static void WriteVectorArray(NetBuffer buffer, Vector3[] objectState)
        {
            if (objectState != null && objectState.Length > 0)
            {
                buffer.Write(objectState.Length);

                foreach (Vector3 f in objectState)
                    NetSerialize.Write(buffer, f);
            }
            else
            {
                buffer.Write((int)0);
            }
        }

        public static Vector3[] ReadVectorArray(NetBuffer buffer)
        {
            Vector3[] data = null;

            int len = buffer.ReadInt32();

            if (len > 0)
            {
                data = new Vector3[len];

                for (int i = 0; i < len; i++)
                {
                    data[i] = NetSerialize.ReadVector3(buffer);
                }
            }

            return data;
        }

        public static void WriteFloatArray(NetBuffer buffer, float[] objectState)
        {
            if (objectState != null && objectState.Length > 0)
            {
                buffer.Write(objectState.Length);

                foreach(float f in objectState)
                    buffer.Write(f);
            }
            else
            {
                buffer.Write((int)0);
            }
        }

        public static float[] ReadFloatArray(NetBuffer buffer)
        {
            float[] data = null;

            int len = buffer.ReadInt32();

            if (len > 0)
            {
                data = new float[len];

                for(int i = 0; i < len; i++)
                {
                    data[i] = buffer.ReadSingle();
                }
            }

            return data;
        }

         

        public static void Write(NetBuffer buffer, byte[] objectState)
        {
            if (objectState != null && objectState.Length > 0)
            {
                buffer.Write(objectState.Length);
                buffer.Write(objectState);
            }
            else
            {
                buffer.Write((int)0);
            }
        }

        public static byte[] ReadBytes(NetBuffer buffer)
        {
            byte[] data = null;
            int len = buffer.ReadInt32();

            if (len > 0)
                data = buffer.ReadBytes(len);

            return data;
        }

        public static Vector3 ReadVector3(NetBuffer buffer)
        {
            Vector3 v;
            v.x = buffer.ReadSingle();
            v.y = buffer.ReadSingle();
            v.z = buffer.ReadSingle();
            return v;
        }

        public static void Write(NetBuffer buffer, Vector3 v)
        {
            buffer.Write(v.x);
            buffer.Write(v.y);
            buffer.Write(v.z);
        }

        public static Quaternion ReadQuaternion(NetBuffer buffer)
        {
            Quaternion v;
            v.x = buffer.ReadSingle();
            v.y = buffer.ReadSingle();
            v.z = buffer.ReadSingle();
            v.w = buffer.ReadSingle();
            return v;
        }

        public static void Write(NetBuffer buffer, Quaternion v)
        {
            buffer.Write(v.x);
            buffer.Write(v.y);
            buffer.Write(v.z);
            buffer.Write(v.w);
        }
    }
}
