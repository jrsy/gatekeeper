using GateKeeper.Server.Controllers;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GateKeeper.Server.Models
{
    public class AccountMessageQueue
    {
        public Guid AccountId { get; set; }
        public ConcurrentDictionary<Guid, DateTime> MessageTimes; // Keeps track of messages from entire account
        public ConcurrentDictionary<Guid, List<Message>> PendingMessagesPerUser;
        public ConcurrentDictionary<Guid, List<Message>> SentMessagesPerUser;

        public AccountMessageQueue(Guid accountId)
        {
            AccountId = accountId;
            MessageTimes = new ConcurrentDictionary<Guid, DateTime>();
            PendingMessagesPerUser = new ConcurrentDictionary<Guid, List<Message>>();
            SentMessagesPerUser = new ConcurrentDictionary<Guid, List<Message>>();
        }

        public string AttemptSendSMSMessage(Guid accountId, Guid userId, Message message)
        {
            int pendingForWholeAccount =
                MessageTimes
                .Where(mt => message.Timestamp.Subtract(mt.Value).TotalSeconds < 1)
                .Select(mt => mt.Key).Count();

            if (pendingForWholeAccount > GateKeeper.PER_ACCOUNT
                && MessageTimes.Any())
            {
                return GateKeeper.PER_ACCOUNT_EXCEEDED;
            }

            List<Message>? pendingForThisUser =
                PendingMessagesPerUser
                .Where(pm => pm.Key == userId)
                .Select(pm => pm.Value).FirstOrDefault();

            int pendingForThisUserWithinLastSecond = 0;
            if (pendingForThisUser != null)
            {
                pendingForThisUserWithinLastSecond =
                    pendingForThisUser
                    .Where(m => message.Timestamp.Subtract(m.Timestamp).TotalSeconds < 1)
                    .Select(m => m).Count();
            }

            if (pendingForThisUserWithinLastSecond > GateKeeper.PER_PHONE)
            {
                return GateKeeper.PER_PHONE_EXCEEDED;
            }

            AddOrUpdateMessagesPerUser(PendingMessagesPerUser, userId, message);

            MessageTimes.AddOrUpdate(message.MessageId, message.Timestamp, AddOrUpdateMessageTimes);

            return String.Empty;
        }

        private void AddOrUpdateMessagesPerUser(ConcurrentDictionary<Guid, List<Message>> perUser, Guid userId, Message message)
        {
            if (perUser.ContainsKey(userId))
            {
                perUser[userId].Add(message);
            }
            else
            {
                // This is mostly needed for the TestSendMessage unit test. In real life we'd call await
                if (perUser.ContainsKey(userId))
                {
                    perUser[userId].Add(message);
                }
                else
                {
                    List<Message> newList = new List<Message> { message };
                    perUser.AddOrUpdate(userId, newList, AddOrUpdateMessagesPerUserPlaceholder);
                }
            }
        }

        // Placeholder for unit tests and front end tests
        private DateTime AddOrUpdateMessageTimes(Guid messageId, DateTime timestamp)
        {
            return timestamp;
        }

        private List<Message> AddOrUpdateMessagesPerUserPlaceholder(Guid userId, List<Message> newList)
        {
            return newList;
        }

        public Message? GetMessageFromQueue(Guid userId)
        {
            List<Message>? messageList =
                PendingMessagesPerUser
                .Where(pm => pm.Key == userId)
                .Select(pm => pm.Value).FirstOrDefault();
            if (messageList == null || !messageList.Any())
            {
                return null;
            }

            Message? firstMessageUp =
                messageList.Select(m => m).FirstOrDefault();

            if (firstMessageUp == null)
            {
                return firstMessageUp;
            }

            return firstMessageUp;
        }

        public void RemoveMessageFromQueue(Message message)
        {
            PendingMessagesPerUser[message.UserId].Remove(message);
            if (!PendingMessagesPerUser[message.UserId].Any())
            {
                List<Message> messages = new List<Message>() { };
                PendingMessagesPerUser.Remove(message.UserId, out messages);
            }

            DateTime timestamp = message.Timestamp;
            MessageTimes.Remove(message.MessageId, out timestamp);

            if (message.Sent != null)
            {
                AddOrUpdateMessagesPerUser(SentMessagesPerUser, message.UserId, message);
            }
        }
    }
}