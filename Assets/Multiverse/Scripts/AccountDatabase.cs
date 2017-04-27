using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LiteDB;

namespace Multiverse
{
    public sealed class AccountDatabase : MonoBehaviour
    {
        public static readonly string AccountTableName = "accounts";

        public string databaseFilePath;
        public string password;

        private LiteDatabase database;
        private LiteCollection<Account> accountCollection;

        private void Awake()
        {
            if (databaseFilePath == null || databaseFilePath.Length == 0)
                throw new InvalidOperationException("database file path was not set");

            database = new LiteDatabase(databaseFilePath);
            accountCollection = database.GetCollection<Account>();
        }

        public bool CreateAccount(string login, string passwordHash, string email, string promocode)
        {
            Account existing = accountCollection.FindById(new BsonValue(HashUtil.Make(login)));

            if (existing != null)
                return false;

            Account newAccount = new Account();
            newAccount.id = HashUtil.Make(login);
            newAccount.email = email;
            newAccount.passwordHash = passwordHash;
            newAccount.promoCode = promocode;
            newAccount.dateCreated = DateTime.Now;

            accountCollection.Insert(newAccount);

            return true;
        }

        public Account GetAccount(string login, string passwordHash)
        {
            Account existing = accountCollection.FindById(new BsonValue(HashUtil.Make(login)));

            if (existing == null)
                return null;

            if (existing.passwordHash == passwordHash)
                return existing;

            return null;
        }

        public void UpdateAccount(Account account)
        {
            accountCollection.Update(account);
        }
    }
}
