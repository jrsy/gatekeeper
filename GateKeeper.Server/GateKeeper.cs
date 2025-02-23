using GateKeeper.Server.Models;

namespace GateKeeper.Server
{
    public class GateKeeper
    {
        public static int PER_ACCOUNT = 20;
        public static int PER_PHONE = 5;

        Dictionary<Guid, Account> Accounts;
        Dictionary<Guid, AccountMessageQueue> MessageQueue;

        public GateKeeper()
        {
            Accounts = new Dictionary<Guid, Account>();
            MessageQueue = new Dictionary<Guid, AccountMessageQueue>();

            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            account1.AddUser("User 1.2", "112-1112");
            account1.AddUser("User 1.3", "113-1113");

            Account account2 = new Account("Account 2");
            account2.AddUser("User 2.1", "221-2221");
            account2.AddUser("User 2.2", "222-2222");
            Accounts.Add(account1.AccountId, account1);
            Accounts.Add(account2.AccountId, account2);
        }

        public Account[] GetAccounts()
        {
            return Accounts
                .Select(a => a.Value).ToArray();
        }

        public Account GetAccount(Guid accountId)
        {
            return Accounts
                .Where(a => a.Key == accountId)
                .Select(a => a.Value).First();
        }

        public string AttemptSendSMSMessage(Guid accountId, Guid userId, Message message)
        {
            AccountMessageQueue accountMessageQueue =
                MessageQueue
                .Where(mq => mq.Key == accountId)
                .Select(mq => mq.Value).First();

            string response = accountMessageQueue.AttemptSendSMSMessage(accountId, userId, message);
            return response;
        }

        public Message GetMessageFromQueue(Guid accountId, Guid userId)
        {
            AccountMessageQueue accountMessageQueue =
                MessageQueue
                    .Where(mq => mq.Key == accountId)
                    .Select(mq => mq.Value).First();

            return accountMessageQueue.GetMessageFromQueue(userId);
        }
    }
}