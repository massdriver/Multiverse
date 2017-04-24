using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse
{
    public sealed class ZcRegisterWorld : Message
    {
        public ulong worldZoneId { get; set; }
        public int sceneBuildIndex { get; set; }
        public string ip { get; set; }
        public int port { get; set; }

        public ZcRegisterWorld()
        {

        }

        public ZcRegisterWorld(ulong worldZoneId, int sceneBuildIndex, string ip, int port)
        {
            this.worldZoneId = worldZoneId;
            this.sceneBuildIndex = sceneBuildIndex;
            this.ip = ip;
            this.port = port;
        }

        public override void Read(NetBuffer msg)
        {
            worldZoneId = msg.ReadUInt64();
            sceneBuildIndex = msg.ReadInt32();
            ip = msg.ReadString();
            port = msg.ReadInt32();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(worldZoneId);
            msg.Write(sceneBuildIndex);
            msg.Write(ip);
            msg.Write(port);
        }
    }

    public sealed class ZmRegisterWorldReply : Message
    {
        public bool success { get; set; }

        public ZmRegisterWorldReply()
        {

        }

        public ZmRegisterWorldReply(bool success)
        {
            this.success = success;
        }

        public override void Read(NetBuffer msg)
        {
            success = msg.ReadBoolean();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(success);
        }
    }
}
