using GateKeeper.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace GateKeeper.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class GateKeeperController : ControllerBase
{
    private readonly ILogger<GateKeeperController> _logger;
    private Models.GateKeeper _gateKeeper;

    public GateKeeperController(ILogger<GateKeeperController> logger)
    {
        _logger = logger;
        _gateKeeper = new Models.GateKeeper();
    }

    public void CallImaginaryExternalAPI(Message message)
    {
        // "Send" message
    }

    [HttpGet]
    public Account[] Get()
    {
        return _gateKeeper.GetAccounts();
    }

    [HttpPost]
    [Route("sendmessage")]
    public string Post(Guid accountId, Guid userId, Message message)
    {
        Account account = _gateKeeper.GetAccount(accountId);
        User user = account.Users
            .Where(u => u.UserId == userId)
            .Select(u => u).First();
        string response = _gateKeeper.AttemptSendSMSMessage(accountId, userId, message);
        if (response == String.Empty)
        {
            // Send immediately
            Message firstMessageUp = _gateKeeper.GetMessageFromQueue(accountId, userId);
            CallImaginaryExternalAPI(firstMessageUp);

            if (firstMessageUp.MessageId != message.MessageId)
            {
                response = "Message queued.";
            }
            else
            {
                CallImaginaryExternalAPI(firstMessageUp);
                response = "Message sent.";
            }
        }

        return response;
    }
}
