using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Multiverse
{
    internal class MsgTest : Message
    {
        public int i;

        public override void Read(NetBuffer msg)
        {
            i = msg.ReadInt32();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(i);
        }
    }

    public sealed class UNetworkBehaviourTest : UNetworkBehaviour
    {
        public float value;

        private void Start()
        {
            SetMessageHandler<MsgTest>(HandleMsgTest);
        }

        private void HandleMsgTest(Message m)
        {
            MsgTest msg = m as MsgTest;
            Debug.Log("HandleMsgTest=" + msg.i);
        }

        private void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            if(isServer)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    MsgTest m = new MsgTest();
                    m.i = 456;
                    SendClientRpc(m);
                }
            }
        }
    }
}
