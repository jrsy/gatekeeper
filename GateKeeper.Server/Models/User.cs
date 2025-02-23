namespace GateKeeper.Server.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }

        public User(Guid accountId, string userName, string phoneNumber)
        {
            UserId = Guid.NewGuid();
            AccountId = accountId;
            UserName = userName;
            PhoneNumber = phoneNumber;
        }
    }
}