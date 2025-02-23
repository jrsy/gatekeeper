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
            account1.AddUser("User 1.2", "111-1112");
            account1.AddUser("User 1.3", "111-1113");
            account1.AddUser("User 1.4", "111-1114");
            account1.AddUser("User 1.5", "111-1115");
            account1.AddUser("User 1.6", "111-1116");
            testGateKeeper.AddAccount(account1);

            int userIndex = 0;
            
            int limit = GateKeeper_t.PER_ACCOUNT * 2;

            List<Message> messages = new List<Message>();

            for (int i = 0; i < limit; i++)
            {
                Guid userId = account1.Users[userIndex].UserId;
                userIndex++;
                if (userIndex >= account1.Users.Count())
                {
                    userIndex = 0;
                }

                Message message =
                    new Message(account1.AccountId, userId, ("Message" + i.ToString()));
                messages.Add(message);
            }

            for (int i = 0; i < messages.Count(); i++)
            {
                Guid userId = messages[i].UserId;
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

        [TestMethod]
        public void TestPerUserLimit()
        {
            GateKeeper_t testGateKeeper = new GateKeeper_t();
            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            testGateKeeper.AddAccount(account1);

            Guid userId = account1.Users[0].UserId;
            int limit = GateKeeper_t.PER_PHONE * 2;

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

                if (i <= GateKeeper_t.PER_PHONE)
                {
                    Assert.AreEqual(String.Empty, testResult);
                }
                else
                {
                    Assert.AreEqual(GateKeeper_t.PER_PHONE_EXCEEDED, testResult);
                }
            }
        }

        [TestMethod]
        public void TestMessageQueue()
        {
            GateKeeper_t testGateKeeper = new GateKeeper_t();
            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            testGateKeeper.AddAccount(account1);

            Guid userId = account1.Users[0].UserId;
            List<Message> messages = new List<Message>();

            for (int i = 0; i < 2; i++)
            {
                Message message =
                    new Message(account1.AccountId, userId, ("Message" + i.ToString()));
                messages.Add(message);
                string testResult = testGateKeeper.AttemptSendSMSMessage(
                    account1.AccountId, userId, message);
            }

            for (int i = 0; i < messages.Count(); i++)
            {
                Message? testResult = testGateKeeper.GetMessageFromQueue(
                    account1.AccountId, userId);

                Assert.AreEqual(messages[i].MessageId, testResult.MessageId);

                testGateKeeper.RemoveMessageFromQueue(testResult);
            }
        }

        [TestMethod]
        public void TestMessageCleanup()
        {
            GateKeeper_t testGateKeeper = new GateKeeper_t();
            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            testGateKeeper.AddAccount(account1);

            Guid userId = account1.Users[0].UserId;
            List<Message> messages = new List<Message>();

            for (int i = 0; i < GateKeeper_t.PER_PHONE; i++)
            {
                Message message =
                    new Message(account1.AccountId, userId, ("Message" + i.ToString()));
                message.SendImmediately = false;
                messages.Add(message);
                string testResult = testGateKeeper.AttemptSendSMSMessage(
                    account1.AccountId, userId, message);
            }

            testGateKeeper.removeUser(account1.AccountId, userId);

            for (int i = 0; i < messages.Count(); i++)
            {
                Message? testResult = testGateKeeper.GetMessageFromQueue(
                    account1.AccountId, userId);

                Assert.AreEqual(null, testResult);
            }
        }
    }
}
