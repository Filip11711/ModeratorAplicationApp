using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ModeratorAplicationApp.Models;
using ModeratorAplicationApp.Util;

namespace ModeratorAplicationApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private const string ModeratorsGroup = "Moderators";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(string user)
        {
            AktivnostiPodaci podaci = new AktivnostiPodaci();
            podaci.ProcessInstances = await CamundaUtil.GetAplications();
            podaci.MyTasks = await CamundaUtil.GetTasks(user);
            if (await CamundaUtil.IsUserInGroup(user, ModeratorsGroup))
            {
                foreach (var active in podaci.ActiveAplications)
                {
                    active.CurrentUserInGroup = true;
                }
            }

            return View(podaci);
        }

        [HttpGet]
        public IActionResult Start(string user)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Start(string user, int id, string email, string languageKnowledge, string motivationalLetter)
        {
            var pid = await CamundaUtil.StartAplicationProcess(id, user, email, languageKnowledge, motivationalLetter);

            return RedirectToAction(nameof(Index), new { user });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyForQuestioning(string user, string pid)
        {
            await CamundaUtil.ApplyForQuestioning(pid, user);
            return RedirectToAction(nameof(Index), new { user });
        }

        [HttpPost]
        public async Task<IActionResult> PickTask(string user, string taskId)
        {
            await CamundaUtil.PickTask(taskId, user);
            return RedirectToAction(nameof(Index), new { user });
        }

        [HttpPost]
        public async Task<IActionResult> FinishTask(string user, string taskId)
        {
            await CamundaUtil.FinishTask(taskId);
            return RedirectToAction(nameof(Index), new { user });
        }

        [HttpPost]
        public async Task<IActionResult> FinishInitialReview(string user, string taskId, bool passed)
        {
            await CamundaUtil.FinishInitialReview(taskId, passed);
            return RedirectToAction(nameof(Index), new { user });
        }

        [HttpPost]
        public async Task<IActionResult> AssignModerator(string user, string moderator, string taskId)
        {
            await CamundaUtil.AssignModerator(taskId, moderator);
            return RedirectToAction(nameof(Index), new { user });
        }

        [HttpPost]
        public async Task<IActionResult> SendQuestionnaire(string user, string taskId, string question1, string question2, string question3)
        {
            await CamundaUtil.SendQuestionnaire(taskId, question1, question2, question3);
            return RedirectToAction(nameof(Index), new { user });
        }

        [HttpPost]
        public async Task<IActionResult> SendSolvedQuestionnaire(string user, string taskId, string answer1, string answer2, string answer3)
        {
            await CamundaUtil.SendSolvedQuestionnaire(taskId, answer1, answer2, answer3);
            return RedirectToAction(nameof(Index), new { user });
        }

        [HttpPost]
        public async Task<IActionResult> FinishFinalReview(string user, string taskId, bool passed)
        {
            await CamundaUtil.FinishFinalReview(taskId, passed);
            return RedirectToAction(nameof(Index), new { user });
        }

        public async Task<ActionResult<string>> Diagram()
        {
            var xml = await CamundaUtil.GetXmlDefinition();
            return xml;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
