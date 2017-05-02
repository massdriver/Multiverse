using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Multiverse
{
    internal sealed class UMsgLoadTargetScene : Message
    {
        public string networkSceneName { get; set; }

        public UMsgLoadTargetScene()
        {

        }

        public UMsgLoadTargetScene(string networkSceneName)
        {
            this.networkSceneName = networkSceneName;
        }

        public override void Read(NetBuffer msg)
        {
            networkSceneName = msg.ReadString();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(networkSceneName);
        }
    }

}
