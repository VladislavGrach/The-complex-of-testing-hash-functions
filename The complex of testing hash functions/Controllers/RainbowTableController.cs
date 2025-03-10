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

            SaveTestResult(hashFunction.Id, "Генерация радужной таблицы", chainLength * tableSize);

            return RedirectToAction("Index");
        }

        public IActionResult Search()
        {
            ViewBag.HashFunctions = _context.HashFunctions.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Search(string inputHash, int hashFunctionId)
        {
            var result = _rainbowTableService.FindPasswordByHash(inputHash, hashFunctionId);

            if (result == null)
            {
                ViewBag.Message = "Данные не найдены в радужной таблице.";
                SaveTestResult(hashFunctionId, "Поиск пароля", 0);
            }
            else
            {
                ViewBag.Message = "Пароль найден!";
                ViewBag.PlainText = result.PlainText;
                ViewBag.ChainLength = result.ChainLength;

                SaveTestResult(hashFunctionId, "Поиск пароля", 1); // Записываем просто 1, раз попытки не считаем
            }

            ViewBag.HashFunctions = _context.HashFunctions.ToList();
            return View();
        }

        private void SaveTestResult(int hashFunctionId, string testType, double attempts)
        {
            var testResult = new TestResult
            {
                HashFunctionId = hashFunctionId,
                TestType = testType,
                Score = attempts, // Количество попыток поиска (чем больше, тем устойчивее хеш)
                TestDate = DateTime.UtcNow
            };

            _context.TestResults.Add(testResult);
            _context.SaveChanges();
        }

        public IActionResult Delete(int id)
        {
            var table = _context.RainbowTables.Find(id);
            if (table == null)
            {
                return NotFound();
            }
            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var table = _context.RainbowTables.Find(id);
            if (table != null)
            {
                _context.RainbowTables.Remove(table);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var table = _context.RainbowTables
                .Include(r => r.HashFunction)
                .FirstOrDefault(m => m.Id == id);

            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }
    }
}
