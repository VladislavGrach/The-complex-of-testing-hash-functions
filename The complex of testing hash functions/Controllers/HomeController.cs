using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using The_complex_of_testing_hash_functions.Models;

namespace The_complex_of_testing_hash_functions.Controllers
{
    public class HomeController : Controller
    {
        private readonly HashTestingContext _context;
        public HomeController(HashTestingContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var hashFunctions = _context.HashFunctions.ToList();
            return View(hashFunctions);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
