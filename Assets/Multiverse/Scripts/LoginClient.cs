using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiverse
{
    public class LoginClient : MonoBehaviour, LidgrenClient.IDelegate
    {
        public string loginServerIp;
        public ushort loginServerPort;

        private LidgrenClient client;
        private NetworkMessageHandler handler;

        public bool isConnected { get; private set; }
        public bool isAuthorized { get; private set; }

        public ulong sessionId { get; private set; }
        public bool isWatingForCreateAccountReply { get; private set; }

        public bool isWaitingForAuthorizeReply { get; private set; }

        private void Awake()
        {
            isConnected = false;
            isAuthorized = false;

            client = new LidgrenClient();
            client.clientDelegate = this;

            handler = new NetworkMessageHandler();

            RegisterMessage<LsLoginReply>(HandleLsLoginReply);
            RegisterMessage<LsCreateAccountReply>(HandleLsCreateAccountReply);
        }

        private void HandleLsCreateAccountReply(Message m)
        {
            LsCreateAccountReply msg = m as LsCreateAccountReply;

            isWatingForCreateAccountReply = false;

            OnCreateAccountResult(msg.success);
        }

        private void HandleLsLoginReply(Message m)
        {
            LsLoginReply msg = m as LsLoginReply;

            isAuthorized = msg.authorized;
            sessionId = msg.sessionId;

            isWaitingForAuthorizeReply = false;

            OnAuthorized(isAuthorized, sessionId);
        }

        private void RegisterMessage<T>(NetworkMessageHandler.MessageHandler msgHandler) where T : Message
        {
            client.RegisterMessageType<T>();
            handler.SetHandler<T>(msgHandler);
        }

        public void Login(string login, string password)
        {
            if (isWaitingForAuthorizeReply || isAuthorized || !isConnected)
                return;

            isWaitingForAuthorizeReply = true;

            client.Send(new LcRequestLogin(login, HashUtil.HashPassword(password)), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
        }

        public void Logout()
        {

        }

        public void CreateAccount(string login, string passwordHash, string email, string promotionCode)
        {
            if (isWatingForCreateAccountReply)
                return;

            isWatingForCreateAccountReply = true;

            client.Send(new LcRequestCreateAccount(login, passwordHash, email, promotionCode), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
        }

        public void StartLoginClient()
        {
            client.Connect(loginServerIp, loginServerPort, LoginServer.LoginSession);
        }

        public void StopLoginClient()
        {
            client.Disconnect();

            sessionId = 0;
            isConnected = false;
            isAuthorized = false;
            isWatingForCreateAccountReply = false;
        }

        private void Update()
        {
            if(client != null)
                client.Update();
        }

        public void OnClientConnected(LidgrenClient client)
        {
            isConnected = true;
            OnConnected();
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

        public virtual void OnCreateAccountResult(bool success)
        {

        }

        public virtual void OnDisconnected()
        {

        }
    }
}