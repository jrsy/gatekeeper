namespace GateKeeper.Server.Models
{
    public class Account
    {
        public Guid AccountId { get; set; }
        public string AccountName { get; set; }
        public Dictionary<Guid, User> Users { get; set; }

        public Account(string accountName)
        {
            AccountId = Guid.NewGuid();
            AccountName = accountName;
            Users = new Dictionary<Guid, User>();
        }

        public void AddUser(string userName, string phoneNumber)
        {
            User newUser = new User(AccountId, userName, phoneNumber);
            Users.Add(newUser.UserId, newUser);
        }
    }
}