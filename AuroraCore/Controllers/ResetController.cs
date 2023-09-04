using Aurora;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Web.Http;

namespace AuroraCore.Controllers
{
    [Route("/[controller]")]
    public class ResetController : Controller
    {
        IHostApplicationLifetime applicationLifetime;

        public ResetController(IHostApplicationLifetime appLifetime)
        {
            applicationLifetime = appLifetime;
        }
        [HttpGet("")]
        public bool Reset()
        {
            applicationLifetime.StopApplication();
            return true;
        }
    }
}
