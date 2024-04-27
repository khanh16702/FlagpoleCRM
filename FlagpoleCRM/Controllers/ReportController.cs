using Microsoft.AspNetCore.Mvc;

namespace FlagpoleCRM.Controllers
{
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
