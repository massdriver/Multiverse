using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiverse
{
    public sealed class LoginSession
    {
        public ushort clientId { get; set; }
        public ulong sessionId { get; set; }

        public Account account { get; set; }
        public Character activeCharacter { get; set; }

        public LoginSession(ushort clientId, ulong sessionId, Account account)
        {
            this.clientId = clientId;
            this.sessionId = sessionId;
            this.account = account;
        }
    }

    [RequireComponent(typeof(AccountDatabase))]
    public class LoginServer : MonoBehaviour, LidgrenServer.IDelegate
    {
        public static readonly string LoginSession = "Loginz";

        private LidgrenServer server;
        private NetworkMessageHandler handler;
        private Dictionary<ushort, LoginSession> activeSessions;
        private AccountDatabase accountDatabase;

        private void Awake()
        {
            accountDatabase = GetComponent<AccountDatabase>();

            activeSessions = new Dictionary<ushort, Multiverse.LoginSession>();

            server = new LidgrenServer();
            server.serverDelegate = this;

            handler = new NetworkMessageHandler();

            RegisterMessage<LcRequestLogin>(HandleLcRequestLogin);
            RegisterMessage<LcRequestCreateAccount>(HandleLcRequestCreateAccount);
        }

        private void HandleLcRequestCreateAccount(Message m)
        {
            LcRequestCreateAccount msg = m as LcRequestCreateAccount;

            if (!OnPreAllowCreateAccount(msg.login, msg.passwordHash, msg.email, msg.promotionCode))
            {
                server.Send(msg.sourceClient, new LsCreateAccountReply(false), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
            }

            bool result = accountDatabase.CreateAccount(msg.login, msg.passwordHash, msg.email, msg.promotionCode);

            server.Send(msg.sourceClient, new LsCreateAccountReply(result), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
        }

        private void HandleLcRequestLogin(Message m)
        {
            LcRequestLogin msg = m as LcRequestLogin;

            Account account = accountDatabase.GetAccount(msg.login, msg.passwordHash);

            if(account == null)
            {
                server.Send(msg.sourceClient, new LsLoginReply(false, 0), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
                return;
            }

            LoginSession newSession = new Multiverse.LoginSession(msg.sourceClient, nextSessionId, account);
            activeSessions.Add(msg.sourceClient, newSession);

            server.Send(msg.sourceClient, new LsLoginReply(false, newSession.sessionId), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
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

            LogoutClientSession(leavingClientID);
        }

        public void OnServerMessageReceived(LidgrenServer server, ushort sourceClient, Message msg)
        {
            handler.HandleMessage(msg);
        }

        private ulong sessionIdGen = 123;

        private ulong nextSessionId
        {
            get
            {
                return sessionIdGen++;
            }
        }

        private void LogoutClientSession(ushort clientId)
        {
            LoginSession session = null;

            activeSessions.TryGetValue(clientId, out session);

            if (session == null)
                return;

            accountDatabase.UpdateAccount(session.account);
            activeSessions.Remove(clientId);
        }

        //
        // Events
        //
        public virtual bool OnPreAllowCreateAccount(string login, string passwordHash, string email, string promotionCode)
        {
            // check login
            // check email
            // check passwordHash
            // promocode optional

            return false;
        }

        public virtual void OnClientConnected(ushort newClientID)
        {

        }

        public virtual void OnClientDisconnected(ushort leavingClientID)
        {

        }
    }
}