using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chatbot.Controllers;

/// <summary>
/// Internal Support Desk chat: staff ask natural-language questions about
/// orders; answers are grounded in the order semantic-search index. No
/// conversation persistence — the client resends prior turns as history.
/// </summary>
public class SupportDeskController(IOrderSupportChatService supportChat) : Controller
{
    public IActionResult Index()
    {
        ViewData["CustomerDetailQuestionTemplate"] = SupportFaqKnowledge.CustomerDetailQuestionTemplate;
        ViewData["CurrentStatusQuestionTemplate"] = SupportFaqKnowledge.CurrentStatusQuestionTemplate;
        return View();
    }

    [HttpGet]
    public IActionResult Faqs() =>
        Json(SupportFaqKnowledge.AllEntries.Select(e => new { question = e.DisplayQuestion, answer = e.Answer }));

    public class AskRequest
    {
        public string Question { get; set; } = string.Empty;
        public List<ChatTurn> History { get; set; } = [];
    }

    public class ChatTurn
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        var history = request.History.Select(h => (h.Role, h.Content)).ToList();
        var result = await supportChat.AskAsync(request.Question, history, ct);

        return Json(new
        {
            answer = result.Answer,
            orders = result.RelatedOrders.Select(o => new
            {
                o.Id,
                o.CustomerName,
                o.OrderDate,
                o.Status,
                o.TotalAmount
            })
        });
    }
}
