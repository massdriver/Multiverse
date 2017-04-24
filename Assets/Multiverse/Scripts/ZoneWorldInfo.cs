using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse
{
    public sealed class ZoneWorldInfo
    {
        public ulong worldZoneId { get; private set; }
        public int sceneBuildIndex { get; private set; }
        public string ip { get; private set; }
        public int port { get; private set; }

        public ushort zoneClientId { get; private set; }

        public ZoneWorldInfo(ulong worldZoneId, int sceneBuildIndex, string ip, int port, ushort zoneClientId)
        {
            this.worldZoneId = worldZoneId;
            this.sceneBuildIndex = sceneBuildIndex;
            this.ip = ip;
            this.port = port;
            this.zoneClientId = zoneClientId;
        }
    }
}
