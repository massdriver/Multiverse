using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    [RequireComponent(typeof(UNetworkIdentity))]
    public abstract class UNetworkBehaviour : MonoBehaviour
    {
        public UNetworkIdentity identity { get { return m_identity; } }
        public ulong netId { get { return identity.netId; } }
        public bool isClient { get { return UNetworkManager.singleton.isClient; } }
        public bool isServer { get { return UNetworkManager.singleton.isServer; } }
        public bool isPureClient { get { return UNetworkManager.singleton.isPureClient; } }
        public bool hasAuthority { get { return identity.hasAuthority; } }

        public float syncTimeDelta = 0;

        private float syncTimer = 0;

        internal byte componentId { get; set; }

        private NetworkMessageHandler handlers;
        private UNetworkIdentity m_identity;

        protected virtual void Awake()
        {
            handlers = new NetworkMessageHandler();
            m_identity = GetComponent<UNetworkIdentity>();
        }

        public void SetMessageHandler<T>(NetworkMessageHandler.MessageHandler handler) where T : Message
        {
            UNetworkManager.singleton.RegisterMessage<T>();
            handlers.SetHandler<T>(handler);
        }

        public virtual void Serialize(NetBuffer msg, bool initialState)
        {

        }

        public virtual void Deserialize(NetBuffer msg, bool initialState)
        {

        }

        public virtual void OnSpawn()
        {

        }

        public virtual void OnUnspawn()
        {

        }

        public void SyncState()
        {
            if (!hasAuthority)
                return;

            // Send update to all clients
            if (isServer)
            {
                UMsgSyncState msg = new UMsgSyncState(this);

                if (isClient)
                    UNetworkManager.singleton.SendMessageToAllClientsExceptLocal(msg);
                else
                    UNetworkManager.singleton.SendMessageToAllClients(msg);
            }

            if (isClient && !isServer)
            {
                UMsgSyncState msg = new UMsgSyncState(this);
                UNetworkManager.singleton.SendMessageToServer(msg);
            }
        }

        protected virtual void Update()
        {
            if(syncTimeDelta > 0)
            {
                syncTimer -= Time.deltaTime;

                if (syncTimer < 0)
                {
                    SyncState();
                    syncTimer = syncTimeDelta;
                }
            }
        }

        internal void HandleMessage(Message m)
        {
            handlers.HandleMessage(m);
        }

        public void SendCommand(Message scriptMessage)
        {
            if (!isClient)
                throw new Exception("Cannot send Command message because you are not client");

            UNetworkManager.singleton.SendMessageToServer(new UMsgScriptMessage(this, scriptMessage));
        }

        public void SendClientRpc(Message scriptMessage)
        {
            if (!isServer)
                throw new Exception("Cannot send ClientRpc message because you are not server");

            UMsgScriptMessage msg = new UMsgScriptMessage(this, scriptMessage);
            UNetworkManager.singleton.SendMessageToAllClients(msg);
        }

        public void SendClientRpcExceptLocal(Message scriptMessage)
        {
            if (!isServer)
                throw new Exception("Cannot send ClientRpc message because you are not server");

            UMsgScriptMessage msg = new UMsgScriptMessage(this, scriptMessage);
            UNetworkManager.singleton.SendMessageToAllClientsExceptLocal(msg);
        }

        public void SendClientRpc(Message scriptMessage, ushort targetClient)
        {
            if (!isServer)
                throw new Exception("Cannot send ClientRpc message because you are not server");

            UMsgScriptMessage msg = new UMsgScriptMessage(this, scriptMessage);
            UNetworkManager.singleton.SendMessageToAllClients(msg);
        }
    }
}
