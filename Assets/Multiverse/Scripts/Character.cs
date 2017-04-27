using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Multiverse
{
    public class Character : INetBufferSerializable
    {
        [LiteDB.BsonId]
        public long id;
        public string nickname;
        public string zone;

        public void Deserialize(NetBuffer msg)
        {
            throw new NotImplementedException();
        }

        public void Serialize(NetBuffer msg)
        {
            throw new NotImplementedException();
        }
    }
}
