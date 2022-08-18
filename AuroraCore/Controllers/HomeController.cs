using Microsoft.AspNetCore.Mvc;

namespace AuroraCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
