using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebhookReceiver.Models;
using WebhookReceiver.Services;

namespace WebhookReceiver.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class HomeController : Controller
    {
        private readonly WebhookServices _webhookServices;

        private readonly ILogger<HomeController> _logger;
        
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, WebhookServices webhookServices, IConfiguration configuration)
        {
            _webhookServices = webhookServices;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("alerts")]
        public async Task<IActionResult> Index([FromBody] JObject payload)
        {
            if (payload == null)
            {
                return BadRequest("Invalid Payload");
            }

            string jsonData=payload.ToString();
            _logger.LogInformation($"Received Alert Webhook: {jsonData}");


            var alertName = payload["data"]?["context"]?["name"]?.ToString();
            var severity = payload["data"]?["context"]?["severity"]?.ToString();
            var resource = payload["data"]?["context"]?["resource"]?.ToString();
            var description = payload["data"]?["context"]?["description"]?.ToString();
            var timeStamp = payload["data"]?["context"]?["timestamp"]?.ToString();

            if (string.IsNullOrWhiteSpace(alertName) || string.IsNullOrWhiteSpace(severity) || string.IsNullOrWhiteSpace(resource) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(timeStamp))
            {
                return BadRequest("Missing necessary payload data");
            }

            var alert = new Alert()
            {
                AlertName = alertName,
                Severity = severity,
                Resource = resource,
                Description = description,
                TimeStamp = DateTime.Parse(timeStamp)
            };

           var result= await _webhookServices.SaveAlert(alert);
            if (result)
            {
               return Ok(new { Message= "Alert processed and stored successfylly" });
            }
            return BadRequest(new { Message = "Unable to process Alert " });
        }

        [HttpPost("ReceiveGithubWebHook")]
        public async Task<IActionResult> ReceiveGithubPushWebHook([FromBody] JObject payload,
            [FromHeader(Name = "X-Hub-Signature-256")] string? signature)
        {
            if (payload == null|| string.IsNullOrEmpty(signature))
            {
                return BadRequest("Invalid payload or missing signature");
            }
            string jsonResult = payload.ToString();

            _logger.LogInformation($"Recieved Invalid Github Webhook: {jsonResult}");
            string secretKey = _configuration["GitHubSecret"];
            if (!_webhookServices.ValidateSignature(jsonResult, signature, secretKey))
            {
                return Unauthorized("Invalid Github Signature");
            }

            var repoName = payload["repository"]?["full_name"]?.ToString();
            var pusher= payload["pusher"]?["name"]?.ToString();
            var commitMessages= payload["commits"]?.Select(c=> c["Message"]?.ToString()).ToList();


            if (string.IsNullOrWhiteSpace(repoName) || string.IsNullOrWhiteSpace(pusher) || commitMessages == null)
            {
                return BadRequest("Missing Necessary Payload Data");
            }

            return Ok(new {Message="Github push event processed"});
           

        }
    }

}
