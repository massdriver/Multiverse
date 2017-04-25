using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    public class ZoneClient : MonoBehaviour, LidgrenClient.IDelegate
    {
        public ulong worldZoneId { get; set; }
        public string zoneMasterIp { get; set; }
        //public ushort zoneMasterPort { get; set; }
        public bool isRegistered { get; private set; }

        private LidgrenClient client;
        private NetworkMessageHandler handler;

        private void Awake()
        {
            handler = new NetworkMessageHandler();

            client = new LidgrenClient();
            client.clientDelegate = this;

            RegisterMessage<ZmRegisterWorldReply>(HandleZmRegisterWorldReply);
        }

        private void RegisterMessage<T>(NetworkMessageHandler.MessageHandler msgHandler) where T : Message
        {
            client.RegisterMessageType<T>();
            handler.SetHandler<T>(msgHandler);
        }

        private void Update()
        {
            if (client != null)
                client.Update();
        }

        private void HandleZmRegisterWorldReply(Message m)
        {
            ZmRegisterWorldReply msg = m as ZmRegisterWorldReply;
            isRegistered = msg.success;
            OnZoneClientRegistered(isRegistered);
        }

        public void StartZoneClient(bool connectToLocalHost)
        {
            string finalIp = connectToLocalHost ? "127.0.0.1" : zoneMasterIp;
            client.Connect(finalIp, ZoneMaster.ZoneMasterPort, ZoneMaster.ZoneSessionName);
            OnZoneClientStart();
        }

        public void OnClientConnected(LidgrenClient client)
        {
            OnZoneClientConnect();
            ZcRegisterWorld msg = new ZcRegisterWorld(worldZoneId, UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex, GetMachineStaticIP(), client.GetConnection().Peer.Port);
            client.Send(msg, Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
        }

        public virtual string GetMachineStaticIP()
        {
            return "127.0.0.1";
        }

        public void OnClientDisconnected(LidgrenClient client)
        {
 
        }

        public void OnClientMessageReceived(LidgrenClient client, Message msg)
        {
            handler.HandleMessage(msg);
        }

        //
        // Overridable events
        //

        public virtual void OnZoneClientStart()
        {

        }

        public virtual void OnZoneClientConnect()
        {

        }

        public virtual void OnZoneClientRegistered(bool success)
        {

        }
    }
}
