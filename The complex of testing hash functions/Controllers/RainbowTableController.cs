using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using The_complex_of_testing_hash_functions.Models;
using The_complex_of_testing_hash_functions.Services;

namespace The_complex_of_testing_hash_functions.Controllers
{
    public class RainbowTableController : Controller
    {
        private readonly HashTestingContext _context;
        private readonly RainbowTableService _rainbowTableService;

        public RainbowTableController(HashTestingContext context, RainbowTableService rainbowTableService)
        {
            _context = context;
            _rainbowTableService = rainbowTableService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.RainbowTables.Include(rt => rt.HashFunction).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.HashFunctions = _context.HashFunctions.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(string? algorithmType, int chainLength, int tableSize)
        {
            if (string.IsNullOrEmpty(algorithmType))
            {
                return BadRequest("Не выбрана хеш-функция.");
            }

            string NormalizeAlgorithmName(string algorithm)
            {
                return algorithm.Replace("-", "").ToUpper();
            }

            var normalizedAlgorithm = NormalizeAlgorithmName(algorithmType);

            var hashFunction = _context.HashFunctions
                .AsEnumerable()
                .FirstOrDefault(h => NormalizeAlgorithmName(h.AlgorithmType) == normalizedAlgorithm);

            if (hashFunction == null)
            {
                return BadRequest($"Выбрана неверная хеш-функция: {algorithmType}");
            }

            _rainbowTableService.GenerateRainbowTable(hashFunction.Id, chainLength, tableSize);
            return RedirectToAction("Index");
        }
    }
}
