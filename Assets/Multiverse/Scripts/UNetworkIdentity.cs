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
        private ulong m_AssetID;
        public ulong assetId { get { return m_AssetID; } }

        public const ulong ServerOwner = ulong.MaxValue;
        public const ulong InvalidNetId = ulong.MaxValue;

        public ulong netId { get; private set; }
        public ulong ownerId { get; private set; }
        public ulong sceneId { get; set; }

        public bool hasAuthority { get { return UNetworkManager.singleton.localOwnerId == ownerId; } }

        private UNetworkBehaviour[] cachedBehaviours;

        private void Awake()
        {
            cachedBehaviours = GetComponents<UNetworkBehaviour>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_AssetID = HashUtil.FromString64(assetNickname);
        }
#endif
    }
}
