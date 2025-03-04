using System.Collections.Concurrent;

namespace GateKeeper.Server.Models
{
    public class GateKeeper
    {
        public static int PER_ACCOUNT = 200;
        public static int PER_PHONE = 50;
        public static string PER_ACCOUNT_EXCEEDED = "Per Account maximum exceeded";
        public static string PER_PHONE_EXCEEDED = "Per Phone maximum exceeded";
        public static string MESSAGE_QUEUED = "Message queued.";
        public static string MESSAGE_SENT = "Message sent.";
        public static string MESSAGE_NOT_SENT = "Message not sent.";

        ConcurrentDictionary<Guid, Account> Accounts;
        ConcurrentDictionary<Guid, AccountMessageQueue> MessageQueue;

        public GateKeeper()
        {
            Accounts = new ConcurrentDictionary<Guid, Account>();
            MessageQueue = new ConcurrentDictionary<Guid, AccountMessageQueue>();
        }

        // Really only necessary because of the TestSendMessage unit tests
        private Account AddOrUpdateAccount(Guid accountId, Account account)
        {
            return account;
        }

        private AccountMessageQueue AddOrUpdateAccountMessageQueue(Guid accountId, AccountMessageQueue newQueue)
        {
            return newQueue;
        }

        public void AddAccount(Account newAccount)
        {
            Accounts.AddOrUpdate(newAccount.AccountId, newAccount, AddOrUpdateAccount);
            MessageQueue.AddOrUpdate(newAccount.AccountId, new AccountMessageQueue(newAccount.AccountId), AddOrUpdateAccountMessageQueue);
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

        public void removeUser(Guid accountId, Guid userId)
        {
            Account account = GetAccount(accountId);
            User removedUser = account.Users.Where(u => u.UserId == userId).Select(u => u).First();
            account.Users.Remove(removedUser);

            // When user is removed, remove from account message queue as well
            AccountMessageQueue accountMessageQueue =
                MessageQueue
                .Where(mq => mq.Key == accountId)
                .Select(mq => mq.Value).First();

            List<Message> messages = new List<Message>() { };
            accountMessageQueue.PendingMessagesPerUser.Remove(userId, out messages);
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

        public Message? GetMessageFromQueue(Guid accountId, Guid userId)
        {
            AccountMessageQueue accountMessageQueue =
                MessageQueue
                    .Where(mq => mq.Key == accountId)
                    .Select(mq => mq.Value).First();

            return accountMessageQueue.GetMessageFromQueue(userId);
        }

        public void RemoveMessageFromQueue(Message message)
        {
            AccountMessageQueue accountMessageQueue =
                MessageQueue
                    .Where(mq => mq.Key == message.AccountId)
                    .Select(mq => mq.Value).First();

            accountMessageQueue.RemoveMessageFromQueue(message);
        }
    }
}