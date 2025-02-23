using GateKeeper.Server.Controllers;

namespace GateKeeper.Server.Models
{
    public class AccountMessageQueue
    {
        public Guid AccountId { get; set; }
        public Dictionary<Guid, DateTime> MessageTimes;
        public Dictionary<Guid, List<Message>> PendingMessagesPerUser;

        public AccountMessageQueue(Guid accountId)
        {
            AccountId = accountId;
            MessageTimes = new Dictionary<Guid, DateTime>();
            PendingMessagesPerUser = new Dictionary<Guid, List<Message>>();
        }

        public string AttemptSendSMSMessage(Guid accountId, Guid userId, Message message)
        {
            if (PendingMessagesPerUser.Count > GateKeeper.PER_ACCOUNT
                && MessageTimes.Any())
            {
                int pendingForWholeAccount =
                    MessageTimes
                    .Where(mt => message.Timestamp.Subtract(mt.Value).Seconds < 1)
                    .Select(mt => mt.Key).Count();

                return "Per Account maximum exceeded";
            }

            int pendingForThisUserWithinLastSecond =
                PendingMessagesPerUser
                .Where(pm => pm.Key == userId)
                .Select(pm => pm.Value
                    .Where(m => message.Timestamp.Subtract(m.Timestamp).Seconds < 1)
                    .Select(m => m).ToList()
                ).Count();

            if (pendingForThisUserWithinLastSecond > GateKeeper.PER_PHONE)
            {
                return "Per User maximum exceeded";
            }

            if (PendingMessagesPerUser.ContainsKey(userId))
            {
                PendingMessagesPerUser[userId].Add(message);
            }
            else
            {
                List<Message> newList = new List<Message> { message };
                PendingMessagesPerUser.Add(userId, newList);
            }

            MessageTimes.Add(message.MessageId, message.Timestamp);

            return String.Empty;
        }

        public Message GetMessageFromQueue(Guid userId)
        {
            Message firstMessageUp =
                PendingMessagesPerUser
                .Where(pm => pm.Key == userId)
                .Select(pm => pm.Value).First().Select(m => m).First();

            PendingMessagesPerUser[userId].Remove(firstMessageUp);
            if (!PendingMessagesPerUser[userId].Any())
            {
                PendingMessagesPerUser.Remove(userId);
            }

            MessageTimes.Remove(firstMessageUp.MessageId);

            return firstMessageUp;
        }
    }
}