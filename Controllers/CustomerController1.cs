using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBN.Controllers
{
    [Authorize]
    public class CustomerController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
