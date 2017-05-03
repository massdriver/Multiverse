using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Multiverse
{
    internal sealed class UMsgSpawnObject : Message
    {
        public ulong netId;
        public ulong sceneId;
        public ulong assetId;

        public byte[] objectState { get; set; }

        public UMsgSpawnObject()
        {

        }

        public UMsgSpawnObject(UNetworkIdentity identity)
        {
            netId = identity.netId;
            sceneId = identity.sceneId;
            assetId = identity.assetId;
            this.objectState = identity.ToBytes(true);
        }

        public override void Read(NetBuffer msg)
        {
            netId = msg.ReadUInt64();
            sceneId = msg.ReadUInt64();
            assetId = msg.ReadUInt64();
            objectState = NetSerialize.ReadBytes(msg);
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(netId);
            msg.Write(sceneId);
            msg.Write(assetId);
            NetSerialize.Write(msg, objectState);
        }
    }

    internal sealed class UMsgUnspawnObject : Message
    {
        public ulong netId { get; set; }

        public UMsgUnspawnObject()
        {

        }

        public UMsgUnspawnObject(ulong netId)
        {
            this.netId = netId;
        }

        public override void Read(NetBuffer msg)
        {
            netId = msg.ReadUInt64();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(netId);
        }
    }

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
