namespace GateKeeper.Server.Models
{
    public class Account
    {
        public Guid AccountId { get; set; }
        public string AccountName { get; set; }
        public List<User> Users { get; set; }

        public Account(string accountName)
        {
            AccountId = Guid.NewGuid();
            AccountName = accountName;
            Users = new List<User>();
        }

        public void AddUser(string userName, string phoneNumber)
        {
            User newUser = new User(AccountId, userName, phoneNumber);
            Users.Add(newUser);
        }

        public void AddUser(User user)
        {
            User newUser = new User(AccountId, user.UserName, user.PhoneNumber);
            Users.Add(newUser);
        }
    }
}