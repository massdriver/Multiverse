using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    [RequireComponent(typeof(UNetworkIdentity))]
    public abstract  class UNetworkBehaviour : MonoBehaviour
    {
        private UNetworkIdentity m_identity;

        public UNetworkIdentity identity
        {
            get
            {
                if (m_identity == null)
                    m_identity = GetComponent<UNetworkIdentity>();

                return m_identity;
            }
        }

        public bool isClient { get { return UNetworkManager.singleton.isClient; } }
        public bool isServer { get { return UNetworkManager.singleton.isServer; } }
        public bool isPureClient { get { return UNetworkManager.singleton.isPureClient; } }
        public bool hasAuthority { get { return identity.hasAuthority; } }

        internal byte componentId { get; set; }

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

    }
}
