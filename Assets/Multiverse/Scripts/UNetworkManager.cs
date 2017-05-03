﻿using System;
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
        private Dictionary<ulong, ushort> players { get; set; }
        private List<ushort> readyPlayers { get; set; }

        private LidgrenServer serverObject;
        private LidgrenClient clientObject;

        private NetworkMessageHandler serverMessageHandler;
        private NetworkMessageHandler clientMessageHandler;

        private ulong netIdCounter = 1;
        private ulong nextOwnerId = 1;

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
            RegisterClientMessageHandler<UMsgAddPlayer>(ClientHandleUMsgAddPlayer);
            RegisterClientMessageHandler<UMsgRemovePlayer>(ClientHandleUMsgRemovePlayer);

            RegisterSpawnablePrefabs();
        }

        protected virtual void RegisterSpawnablePrefabs()
        {
            if (playerPrefab != null)
                RegisterPrefab(playerPrefab);

            foreach (GameObject obj in spawnablePrefabs)
                RegisterPrefab(obj);
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

            CleanNetworkScene();

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

        public void RegisterPrefab(GameObject obj)
        {
            registeredPrefabs[obj.GetComponent<UNetworkIdentity>().assetId] = obj;
        }

        private void CleanNetworkScene()
        {
            localPlayerObject = null;

            foreach (KeyValuePair<ulong, UNetworkIdentity> kp in networkObjects)
                ForceUnspawnNetworkObject(kp.Value);

            networkObjects.Clear();
            playerObjects.Clear();
        }

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

        private void AddPlayer(ushort client, ulong owner)
        {
            
            players.Add(nextOwnerId++, client);

            serverObject.SendToAll(new UMsgAddPlayer(owner, client), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
        }

        private void RemovePlayer(ushort client, ulong owner)
        {
            throw new NotImplementedException();
        }

        public void SetAuthority(GameObject obj, ushort owner)
        {
            if (!isServer)
                return;

            throw new NotImplementedException();
        }

        //
        //
        //

        private GameObject[] topRootObjects
        {
            get
            {
                return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            }
        }

        private void SpawnSceneObjects()
        {
            if (!isServer)
                return;

            foreach (var obj in topRootObjects)
            {
                UNetworkIdentity uv = obj.GetComponent<UNetworkIdentity>();

                if (uv == null)
                    continue;

                if (uv.sceneId != UNetworkIdentity.InvalidNetId)
                    sceneInitialObjects.Add(uv.sceneId, uv);
            }

            foreach (var kp in sceneInitialObjects)
                Spawn(kp.Value.gameObject);
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
            if (isServer)
                return;

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

        private void ClientHandleUMsgAddPlayer(Message m)
        {
            if (isServer)
                return;

            UMsgAddPlayer msg = m as UMsgAddPlayer;

            players.Add(msg.ownerId, msg.clientId);
        }

        private void ClientHandleUMsgRemovePlayer(Message m)
        {
            if (isServer)
                return;

            UMsgRemovePlayer msg = m as UMsgRemovePlayer;

            players.Remove(msg.owner);
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
