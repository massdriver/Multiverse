using System;
using System.Collections.Generic;
using UnityEngine;

namespace Multiverse
{
    [DisallowMultipleComponent]
    public class UNetworkManager : MonoBehaviour
    {
        public int serverPort = 14678;
        public int maxServerConnections = 32;
        public string sessionName = "unet";
        public string targetServerAddress = "127.0.0.1";
        public GameObject playerPrefab;
        public bool autoCreatePlayer;

        public static UNetworkManager singleton { get; private set; }

        public string networkSceneName { get; private set; }
        public ulong localOwnerId { get; private set; }
        public GameObject localPlayerObject { get; private set; }
        public bool isServer { get; private set; }
        public bool isClient { get; private set; }
        public bool isPureClient { get { return !isServer && isClient; } }

        private Dictionary<ulong, UNetworkIdentity> networkObjects { get; set; }
        private Dictionary<ulong, UNetworkIdentity> sceneInitialObjects { get; set; }
        private Dictionary<ulong, GameObject> playerObjects { get; set; }

        private LidgrenServer serverObject;
        private LidgrenClient clientObject;

        private void Awake()
        {
            if (singleton != null)
                throw new InvalidOperationException("Multiple instances of network manager is not allowed");

            singleton = this;
        }

        public void StartHost()
        {
            
        }

        public void StartServer()
        {

        }

        public void StartClient()
        {

        }

        public void StopManager()
        {

        }
    }
}
