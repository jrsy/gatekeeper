using GateKeeper.Server;
using GateKeeper.Server.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;
using GateKeeper_t = GateKeeper.Server.Models.GateKeeper;
using Newtonsoft.Json;

namespace GateKeeperTests
{
    [TestClass]
    public sealed class UnitTestsController
    {
        [TestInitialize()]
        public async Task Initialize()
        {
            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();
            await client.PostAsync("/gatekeeper/testinitialize", null);
        }

        [TestMethod]
        public async Task TestSendMessage()
        {
            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();

            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");

            var response = await client.PostAsJsonAsync("/gatekeeper/addaccounttest", account1);
            response.EnsureSuccessStatusCode();

            Guid userId = account1.Users[0].UserId;
            List<Message> messages = new List<Message>();

            for (int i = 0; i < 2; i++)
            {
                Message message =
                    new Message(account1.AccountId, userId, ("Message" + i.ToString()));
                message.SendImmediately = false;
                messages.Add(message);
            }

            var response1 = await client.PostAsJsonAsync("/gatekeeper/sendmessage",
                new
                {
                    message = messages[0]
                });
            var response2 = await client.PostAsJsonAsync("/gatekeeper/sendmessage",
                new
                {
                    message = messages[1]
                });

            response1.EnsureSuccessStatusCode();
            response2.EnsureSuccessStatusCode();
            
            string responseString1 = await response1.Content.ReadAsStringAsync();
            string responseString2 = await response2.Content.ReadAsStringAsync();

            Assert.AreEqual(GateKeeper_t.MESSAGE_SENT, responseString1);
            Assert.AreEqual(GateKeeper_t.MESSAGE_QUEUED, responseString2);
        }

        [TestMethod]
        public async Task TestSendMessagePerAccountLimit()
        {
            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();

            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");
            account1.AddUser("User 1.2", "111-1112");
            account1.AddUser("User 1.3", "111-1113");
            account1.AddUser("User 1.4", "111-1114");
            account1.AddUser("User 1.5", "111-1115");
            account1.AddUser("User 1.6", "111-1116");

            int userIndex = 0;

            int limit = GateKeeper_t.PER_ACCOUNT * 2;

            var postResponse = await client.PostAsJsonAsync("/gatekeeper/addaccounttest", account1);
            postResponse.EnsureSuccessStatusCode();

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
                message.SendImmediately = false;
                messages.Add(message);
            }

            List<HttpResponseMessage?> responses = new List<HttpResponseMessage?>();
            for (int i = 0; i < messages.Count(); i++)
            {
                Guid userId = messages[i].UserId;
                var response = await client.PostAsJsonAsync("/gatekeeper/sendmessage",
                    new
                    {
                        message = messages[i]
                    });
            }

            for (int i = 0; i < responses.Count(); i++)
            {
                var result = responses[i];
                result.EnsureSuccessStatusCode();
                string responseString = await result.Content.ReadAsStringAsync();

                if (i <= GateKeeper_t.PER_ACCOUNT)
                {
                    Assert.AreNotEqual(GateKeeper_t.PER_ACCOUNT_EXCEEDED, responseString);
                }
                else
                {
                    Assert.AreEqual(GateKeeper_t.PER_ACCOUNT_EXCEEDED, responseString);
                }
            }
        }

        [TestMethod]
        public async Task TestSendMessagePerUserLimit()
        {
            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();

            Account account1 = new Account("Account 1");
            account1.AddUser("User 1.1", "111-1111");

            int userIndex = 0;

            int limit = GateKeeper_t.PER_PHONE * 2;

            var postResponse = await client.PostAsJsonAsync("/gatekeeper/addaccounttest", account1);
            postResponse.EnsureSuccessStatusCode();

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
                message.SendImmediately = false;
                messages.Add(message);
            }

            List<HttpResponseMessage?> responses = new List<HttpResponseMessage?>();
            for (int i = 0; i < messages.Count(); i++)
            {
                Guid userId = messages[i].UserId;
                var response = await client.PostAsJsonAsync("/gatekeeper/sendmessage",
                    new
                    {
                        message = messages[i]
                    });
                responses.Add(response);
            }

            for (int i = 0; i < responses.Count(); i++)
            {
                var result = responses[i];
                result.EnsureSuccessStatusCode();
                string responseString = await result.Content.ReadAsStringAsync();

                if (i <= GateKeeper_t.PER_PHONE)
                {
                    Assert.AreNotEqual(GateKeeper_t.PER_PHONE_EXCEEDED, responseString);
                }
                else
                {
                    Assert.AreEqual(GateKeeper_t.PER_PHONE_EXCEEDED, responseString);
                }
            }
        }
    }
}
