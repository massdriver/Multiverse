using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    public sealed class UNetworkIdentity : MonoBehaviour
    {
        public string assetNickname;

        [SerializeField]
        private ulong m_AssetId;

        public ulong assetId { get { return m_AssetId; } }

        public const ulong ServerOwner = ulong.MaxValue;
        public const ulong InvalidNetId = ulong.MaxValue;

        public ulong netId { get; private set; }
        public ulong ownerId { get; private set; }

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_AssetId = HashUtil.FromString64(assetNickname);
        }
#endif
    }
}
