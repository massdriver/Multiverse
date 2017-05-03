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
        public const ulong ServerOwnerID = ulong.MaxValue;

        public ushort serverPort = 14678;
        public ushort maxServerConnections = 32;
        public float connectionTimeout = 3000;
        public string sessionName = "unet";
        public string targetServerAddress = "127.0.0.1";
        public GameObject playerPrefab;

        public List<GameObject> spawnablePrefabs;

        public static UNetworkManager singleton { get; private set; }

        public string networkSceneName { get; private set; }
        public ulong networkOwnerId { get; private set; }
        public GameObject localPlayerObject { get; private set; }
        public bool isServer { get; private set; }
        public bool isClient { get; private set; }
        public bool isPureClient { get { return !isServer && isClient; } }
        public bool isManagerActive { get { return isServer || isClient; } }

        private Dictionary<ulong, GameObject> registeredPrefabs { get; set; }
        private Dictionary<ulong, UNetworkIdentity> networkObjects { get; set; }
        private Dictionary<ulong, UNetworkIdentity> sceneInitialObjects { get; set; }
        private Dictionary<ushort, GameObject> playerObjects { get; set; }
        private List<ushort> readyPlayers { get; set; }

        private LidgrenServer serverObject;
        private LidgrenClient clientObject;

        private NetworkMessageHandler serverMessageHandler;
        private NetworkMessageHandler clientMessageHandler;

        private void Awake()
        {
            if (singleton != null)
                throw new InvalidOperationException("Multiple instances of network manager is not allowed");

            singleton = this;

            networkObjects = new Dictionary<ulong, UNetworkIdentity>();
            sceneInitialObjects = new Dictionary<ulong, UNetworkIdentity>();
            playerObjects = new Dictionary<ushort, GameObject>();
            readyPlayers = new List<ushort>();
            registeredPrefabs = new Dictionary<ulong, GameObject>();

            serverObject = new LidgrenServer();
            serverObject.serverDelegate = this;

            clientObject = new LidgrenClient();
            clientObject.clientDelegate = this;

            serverMessageHandler = new NetworkMessageHandler();
            clientMessageHandler = new NetworkMessageHandler();

            RegisterClientMessageHandler<UMsgLoadTargetScene>(ClientHandleUMsgLoadTargetScene);
            RegisterClientMessageHandler<UMsgSpawnObject>(ClientHandleUMsgSpawnObject);
            RegisterClientMessageHandler<UMsgUnspawnObject>(ClientHandleUMsgUnspawnObject);
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
            StartServer();
            StartClient();
        }

        public void StartServer()
        {
            isServer = true;
            serverObject.Start(serverPort, maxServerConnections, sessionName, connectionTimeout);

            networkOwnerId = ServerOwnerID;

            SpawnSceneObjects();

            OnStartServer();
        }

        public void StartClient()
        {
            isClient = true;
            clientObject.Connect(targetServerAddress, serverPort, sessionName);

            OnStartClient();
        }

        public void StopManager()
        {
            clientObject.Disconnect();
            serverObject.Stop();

            isClient = false;
            isServer = false;
        }

        private void Update()
        {
            if (clientObject != null)
                clientObject.Update();

            if (serverObject != null)
                serverObject.Update();
        }

        private ulong netIdCounter = 1;

        //
        //
        //
        public void Spawn(GameObject networkGameObject)
        {
            SpawnWithAuthority(networkGameObject, ServerOwnerID);
        }

        public void SpawnWithAuthority(GameObject networkGameObject, ulong owner)
        {
            if (!isServer)
                return;

            UNetworkIdentity iden = networkGameObject.GetComponent<UNetworkIdentity>();

            if (iden.sceneId != UNetworkIdentity.InvalidNetId)
                iden.gameObject.SetActive(true);

            iden.netId = netIdCounter++;
            iden.ownerId = owner;
            networkObjects.Add(iden.netId, iden);
            serverObject.SendToAll(new UMsgSpawnObject(iden), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
            iden.CallEventOnSpawn();
        }

        public void Unspawn(GameObject networkGameObject)
        {
            if (!isServer)
                return;

            UNetworkIdentity iden = networkGameObject.GetComponent<UNetworkIdentity>();

            networkObjects.Remove(iden.netId);
            serverObject.SendToAll(new UMsgUnspawnObject(iden.netId), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);

            ForceUnspawnNetworkObject(iden);
        }

        private void SpawnPlayerObject(ulong ownerId)
        {

        }

        private void UnspawnPlayerObject(ulong owner)
        {

        }

        public void SetAuthority(GameObject obj, ushort owner)
        {
            if (!isServer)
                return;
        }

        //
        //
        //

        private void SpawnSceneObjects()
        {
            if (!isServer)
                return;

            foreach (var kp in sceneInitialObjects)
            {
                Spawn(kp.Value.gameObject);
            }
        }

        private void ForceUnspawnNetworkObject(UNetworkIdentity iden)
        {
            if (!isManagerActive)
                return;

            iden.CallEventOnUnspawn();
            iden.netId = UNetworkIdentity.InvalidNetId;
     
            if (iden.sceneId != UNetworkIdentity.InvalidNetId)
            {
                iden.gameObject.SetActive(false);
            }
            else
            {
                DestroyImmediate(iden.gameObject);
            }
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

        private void ClientHandleUMsgLoadTargetScene(Message m)
        {
            UMsgLoadTargetScene msg = m as UMsgLoadTargetScene;
        }

        private void ClientHandleUMsgSpawnObject(Message m)
        {
            if (isServer)
                return;

            UMsgSpawnObject msg = m as UMsgSpawnObject;

            GameObject newObject = null;

            if (msg.sceneId != UNetworkIdentity.InvalidNetId)
            {
                newObject = sceneInitialObjects[msg.sceneId].gameObject;
                newObject.SetActive(true);
            }
            else
            {
                newObject = GameObject.Instantiate(registeredPrefabs[msg.assetId]);
            }

            UNetworkIdentity iden = newObject.GetComponent<UNetworkIdentity>();
            iden.FromBytes(msg.objectState, true);
            iden.netId = msg.netId;
            networkObjects.Add(iden.netId, iden);
            iden.CallEventOnSpawn();
        }

        private void ClientHandleUMsgUnspawnObject(Message m)
        {
            if (isServer)
                return;

            UMsgUnspawnObject msg = m as UMsgUnspawnObject;
            UNetworkIdentity iden = networkObjects[msg.netId];

            networkObjects.Remove(iden.netId);

            ForceUnspawnNetworkObject(iden);
        }

        //
        //
        //

        public virtual void OnStartServer()
        {

        }

        public virtual void OnStartClient()
        {

        }
    }
}
