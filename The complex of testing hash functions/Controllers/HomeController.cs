using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using The_complex_of_testing_hash_functions.Interfaces;
using The_complex_of_testing_hash_functions.Models;


namespace The_complex_of_testing_hash_functions.Controllers
{
    public class HomeController : Controller
    {
        //private readonly HashTestingContext _context;
        //private readonly ILogger<HomeController> _logger;
        //public HomeController(HashTestingContext context)
        //{
        //    _context = context;
        //}
        private readonly HashTestingContext _context;
        private readonly INistTestingService _nistService;
        private readonly IDiehardTestingService _diehardService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            HashTestingContext context,
            INistTestingService nistService,
            IDiehardTestingService diehardService,
            ILogger<HomeController> logger)
        {
            _context = context;
            _nistService = nistService;
            _diehardService = diehardService;
            _logger = logger;
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
