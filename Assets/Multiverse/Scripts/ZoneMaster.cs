using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

namespace Multiverse
{
    public class ZoneMaster : MonoBehaviour, LidgrenServer.IDelegate
    {
        public static readonly string ZoneSessionName = "Zoners";

        private Dictionary<ulong, ZoneWorldInfo> zones { get; set; }
        private LidgrenServer server { get; set; }
        private NetworkMessageHandler handler { get; set; }

        public const ushort ZoneMasterPort = 10333;
        public const ushort MaxZones = 32;

        public static ZoneMaster singleton { get; private set; }

        private void Awake()
        {
            if (singleton != null)
                throw new InvalidOperationException("ZoneMaster script already present on scene, only one allowed");

            singleton = this;

            handler = new NetworkMessageHandler();
            handler.SetHandler<ZcRegisterWorld>(HandleZcRegisterWorld);

            server = new LidgrenServer();
            server.serverDelegate = this;
            zones = new Dictionary<ulong, ZoneWorldInfo>();

            RegisterMessage<ZcRegisterWorld>(HandleZcRegisterWorld);
        }

        public void StartZoneMaster()
        {
            server.Start(ZoneMasterPort, MaxZones, ZoneSessionName, 1000);

            OnZoneMasterStarted();
        }

        private void RegisterMessage<T>(NetworkMessageHandler.MessageHandler msgHandler) where T : Message
        {
            server.RegisterMessageType<T>();
            handler.SetHandler<T>(msgHandler);
        }

        public List<ZoneWorldInfo> allZones
        {
            get
            {
                return zones.Values.ToList();
            }
        }

        private void Update()
        {
            if (server != null)
                server.Update();
        }

        void HandleZcRegisterWorld(Message m)
        {
            ZcRegisterWorld msg = m as ZcRegisterWorld;

            ZoneWorldInfo newZone = null;

            if (zones.TryGetValue(msg.worldZoneId, out newZone))
            {
                server.Send(msg.sourceClient, new ZmRegisterWorldReply(false), NetDeliveryMethod.ReliableOrdered);
                Debug.LogWarning("Attempt to register already registered zone server");
                return;
            }

            newZone = new ZoneWorldInfo(msg.worldZoneId, msg.sceneBuildIndex, msg.ip, msg.port, msg.sourceClient);
            zones.Add(msg.worldZoneId, newZone);
            OnZoneRegistered(newZone);

            // Notify zone client that everything is okay
            server.Send(msg.sourceClient, new ZmRegisterWorldReply(true), NetDeliveryMethod.ReliableOrdered);
        }

        public void OnServerClientConnected(LidgrenServer server, ushort newClientID)
        {
            // do nothing unless or start register timer
        }

        public void OnServerClientDisconnected(LidgrenServer server, ushort leavingClientID)
        {
            ZoneWorldInfo zw = ZoneWorldFromClientId(leavingClientID);
            OnZoneDisconnected(zw);
            zones.Remove(zw.worldZoneId);
        }

        public void OnServerMessageReceived(LidgrenServer server, ushort sourceClient, Message msg)
        {
            handler.HandleMessage(msg);
        }

        private ZoneWorldInfo ZoneWorldFromClientId(ushort zoneClient)
        {
            foreach(var kp in zones)
            {
                if (kp.Value.zoneClientId == zoneClient)
                    return kp.Value;
            }

            return null;
        }

        //
        // Events
        //
        public virtual void OnZoneMasterStarted()
        {

        }

        public virtual void OnZoneRegistered(ZoneWorldInfo info)
        {

        }

        public virtual void OnZoneDisconnected(ZoneWorldInfo info)
        {

        }
    }
}
