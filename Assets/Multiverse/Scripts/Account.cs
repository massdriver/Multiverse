using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Multiverse
{
    public class Account
    {
        [LiteDB.BsonId]
        public long id;
        public string login;
        public string email;
        public string passwordHash;
        public string promoCode;
        public DateTime dateCreated;

        public ulong premiumCurrency;
        public DateTime premiumSubscription;
    }
}
