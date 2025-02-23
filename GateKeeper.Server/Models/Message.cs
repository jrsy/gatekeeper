namespace GateKeeper.Server.Models
{
    public class Message
    {
        public Guid MessageId { get; set; }
        public Guid AccountId { get; set; }
        public Guid UserId { get; set; }

        public string Data { get; set; }
        public DateTime Timestamp { get; set; }

        public Message(Guid accountId, Guid userId, string data)
        {
            MessageId = Guid.NewGuid();
            AccountId = accountId;
            UserId = userId;
            Data = data;
            Timestamp = DateTime.Now;
        }
    }
}