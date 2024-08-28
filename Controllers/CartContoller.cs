using Microsoft.AspNetCore.Mvc;

namespace RolesAuth.Controllers
{
    public class CartContoller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
