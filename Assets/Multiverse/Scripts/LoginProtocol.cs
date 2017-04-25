using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Multiverse
{
    public sealed class LcRequestLogin : Message
    {
        public string login { get; set; }
        public string passwordHash { get; set; }

        public LcRequestLogin()
        {

        }

        public LcRequestLogin(string login, string passwordHash)
        {
            this.login = login;
            this.passwordHash = passwordHash;
        }

        public override void Read(NetBuffer msg)
        {
            login = msg.ReadString();
            passwordHash = msg.ReadString();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(login);
            msg.Write(passwordHash);
        }
    }

    public sealed class LsLoginReply : Message
    {
        public bool authorized { get; set; }
        public ulong sessionId { get; set; }

        public override void Read(NetBuffer msg)
        {
            authorized = msg.ReadBoolean();
            sessionId = msg.ReadUInt64();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(authorized);
            msg.Write(sessionId);
        }
    }

    public sealed class LsCreateAccountReply : Message
    {
        public bool success { get; set; }

        public LsCreateAccountReply()
        {

        }

        public LsCreateAccountReply(bool success)
        {
            this.success = success;
        }

        public override void Read(NetBuffer msg)
        {
            success = msg.ReadBoolean();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(success);
        }
    }

    public sealed class LcRequestCreateAccount : Message
    {
        public string login { get; set; }
        public string email { get; set; }
        public string passwordHash { get; set; }
        public string promotionCode { get; set; }

        public override void Read(NetBuffer msg)
        {
            login = msg.ReadString();
            email = msg.ReadString();
            passwordHash = msg.ReadString();
            promotionCode = msg.ReadString();
        }

        public override void Write(NetBuffer msg)
        {
            msg.Write(login);
            msg.Write(email);
            msg.Write(passwordHash);
            msg.Write(promotionCode);
        }
    }
}
