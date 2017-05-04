using UnityEngine;
using Lidgren.Network;

namespace Multiverse
{
    [DisallowMultipleComponent]
    public class UNetworkTransform : UNetworkBehaviour
    {
        public bool interpolateRigidbody;

        private Rigidbody rigid;

        override protected void Awake()
        {
            base.Awake();

            rigid = GetComponent<Rigidbody>();
        }

        public override void Serialize(NetBuffer msg, bool initialState)
        {
            NetSerialize.Write(msg, transform.position);
            NetSerialize.Write(msg, transform.rotation);
        }

        public override void Deserialize(NetBuffer msg, bool initialState)
        {
            transform.position = NetSerialize.ReadVector3(msg);
            transform.rotation = NetSerialize.ReadQuaternion(msg);
        }
    }
}
