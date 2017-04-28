using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Multiverse
{
    public sealed class LidgrenClient
    {
        public interface IDelegate
        {
            void OnClientConnected(LidgrenClient client);
            void OnClientDisconnected(LidgrenClient client);
            void OnClientMessageReceived(LidgrenClient client, Message msg);
        }

        public static readonly int InvalidClientID = -1;

        public int clientid { get; set; }
        public IDelegate clientDelegate { get; set; }

        public LidgrenClient()
        {
            clientid = LidgrenClient.InvalidClientID;

            messageTypes = new Dictionary<int, Type>();
        }

        public NetConnection GetConnection()
        {
            return client.ServerConnection;
        }

        /*
         * This call is asynchronuous, connect delegate invoke wont occur until next Update()
         */
        public void Connect(string address, ushort port, string sessionName)
        {
            if (sessionName == null || sessionName.Length == 0)
                sessionName = LidgrenServer.DEFAULT_SESSION_NAME;

            client = new NetClient(new NetPeerConfiguration(sessionName));
            client.Start();
            client.Connect(address, port);
        }

        public void Update()
        {
            if(client == null)
                return;

            NetIncomingMessage msg = null;

            while (true)
            {
                // MH: someone may call Disconnect inside delegate, actually it should just poll events and then process them sequentially but Im lazu
                if (client == null)
                    break;

                msg = client.ReadMessage();

                if (msg == null)
                    break;

                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.StatusChanged:
                        {
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();

                            if (clientDelegate != null)
                            {
                                if (status == NetConnectionStatus.Connected)
                                {
                                    clientDelegate.OnClientConnected(this);
                                }

                                if (status == NetConnectionStatus.Disconnected)
                                {
                                    clientDelegate.OnClientDisconnected(this);
                                }
                            }

                        }
                        break;

                    case NetIncomingMessageType.Data:
                        {
                            int msgid = msg.ReadInt32();
                            Message message = CreateMessageObject(msgid);

                            if ((clientDelegate != null) && (message != null))
                            {
                                message.id = msgid;
                                message.Read(msg);
                                clientDelegate.OnClientMessageReceived(this, message);
                            }
                        }
                        break;

                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    default:
                        break;
                }

                if(client != null)
                    client.Recycle(msg);
            }
        }

        public void Disconnect()
        {
            if (client != null)
            {
                client.Disconnect("bye");
                client.Shutdown("bye");
            }

            client = null;
        }

        public bool isConnected
        {
            get
            {
                return client != null && client.ConnectionStatus == NetConnectionStatus.Connected;
            }
        }

        public void Send(Message msg, NetDeliveryMethod deliveryType)
        {
            if (client == null)
                throw new Exception("Client was not started");

            NetOutgoingMessage msgOut = client.CreateMessage();
            msgOut.Write(Message.GetMessageCode(msg.GetType()));
            msg.Write(msgOut);
            client.SendMessage(msgOut, deliveryType);
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

            return null;
        }

        private NetClient client { get; set; }
        private Dictionary<int, Type> messageTypes { get; set; }
    }
}
