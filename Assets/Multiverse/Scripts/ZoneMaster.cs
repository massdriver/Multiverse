using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    public sealed class ZoneWorldInfo
    {
        public string sceneName { get; private set; }
        public string ip { get; private set; }
        public int port { get; private set; }

        public override int GetHashCode()
        {
            return sceneName.GetHashCode();
        }
    }

    public class ZoneMaster : MonoBehaviour
    {
        private HashSet<ZoneWorldInfo> zones;
        private LidgrenServer server;
        private NetworkMessageHandler handler;

        
    }
}
