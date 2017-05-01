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

        public string databaseDirectory;

        private FileStorage<Account> accountStorage;

        private void Awake()
        {
            accountStorage = new FileStorage<Account>(databaseDirectory);
        }

        public bool CreateAccount(string login, string passwordHash, string email, string promocode)
        {
            Account existing = accountStorage.Load(login);

            if (existing != null)
                return false;

            Account newAccount = new Account();
            newAccount.id = HashUtil.FromString64(login);
            newAccount.login = login;
            newAccount.email = email;
            newAccount.passwordHash = passwordHash;
            newAccount.promoCode = promocode;
            newAccount.dateCreated = DateTime.Now;

            accountStorage.Store(newAccount);

            return true;
        }

        public Account GetAccount(string login, string passwordHash)
        {
            Account existing = accountStorage.Load(login);

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
            if (!accountStorage.Exists(account.id))
            {
                Debug.LogWarning("Storage element doesnt exist but there was attempt to update it, skipped");
                return;
            }

            accountStorage.Store(account);
        }
    }
}
