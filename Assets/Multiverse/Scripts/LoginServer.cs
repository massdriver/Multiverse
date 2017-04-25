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
    }

    public class LoginServer : MonoBehaviour, LidgrenServer.IDelegate
    {
        public static readonly string LoginSession = "Loginz";

        private LidgrenServer server { get; set; }
        private NetworkMessageHandler handler { get; set; }
        private Dictionary<ushort, LoginSession> loginSessions { get; set; }

        private void Awake()
        {
            loginSessions = new Dictionary<ushort, Multiverse.LoginSession>();

            server = new LidgrenServer();
            server.serverDelegate = this;

            RegisterMessage<LcRequestLogin>(HandleLcRequestLogin);
            RegisterMessage<LcRequestCreateAccount>(HandleLcRequestCreateAccount);
        }

        private void HandleLcRequestCreateAccount(Message m)
        {

        }

        private void HandleLcRequestLogin(Message m)
        {
            LcRequestLogin msg = m as LcRequestLogin;
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

        private LoginSession AllocateLoginSession()
        {
            LoginSession ls = new Multiverse.LoginSession();

            ls.sessionId = nextSessionId;

            return ls;
        }

        //
        // Events
        //
        public virtual bool OnAllowCreateAccount()
        {
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