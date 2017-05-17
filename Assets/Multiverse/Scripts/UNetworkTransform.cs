using UnityEngine;
using Lidgren.Network;

namespace Multiverse
{
    [DisallowMultipleComponent]
    public class UNetworkTransform : UNetworkBehaviour
    {
        public bool interpolateRigidbody;
        public float maxPositionError = 0;

        private Rigidbody rigid;

        override protected void Awake()
        {
            base.Awake();

            rigid = GetComponent<Rigidbody>();

            movePos = transform.position;
            moveRot = transform.rotation;
        }

        public override void Serialize(NetBuffer msg, bool initialState)
        {
            NetSerialize.Write(msg, transform.position);
            NetSerialize.Write(msg, transform.rotation);
        }

        private Vector3 movePos;
        private Quaternion moveRot;

        public override void Deserialize(NetBuffer msg, bool initialState)
        {
            movePos = NetSerialize.ReadVector3(msg);
            moveRot = NetSerialize.ReadQuaternion(msg);
        }

        public static Vector3 ComputeOmegaVelocity(Quaternion from, Quaternion to, float dt)
        {
            Quaternion conj = new Quaternion(-from.x, -from.y, -from.z, from.w);
            Quaternion dq = new Quaternion((to.x - from.x) * 2.0f, 2.0f * (to.y - from.y), 2.0f * (to.z - from.z), 2.0f * (to.w - from.w));
            Quaternion c = dq * conj;
            return new Vector3(c.x / dt, c.y / dt, c.z / dt);
        }

        protected override void Update()
        {
            base.Update();

            if (!hasAuthority)
            {
                if (rigid != null)
                {
                    if (interpolateRigidbody)
                    {
                        rigid.velocity = (movePos - rigid.position);///Time.deltaTime;
                        //rigid.MovePosition(movePos);
                        rigid.MoveRotation(moveRot);
                        //rigid.angularVelocity = ComputeOmegaVelocity(rigid.rotation, moveRot, Time.deltaTime);

                        if(maxPositionError > 0 && (rigid.position - movePos).sqrMagnitude > maxPositionError* maxPositionError)
                        {
                            rigid.position = movePos;
                        }
                    }
                    else
                    {
                        rigid.position = movePos;
                        rigid.rotation = moveRot;
                    }
                }
                else
                {
                    transform.position = movePos;
                    transform.rotation = moveRot;
                }
            }

        }
    }

}
