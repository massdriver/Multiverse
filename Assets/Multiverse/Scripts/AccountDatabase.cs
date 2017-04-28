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
            accountCollection.EnsureIndex("id", true);
        }

        public bool CreateAccount(string login, string passwordHash, string email, string promocode)
        {
            Account existing = accountCollection.FindById(new BsonValue(HashUtil.Make(login)));

            if (existing != null)
                return false;

            Account newAccount = new Account();
            newAccount.id = HashUtil.MakeInt32(login);
            newAccount.login = login;
            newAccount.email = email;
            newAccount.passwordHash = passwordHash;
            newAccount.promoCode = promocode;
            newAccount.dateCreated = DateTime.Now;

            BsonValue val = accountCollection.Insert(newAccount);

            Debug.Log("Account Database: account created, id=" + newAccount.id + ", val=" + val.ToString());

            return true;
        }

        public Account GetAccount(string login, string passwordHash)
        {
            {
                foreach(Account acc in accountCollection.FindAll())
                {
                    Debug.Log("acc id=" + acc.id + ", login=" + acc.login);
                }
            }

            Account existing = accountCollection.FindById(new BsonValue(HashUtil.MakeInt32(login)));

            if (existing == null)
            {
                Debug.Log("Account Database: account not found");
                return null;
            }

            if (existing.passwordHash == passwordHash)
            {
                return existing;
            }

            Debug.Log("Account Database: account password invalid");
            return null;
        }

        public void UpdateAccount(Account account)
        {
            accountCollection.Update(account);
        }
    }
}
