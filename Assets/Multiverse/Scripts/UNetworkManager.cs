using System;
using System.Collections.Generic;
using UnityEngine;

namespace Multiverse
{
    [DisallowMultipleComponent]
    public class UNetworkManager :
        MonoBehaviour,
        LidgrenServer.IDelegate,
        LidgrenClient.IDelegate
    {
        public int serverPort = 14678;
        public int maxServerConnections = 32;
        public string sessionName = "unet";
        public string targetServerAddress = "127.0.0.1";
        public GameObject playerPrefab;

        public static UNetworkManager singleton { get; private set; }

        public string networkSceneName { get; private set; }
        public ushort localOwnerId { get; private set; }
        public GameObject localPlayerObject { get; private set; }
        public bool isServer { get; private set; }
        public bool isClient { get; private set; }
        public bool isPureClient { get { return !isServer && isClient; } }
        public bool isManagerActive { get { return isServer || isClient; } }

        private Dictionary<ulong, UNetworkIdentity> networkObjects { get; set; }
        private Dictionary<ulong, UNetworkIdentity> sceneInitialObjects { get; set; }
        private Dictionary<ushort, GameObject> playerObjects { get; set; }

        private LidgrenServer serverObject;
        private LidgrenClient clientObject;

        private NetworkMessageHandler serverMessageHandler;
        private NetworkMessageHandler clientMessageHandler;

        private void Awake()
        {
            if (singleton != null)
                throw new InvalidOperationException("Multiple instances of network manager is not allowed");

            singleton = this;

            serverObject = new LidgrenServer();
            serverObject.serverDelegate = this;

            clientObject = new LidgrenClient();
            clientObject.clientDelegate = this;

            serverMessageHandler = new NetworkMessageHandler();
            clientMessageHandler = new NetworkMessageHandler();

            RegisterClientMessageHandler<UMsgLoadTargetScene>(HandleUMsgLoadTargetScene);
        }

        public void RegisterServerMessageHandler<T>(NetworkMessageHandler.MessageHandler handler) where T : Message
        {
            serverObject.RegisterMessageType<T>();
            serverMessageHandler.SetHandler<T>(handler);
        }

        public void RegisterClientMessageHandler<T>(NetworkMessageHandler.MessageHandler handler) where T : Message
        {
            clientObject.RegisterMessageType<T>();
            clientMessageHandler.SetHandler<T>(handler);
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

        private void Update()
        {
            if (clientObject != null)
                clientObject.Update();

            if (serverObject != null)
                serverObject.Update();
        }

        //
        //
        //
        public void Spawn(GameObject obj)
        {

        }

        public void SpawnWithAuthority(GameObject obj, ushort owner)
        {

        }

        public void SpawnWithAuthority(GameObject obj, GameObject playerOwner)
        {

        }

        public void Unspawn(GameObject obj)
        {

        }

        public void SetAuthority(GameObject obj, ushort owner)
        {

        }

        //
        //
        //

        private void SpawnSceneObjects()
        {

        }

        //
        //
        //

        void LidgrenServer.IDelegate.OnServerClientConnected(LidgrenServer server, ushort newClientID)
        {
            
        }

        void LidgrenServer.IDelegate.OnServerClientDisconnected(LidgrenServer server, ushort leavingClientID)
        {
            
        }

        void LidgrenServer.IDelegate.OnServerMessageReceived(LidgrenServer server, ushort sourceClient, Message msg)
        {
            serverMessageHandler.HandleMessage(msg);
        }

        //
        //
        //

        void LidgrenClient.IDelegate.OnClientConnected(LidgrenClient client)
        {
            
        }

        void LidgrenClient.IDelegate.OnClientDisconnected(LidgrenClient client)
        {
            
        }

        void LidgrenClient.IDelegate.OnClientMessageReceived(LidgrenClient client, Message msg)
        {
            clientMessageHandler.HandleMessage(msg);
        }

        private void HandleUMsgLoadTargetScene(Message m)
        {
            UMsgLoadTargetScene msg = m as UMsgLoadTargetScene;
        }
    }
}
