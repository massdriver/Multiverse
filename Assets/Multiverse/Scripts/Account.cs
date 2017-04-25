using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse
{
    public class Account
    {
        public ulong id { get; set; }
        public string email { get; set; }
        public string passwordHash { get; set; }
        public DateTime dateCreated { get; set; }
    }
}
