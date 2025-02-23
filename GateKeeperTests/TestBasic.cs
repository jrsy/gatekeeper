using GateKeeper.Server.Models;
using GateKeeper_t = GateKeeper.Server.Models.GateKeeper;

namespace GateKeeperTests
{
    [TestClass]
    public sealed class UnitTestsBasic
    {
        [TestMethod]
        public void TestGetAccounts()
        {
            GateKeeper_t testGateKeeper = new GateKeeper_t();
            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            account1.AddUser("User 1.2", "112-1112");
            account1.AddUser("User 1.3", "113-1113");

            Account account2 = new Account("Account 2");
            account2.AddUser("User 2.1", "221-2221");
            account2.AddUser("User 2.2", "222-2222");
            testGateKeeper.AddAccount(account1);
            testGateKeeper.AddAccount(account2);

            Account[] expectedResults =
            {
                account1, account2
            };

            Account[] testResults = testGateKeeper.GetAccounts();
            testResults = testResults.OrderBy(a => a.AccountName).ToArray();
            for (int i = 0; i < testResults.Count(); i++)
            {
                Account expectedResult = expectedResults[i];
                Account testResult = testResults[i];
                Assert.AreEqual(expectedResult.AccountId, testResult.AccountId);
                Assert.AreEqual(expectedResult.AccountName, testResult.AccountName);

                for (int j = 0; j < testResult.Users.Count(); j++)
                {
                    User expectedUser = expectedResult.Users[j];
                    User testUser = testResult.Users[j];
                    Assert.AreEqual(expectedUser.UserId, testUser.UserId);
                    Assert.AreEqual(expectedUser.UserName, testUser.UserName);
                    Assert.AreEqual(expectedUser.PhoneNumber, testUser.PhoneNumber);
                }
            }
        }

        [TestMethod]
        public void TestGetAccount()
        {
            GateKeeper_t testGateKeeper = new GateKeeper_t();
            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            account1.AddUser("User 1.2", "112-1112");
            account1.AddUser("User 1.3", "113-1113");

            Account account2 = new Account("Account 2");
            account2.AddUser("User 2.1", "221-2221");
            account2.AddUser("User 2.2", "222-2222");
            testGateKeeper.AddAccount(account1);
            testGateKeeper.AddAccount(account2);

            Account testResult = testGateKeeper.GetAccount(account1.AccountId);
            Assert.AreEqual(account1.AccountId, testResult.AccountId);
            Assert.AreEqual(account1.AccountName, testResult.AccountName);
            for (int i = 0; i < testResult.Users.Count(); i++)
            {
                User expectedUser = account1.Users[i];
                User testUser = testResult.Users[i];
                Assert.AreEqual(expectedUser.UserId, testUser.UserId);
                Assert.AreEqual(expectedUser.UserName, testUser.UserName);
                Assert.AreEqual(expectedUser.PhoneNumber, testUser.PhoneNumber);
            }

            testResult = testGateKeeper.GetAccount(account2.AccountId);
            Assert.AreEqual(account2.AccountId, testResult.AccountId);
            Assert.AreEqual(account2.AccountName, testResult.AccountName);
            for (int i = 0; i < testResult.Users.Count(); i++)
            {
                User expectedUser = account2.Users[i];
                User testUser = testResult.Users[i];
                Assert.AreEqual(expectedUser.UserId, testUser.UserId);
                Assert.AreEqual(expectedUser.UserName, testUser.UserName);
                Assert.AreEqual(expectedUser.PhoneNumber, testUser.PhoneNumber);
            }
        }

        [TestMethod]
        public void TestAttemptSendSMSMessage()
        {
            GateKeeper_t testGateKeeper = new GateKeeper_t();
            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            account1.AddUser("User 1.2", "112-1112");
            account1.AddUser("User 1.3", "113-1113");
            testGateKeeper.AddAccount(account1);

            Message message1 =
                new Message(account1.AccountId, account1.Users[0].UserId, "User1");
            Message message2 =
                new Message(account1.AccountId, account1.Users[1].UserId, "User2");
            Message message3 =
                new Message(account1.AccountId, account1.Users[2].UserId, "User3");

            string testResult = testGateKeeper.AttemptSendSMSMessage(
                account1.AccountId, account1.Users[0].UserId, message1);
            Assert.AreEqual(String.Empty, testResult);

            testResult = testGateKeeper.AttemptSendSMSMessage(
                account1.AccountId, account1.Users[1].UserId, message2);
            Assert.AreEqual(String.Empty, testResult);

            testResult = testGateKeeper.AttemptSendSMSMessage(
                account1.AccountId, account1.Users[2].UserId, message3);
            Assert.AreEqual(String.Empty, testResult);
        }

        [TestMethod]
        public void TestGetMessageFromQueue()
        {
            GateKeeper_t testGateKeeper = new GateKeeper_t();
            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            testGateKeeper.AddAccount(account1);

            Guid userId = account1.Users[0].UserId;

            Message message =
                new Message(account1.AccountId, userId, "User1");

            string testResult = testGateKeeper.AttemptSendSMSMessage(
                account1.AccountId, userId, message);
            Assert.AreEqual(String.Empty, testResult);

            Message testMessage = testGateKeeper.GetMessageFromQueue(account1.AccountId, userId);
            Assert.AreEqual(message.MessageId, testMessage.MessageId);
            Assert.AreEqual(message.UserId, testMessage.UserId);
            Assert.AreEqual(message.Timestamp, testMessage.Timestamp);
            Assert.AreEqual(message.Data, testMessage.Data);
        }
    }
}
