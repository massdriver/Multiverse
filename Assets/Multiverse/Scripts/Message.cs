using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Multiverse
{
    public abstract class Message
    {
        /*
         * This field is automatically serialized
         */
        public int id { get; set; }

        public abstract void Write(NetBuffer msg);
        public abstract void Read(NetBuffer msg);

        /*
         * Source client id is setup only for server message processing
         */
        public ushort sourceClient { get; internal set; }

        public int hashCode
        {
            get
            {
                return GetMessageCode(GetType());
            }
        }

        public static int GetMessageCode(Type type)
        {
            return (int)HashUtil.FromString(type.FullName);
        }
    }

    public abstract class EmptyMessage : Message
    {
        override public void Write(NetBuffer msg) { }
        override public void Read(NetBuffer msg){ }
    }
}
