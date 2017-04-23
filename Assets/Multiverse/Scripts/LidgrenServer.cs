using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

namespace Multiverse
{
    public sealed class LidgrenServer
    {
        public interface IDelegate
        {
            void OnServerClientConnected(LidgrenServer server, ushort newClientID);
            void OnServerClientDisconnected(LidgrenServer server, ushort leavingClientID);
            void OnServerMessageReceived(LidgrenServer server, ushort sourceClient, Message msg);
        }

        public ushort[] activeClientIDs { get { return activeClients.ToArray(); } }

        public ushort port { get; private set; }
        public ushort maxClients { get; private set; }
        public IDelegate serverDelegate { get; set; }
        public string sessionName { get; private set; }

        public static readonly string DEFAULT_SESSION_NAME = "default_session";

        public bool acceptIncomingConnections
        {
            get
            {
                return netServer.Configuration.AcceptIncomingConnections;
            }

            set
            {
                netServer.Configuration.AcceptIncomingConnections = value;
            }
        }

        public LidgrenServer()
        {
            messageTypes = new Dictionary<int, Type>();
        }

        public void Start(ushort port, ushort maxClients, string sessionName, float connectionTimeout)
        {
            if (sessionName == null || sessionName.Length == 0)
                sessionName = LidgrenServer.DEFAULT_SESSION_NAME;

            if (maxClients == 0)
                throw new Exception("Unable to create server with zero clients");

            this.sessionName = sessionName;
            this.port = port;
            this.maxClients = maxClients;

            activeClients = new List<ushort>();

            clientInfo = new ClientInfo[maxClients];
            freeids = new Stack<ushort>();

            for (ushort i = 0; i < maxClients; i++)
                freeids.Push(i);

            serverConfig = new NetPeerConfiguration(sessionName);
            serverConfig.ConnectionTimeout = connectionTimeout;
            serverConfig.Port = port;
            serverConfig.MaximumConnections = maxClients;

            netServer = new NetServer(serverConfig);
            netServer.Start();
        }

        public NetPeerStatus status
        {
            get
            {
                if(netServer != null)
                    return netServer.Status;

                return NetPeerStatus.NotRunning;
            }
        }

        public NetConnection GetConnection(ushort clientid)
        {
            return clientInfo[clientid].connection;
        }

        ~LidgrenServer()
        {
            if (netServer != null)
                throw new Exception("Server was not properly stopped");
        }

        public void Send(ushort targetClient, Message msg, NetDeliveryMethod deliveryType)
        {
            NetOutgoingMessage msgOut = netServer.CreateMessage();

            msgOut.Write(Message.GetMessageCode(msg.GetType()));
            msg.Write(msgOut);

            netServer.SendMessage(msgOut, clientInfo[targetClient].connection, deliveryType);

        }

        public void SendToAllExceptOneClient(ushort exceptThisClient, Message msg, NetDeliveryMethod deliveryType)
        {
            NetOutgoingMessage msgOut = netServer.CreateMessage();

            msgOut.Write(Message.GetMessageCode(msg.GetType()));
            msg.Write(msgOut);

            foreach(ushort clid in activeClients)
            {
                if(clid != exceptThisClient)
                    netServer.SendMessage(msgOut, clientInfo[clid].connection, deliveryType);
            }
        }

        public void SendToAll(Message msg, NetDeliveryMethod deliveryType)
        {
            NetOutgoingMessage msgOut = netServer.CreateMessage();

            msgOut.Write(Message.GetMessageCode(msg.GetType()));
            msg.Write(msgOut);

            netServer.SendToAll(msgOut, deliveryType);
        }

        public void Update()
        {
            if (netServer == null)
                return;

            NetIncomingMessage im;
            while ((im = netServer.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        if (status == NetConnectionStatus.Connected)
                        {
                            ushort newClient = freeids.Pop();

                            clientInfo[newClient] = new ClientInfo(newClient, im.SenderConnection);
                            im.SenderConnection.Tag = clientInfo[newClient];

                            activeClients.Add(newClient);

                            if (serverDelegate != null)
                                serverDelegate.OnServerClientConnected(this, newClient);
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            ushort clientID = ((ClientInfo)im.SenderConnection.Tag).id;

                            if (serverDelegate != null)
                                serverDelegate.OnServerClientDisconnected(this, clientID);

                            clientInfo[clientID] = null;
                            freeids.Push(clientID);

                            activeClients.Remove(clientID);
                        }


                        break;
                    case NetIncomingMessageType.Data:
                        {
                           
                            ushort clientID = ((ClientInfo)im.SenderConnection.Tag).id;
                            int msgid = im.ReadInt32();

                            Message msg = CreateMessageObject(msgid);

                            if (msg != null && serverDelegate != null)
                            {
                                msg.id = msgid;
                                msg.Read(im);
                                msg.sourceClient = clientID;
                                serverDelegate.OnServerMessageReceived(this, clientID, msg);
                            }
                        }
                        break;
                }

                netServer.Recycle(im);
            }
        }

        public void Stop()
        {
            if (netServer == null)
                return;

            netServer.FlushSendQueue();
            netServer.Shutdown("bye");
            netServer = null;
        }
        
        // MH: not sure if this works actually   
        public void Kick(ushort id)
        {
            if (clientInfo[id] == null)
                return;

            NetConnection connection = clientInfo[id].connection;

            if (connection == null)
                return;

            connection.Disconnect(null);

            clientInfo[id] = null;
            freeids.Push(id);
            activeClients.Remove(id);
        }

        public void RegisterMessageType<T>() where T : Message
        {
            int code = Message.GetMessageCode(typeof(T));

            //if (messageTypes.ContainsKey(code))
            //    throw new Exception("Message type already registered or there's typename hash collision");

            messageTypes[code] = typeof(T);
        }

        public Message CreateMessageObject(int id)
        {
            Type msgType = null;

            if (messageTypes.TryGetValue(id, out msgType))
                return Activator.CreateInstance(msgType) as Message;

            throw new Exception("Unregistered message type found");
        }

        private class ClientInfo
        {
            public ushort id { get; private set; }
            public NetConnection connection { get; private set; }

            public ClientInfo(ushort id, NetConnection connection)
            {
                this.id = id;
                this.connection = connection;
            }
        }

        private List<ushort> activeClients { get; set; }
        private Dictionary<int, Type> messageTypes { get; set; }
        private Stack<ushort> freeids { get; set; }
        private ClientInfo[] clientInfo { get; set; }
        private NetServer netServer { get; set; }
        private NetPeerConfiguration serverConfig { get; set; }
    }
}
