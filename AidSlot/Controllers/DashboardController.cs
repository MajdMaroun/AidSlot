using Microsoft.AspNetCore.Mvc;
namespace AidSlot.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}