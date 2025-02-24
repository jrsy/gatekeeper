using GateKeeper.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace GateKeeper.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class GateKeeperController : ControllerBase
{
    private readonly ILogger<GateKeeperController> _logger;
    private static Models.GateKeeper _gateKeeper = new Models.GateKeeper();

    public struct SendMessageResponse
    {
        public string Response;
        public Guid MessageId;
        public DateTime Timestamp;
    }

    public GateKeeperController(ILogger<GateKeeperController> logger)
    {
        _logger = logger;
    }

    public void CallImaginaryExternalAPI(Message message)
    {
        // "Send" message
        message.Sent = DateTime.Now;
    }

    [HttpGet]
    public Account[] Get()
    {
        Account[] accounts = _gateKeeper.GetAccounts();
        return accounts;
    }

    [HttpPost]
    [Route("testinitialize")]
    public int Post()
    {
        _gateKeeper = new Models.GateKeeper();
        return StatusCodes.Status200OK;
    }

    [HttpPost]
    [Route("addaccounttest")]
    public Guid Post(Account account)
    {
        _gateKeeper.AddAccount(account);
        return account.AccountId;
    }

    [HttpPost]
    [Route("addaccount")]
    public Guid Post([FromBody] string accountName)
    {
        Account account = new Account(accountName);
        _gateKeeper.AddAccount(account);
        return account.AccountId;
    }

    [HttpPost]
    [Route("adduser")]
    public Guid PostAddUser([FromBody] JObject data)
    {
        Guid accountId = data["accountId"].ToObject<Guid>();
        string userName = data["userName"].ToObject<string>();
        string phoneNumber = data["phoneNumber"].ToObject<string>();
        Account account = _gateKeeper.GetAccount(accountId);

        User newUser = new User(accountId, userName, phoneNumber);
        account.AddUser(newUser);

        return newUser.UserId;
    }

    [HttpPost]
    [Route("removeuser")]
    public int PostRemoveUser([FromBody] JObject data)
    {
        Guid accountId = data["accountId"].ToObject<Guid>();
        Guid userId = data["userId"].ToObject<Guid>();
        Account account = _gateKeeper.GetAccount(accountId);

        _gateKeeper.removeUser(accountId, userId);

        return StatusCodes.Status200OK;
    }

    [HttpPost]
    [Route("sendmessage")]
    public SendMessageResponse PostSendMessage([FromBody] JObject data)
    {
        Message message = data["message"].ToObject<Message>();
        if (message.GetType().GetProperty("MessageId") == null)
        {
            message.MessageId = Guid.NewGuid();
        }
        message.Timestamp = DateTime.Now;

        Guid accountId = message.AccountId;
        Guid userId = message.UserId;
        Account account = _gateKeeper.GetAccount(accountId);
        User user = account.Users
            .Where(u => u.UserId == userId)
            .Select(u => u).First();

        string response = _gateKeeper.AttemptSendSMSMessage(accountId, userId, message);
        if (response == String.Empty)
        {
            // Send immediately
            Message? firstMessageUp = _gateKeeper.GetMessageFromQueue(accountId, userId);
            if (firstMessageUp == null || firstMessageUp.MessageId != message.MessageId)
            {
                // Earlier message from same user - send that one first
                response = Models.GateKeeper.MESSAGE_QUEUED;
            }
            else
            {
                CallImaginaryExternalAPI(firstMessageUp);
                if (firstMessageUp.SendImmediately)
                {
                    _gateKeeper.RemoveMessageFromQueue(firstMessageUp);
                }
                
                response = Models.GateKeeper.MESSAGE_SENT;
            }
        }

        return new SendMessageResponse
        {
            Response = response,
            MessageId = message.MessageId,
            Timestamp = message.Timestamp
        };
    }
}
