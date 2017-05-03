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

        internal byte componentId { get; set; }

        private NetworkMessageHandler handlers;
        private UNetworkIdentity m_identity;

        private void Awake()
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

        internal void HandleScriptMessage(Message m)
        {
            handlers.HandleMessage(m);
        }

    }
}
