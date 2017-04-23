
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse
{
    public sealed class NetworkMessageHandler
    {
        public delegate void MessageHandler(Message m);

        private Dictionary<Type, MessageHandler> handlers { get; set; }

        public NetworkMessageHandler()
        {
            handlers = new Dictionary<Type, MessageHandler>();
        }

        public void SetHandler<T>(MessageHandler handler) where T : Message
        {
            handlers[typeof(T)] = handler;
        }

        public void HandleMessage(Message m)
        {
            handlers[m.GetType()](m);
        }
    }
}
