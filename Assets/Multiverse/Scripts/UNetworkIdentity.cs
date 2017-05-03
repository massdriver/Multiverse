using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    [DisallowMultipleComponent]
    public sealed class UNetworkIdentity : MonoBehaviour
    {
        public const ulong ServerOwner = 0;
        public const ulong InvalidNetId = 0;

        public string assetNickname;

        [SerializeField]
        private ulong precomputedAssetId;

        public ulong assetId { get { return precomputedAssetId; } }
        public ulong netId { get; internal set; }
        public ulong ownerId { get; internal set; }
        public bool hasAuthority { get { return UNetworkManager.singleton.networkOwnerId == ownerId; } }

        internal ulong sceneId { get; set; }

        public bool isClient { get { if (UNetworkManager.singleton == null) return false; return UNetworkManager.singleton.isClient; } }
        public bool isServer { get { if (UNetworkManager.singleton == null) return false; return UNetworkManager.singleton.isServer; } }

        private UNetworkBehaviour[] cachedBehaviours;

        private void Awake()
        {
            cachedBehaviours = GetComponents<UNetworkBehaviour>();

            byte i = 0;
            foreach(UNetworkBehaviour b in cachedBehaviours)
            {
                b.componentId = i;
                i++;
            }
        }

        internal void CallEventOnSpawn()
        {
            foreach(UNetworkBehaviour b in cachedBehaviours)
            {
                b.OnSpawn();
            }
        }

        internal void CallEventOnUnspawn()
        {
            foreach (UNetworkBehaviour b in cachedBehaviours.Reverse())
            {
                b.OnUnspawn();
            }
        }

        internal byte[] ToBytes(bool initialState)
        {
            NetBuffer buffer = new NetBuffer();
            Serialize(buffer, initialState);
            return buffer.Data;
        }

        internal void FromBytes(byte[] data, bool initialState)
        {
            NetBuffer buffer = new NetBuffer();
            buffer.Data = data;
            Deserialize(buffer, initialState);
        }

        internal void HandleScriptMessage(Message m, byte component)
        {
            UNetworkBehaviour b = cachedBehaviours[component];

            if (b.enabled)
                b.HandleMessage(m);
        }

        internal void Serialize(NetBuffer msg, bool initialState)
        {
            if(initialState)
            {
                msg.Write(precomputedAssetId);
                msg.Write(sceneId);
                msg.Write(ownerId);
                msg.Write(netId);

                NetSerialize.Write(msg, transform.position);
                NetSerialize.Write(msg, transform.rotation);
            }

            foreach (UNetworkBehaviour comp in cachedBehaviours)
            {
                comp.Serialize(msg, initialState);
            }
        }

        internal void Deserialize(NetBuffer msg, bool initialState)
        {
            if(initialState)
            {
                precomputedAssetId = msg.ReadUInt64();
                sceneId = msg.ReadUInt64();
                ownerId = msg.ReadUInt64();
                netId = msg.ReadUInt64();

                transform.position = NetSerialize.ReadVector3(msg);
                transform.rotation = NetSerialize.ReadQuaternion(msg);
            }

            foreach (UNetworkBehaviour comp in cachedBehaviours)
            {
                comp.Deserialize(msg, initialState);
            }
        }

        internal void HandleBehaviourSyncMessage(UMsgSyncState msg)
        {
            UNetworkBehaviour beh = cachedBehaviours[msg.netComponentId];

            NetBuffer reader = new NetBuffer();
            reader.Data = msg.data;

            beh.Deserialize(reader, false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            precomputedAssetId = HashUtil.FromString64(assetNickname);
        }
#endif
    }
}
