using GateKeeper.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace GateKeeper.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class GateKeeperController : ControllerBase
{
    private readonly ILogger<GateKeeperController> _logger;
    private static Models.GateKeeper _gateKeeper = new Models.GateKeeper();

    public GateKeeperController(ILogger<GateKeeperController> logger)
    {
        _logger = logger;
    }

    public void CallImaginaryExternalAPI(Message message)
    {
        // "Send" message
    }

    [HttpGet]
    public Account[] Get()
    {
        Account[] accounts = _gateKeeper.GetAccounts();
        return accounts;
    }

    [HttpPost]
    [Route("addaccount")]
    public int Post(Account account)
    {
        _gateKeeper.AddAccount(account);
        return StatusCodes.Status200OK;
    }

    [HttpPost]
    [Route("sendmessage")]
    public string Post(Message message)
    {
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

        return response;
    }
}
