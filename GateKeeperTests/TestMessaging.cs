using GateKeeper.Server.Models;
using GateKeeper_t = GateKeeper.Server.Models.GateKeeper;

namespace GateKeeperTests
{
    [TestClass]
    public sealed class UnitTestsMessaging
    {
        [TestMethod]
        public void TestPerAccountLimit()
        {
            GateKeeper_t testGateKeeper = new GateKeeper_t();
            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            testGateKeeper.AddAccount(account1);

            Guid userId = account1.Users[0].UserId;
            int limit = GateKeeper_t.PER_ACCOUNT * 2;

            List<Message> messages = new List<Message>();

            for (int i = 0; i < limit; i++)
            {
                Message message =
                    new Message(account1.AccountId, userId, ("Message" + i.ToString()));
                messages.Add(message);
            }

            for (int i = 0; i < messages.Count(); i++)
            {
                string testResult = testGateKeeper.AttemptSendSMSMessage(
                    account1.AccountId, userId, messages[i]);

                if (i <= GateKeeper_t.PER_ACCOUNT)
                {
                    Assert.AreEqual(String.Empty, testResult);
                }
                else
                {
                    Assert.AreEqual(GateKeeper_t.PER_ACCOUNT_EXCEEDED, testResult);
                }
            }
        }
    }
}
