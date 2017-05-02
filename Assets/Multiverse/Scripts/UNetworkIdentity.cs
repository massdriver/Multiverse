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
        public string assetNickname;

        [SerializeField]
        private ulong precomputedAssetId;
        public ulong assetId { get { return precomputedAssetId; } }

        public const ulong ServerOwner = ulong.MaxValue;
        public const ulong InvalidNetId = ulong.MaxValue;

        public ulong netId { get; private set; }
        public ulong sceneId { get; set; }
        public ushort ownerId { get; private set; }
        public bool hasAuthority { get { return UNetworkManager.singleton.localOwnerId == ownerId; } }

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

        internal byte[] ToBytes()
        {
            return null;
        }

        internal void FromBytes(byte[] data)
        {

        }

        internal void HandleScriptMessage(Message m, byte component)
        {

        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            precomputedAssetId = HashUtil.FromString64(assetNickname);
        }
#endif
    }
}
