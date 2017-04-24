using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiverse
{
    public sealed class LoginSession
    {
        public ushort clientId { get; private set; }
        public ulong sessionId { get; private set; }

        public Account account { get; set; }
        public Character activeCharacter { get; set; }
    }

    public class LoginServer : MonoBehaviour, LidgrenServer.IDelegate
    {
        public static readonly string LoginSession = "Loginz";

        private ZoneMaster zoneMaster { get; set; }
        private LidgrenServer server { get; set; }
        private NetworkMessageHandler handler { get; set; }

        private void Awake()
        {
            zoneMaster = GetComponent<ZoneMaster>();

            server = new LidgrenServer();
            server.serverDelegate = this;

            RegisterMessage<LcRequestLogin>(HandleLcRequestLogin);
        }

        private void HandleLcRequestLogin(Message m)
        {

        }

        private void RegisterMessage<T>(NetworkMessageHandler.MessageHandler msgHandler) where T : Message
        {
            server.RegisterMessageType<T>();
            handler.SetHandler<T>(msgHandler);
        }

        public void OnServerClientConnected(LidgrenServer server, ushort newClientID)
        {
            OnClientConnected(newClientID);
        }

        public void OnServerClientDisconnected(LidgrenServer server, ushort leavingClientID)
        {
            OnClientDisconnected(leavingClientID);
        }

        public void OnServerMessageReceived(LidgrenServer server, ushort sourceClient, Message msg)
        {
            throw new NotImplementedException();
        }

        //
        // Events
        //

        public virtual void OnClientConnected(ushort newClientID)
        {

        }

        public virtual void OnClientDisconnected(ushort leavingClientID)
        {

        }
    }
}