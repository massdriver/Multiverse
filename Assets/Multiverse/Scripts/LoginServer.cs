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

        public override string ToString()
        {
            return "LoginSession: " + "clientId=" + clientId + ", sessionId=" + sessionId;
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

        public ushort maxClients = 128;

        public ushort LoginServerPort = 16543;

        public int numConnections
        {
            get
            {
                return server.activeClientIDs.Length;
            }
        }

        public int numSessions
        {
            get
            {
                return activeSessions.Count;
            }
        }

        public bool isRunning { get; private set; }

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

        public void StartLoginServer()
        {
            server.Start(LoginServerPort, maxClients, LoginSession, 5000);
            isRunning = true;
            OnLoginServerStarted();
        }

        public void StopLoginServer()
        {
            isRunning = false;
            server.Update();
            server.Stop();

            foreach(var kp in activeSessions)
            {
                OnPreLoginSessionLogout(kp.Value);
                accountDatabase.UpdateAccount(kp.Value.account);
            }

            activeSessions.Clear();

            OnLoginServerStopped();
        }

        private void Update()
        {
            if (server != null)
                server.Update();
        }

        private void HandleLcRequestCreateAccount(Message m)
        {
            LcRequestCreateAccount msg = m as LcRequestCreateAccount;

            if (!OnPreAllowCreateAccount(msg.login, msg.passwordHash, msg.email, msg.promotionCode))
            {
                server.Send(msg.sourceClient, new LsCreateAccountReply(false), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);

                Debug.Log("Login Server: failed to create account");
                return;
            }

            bool result = accountDatabase.CreateAccount(msg.login, msg.passwordHash, msg.email, msg.promotionCode);

            server.Send(msg.sourceClient, new LsCreateAccountReply(result), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);

            OnAccountCreated(msg.login, msg.passwordHash, msg.email, msg.promotionCode);
        }

        public bool IsAccountLogged(string login)
        {
            foreach(var kp in activeSessions)
            {
                if (kp.Value.account.login == login)
                    return true;
            }

            return false;
        }

        private void HandleLcRequestLogin(Message m)
        {
            LcRequestLogin msg = m as LcRequestLogin;

            // Do not login twice
            if(IsAccountLogged(msg.login))
            {
                server.Send(msg.sourceClient, new LsLoginReply(false, 0), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
                OnClientAuthorizationFailed(msg.sourceClient);
                return;
            }

            Account account = accountDatabase.GetAccount(msg.login, msg.passwordHash);

            if(account == null)
            {
                server.Send(msg.sourceClient, new LsLoginReply(false, 0), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);
                OnClientAuthorizationFailed(msg.sourceClient);
                return;
            }

            LoginSession newSession = new Multiverse.LoginSession(msg.sourceClient, nextSessionId, account);
            activeSessions.Add(msg.sourceClient, newSession);

            server.Send(msg.sourceClient, new LsLoginReply(false, newSession.sessionId), Lidgren.Network.NetDeliveryMethod.ReliableOrdered);

            OnNewLoginSession(newSession);
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

            OnPreLoginSessionLogout(session);

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

            Debug.Log("LoginServer OnPreAllowCreateAccount: " + "login=" + login + ", passwordHash=" + passwordHash + ", email=" + email + ", promotionCode=" + promotionCode);

            return false;
        }

        public virtual void OnNewLoginSession(LoginSession session)
        {

        }

        public virtual void OnClientAuthorizationFailed(ushort client)
        {

        }

        public virtual void OnClientConnected(ushort newClientID)
        {
            Debug.Log("LoginServer OnClientConnected: " + newClientID);
        }

        public virtual void OnClientDisconnected(ushort leavingClientID)
        {
            Debug.Log("LoginServer OnClientDisconnected: " + leavingClientID);
        }

        public virtual void OnPreLoginSessionLogout(LoginSession session)
        {
            Debug.Log("LoginServer LoginSession: " + session.ToString());
        }

        public virtual void OnLoginServerStarted()
        {
            Debug.Log("LoginServer OnLoginServerStarted");
        }

        public virtual void OnLoginServerStopped()
        {
            Debug.Log("LoginServer OnLoginServerStopped");
        }

        public virtual void OnAccountCreated(string login, string passwordHash, string email, string promotionCode)
        {
            Debug.Log("LoginServer OnAccountCreated: " + "login=" + login + ", passwordHash=" + passwordHash + ", email=" + email + ", promotionCode=" + promotionCode);
        }
    }
}