using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiverse
{
    public class LoginClient : MonoBehaviour, LidgrenClient.IDelegate
    {
        public string loginServerIp { get; set; }
        public ushort loginServerPort { get; set; }

        private LidgrenClient client { get; set; }
        private NetworkMessageHandler handler { get; set; }

        public bool isConnected { get; private set; }
        public bool isAuthorized { get; private set; }

        public string login { get; private set; }
        public string passwordHash { get; private set; }

        public ulong sessionId { get; private set; }

        private void Awake()
        {
            isConnected = false;
            isAuthorized = false;

            client = new LidgrenClient();
            client.clientDelegate = this;

            handler = new NetworkMessageHandler();

            RegisterMessage<LsLoginReply>(HandleLsLoginReply);
        }

        private void HandleLsLoginReply(Message m)
        {
            LsLoginReply msg = m as LsLoginReply;

            isAuthorized = msg.authorized;
            sessionId = msg.sessionId;

            OnAuthorized(isAuthorized, sessionId);
        }

        private void RegisterMessage<T>(NetworkMessageHandler.MessageHandler msgHandler) where T : Message
        {
            client.RegisterMessageType<T>();
            handler.SetHandler<T>(msgHandler);
        }

        public void Login(string login, string password)
        {
            client.Connect(loginServerIp, loginServerPort, LoginServer.LoginSession);
        }

        public void OnClientConnected(LidgrenClient client)
        {
            isConnected = true;
            OnConnected();
            client.Send(new LcRequestLogin(login, passwordHash), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
        }

        public void OnClientDisconnected(LidgrenClient client)
        {
            OnDisconnected();

            isConnected = false;
            isAuthorized = false;
        }

        public void OnClientMessageReceived(LidgrenClient client, Message msg)
        {
            handler.HandleMessage(msg);
        }

        //
        // Events
        //
        public virtual void OnConnected()
        {

        }

        public virtual void OnAuthorized(bool success, ulong sessionId)
        {

        }

        public virtual void OnAccountData(Account account)
        {

        }

        public virtual void OnCharacterData(Character character)
        {

        }

        public virtual void OnDisconnected()
        {

        }
    }
}