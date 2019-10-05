using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoApp.Models;
using Microsoft.AspNetCore.Http;
 
namespace MongoApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly MobileContext db;
        public HomeController(MobileContext context)
        {
            db = context;
        }
        public async Task<IActionResult> Index(FilterViewModel filter)
        {
            var phones = await db.GetPhones(filter.MinPrice, filter.MaxPrice, filter.Name);
            var model = new IndexViewModel { Phones = phones, Filter = filter };
            return View(model);
        }
 
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Phone p)
        {
            if (ModelState.IsValid)
            {
                await db.Create(p);
                 return RedirectToAction("Index");
            }
            return View(p);
        }
    }
}