using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    public class UNetworkManager : MonoBehaviour
    {
        public int serverPort = 14678;
        public int maxServerConnections = 32;
        public string sessionName = "unet";

        public string networkSceneName { get; private set; }
        public ulong localOwnerId { get; private set; }
        public GameObject localPlayerObject { get; private set; }
        public bool isServer { get; private set; }
        public bool isClient { get; private set; }
        public bool isPureClient { get { return !isServer && isClient; } }

        private Dictionary<ulong, UNetworkIdentity> networkObjects { get; set; }
        private Dictionary<ulong, UNetworkIdentity> sceneInitialObjects { get; set; }
        private Dictionary<ulong, GameObject> playerObjects { get; set; }

    }
}
