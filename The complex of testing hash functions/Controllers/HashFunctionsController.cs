using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using The_complex_of_testing_hash_functions.Models;
using The_complex_of_testing_hash_functions.Services;

namespace The_complex_of_testing_hash_functions.Controllers
{
    public class HashFunctionsController : Controller
    {
        private readonly HashTestingContext _context;
        private readonly TestingService _randomnessTestingService;

        public HashFunctionsController(HashTestingContext context)
        {
            _context = context;
            _randomnessTestingService = new TestingService();
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.HashFunctions.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FirstOrDefaultAsync(m => m.Id == id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AlgorithmType,Description")] HashFunction hashFunction)
        {
            if (ModelState.IsValid)
            {
                _context.HashFunctions.Add(hashFunction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hashFunction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FindAsync(id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, HashFunction hashFunction)
        {
            if (id != hashFunction.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _context.Update(hashFunction);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(hashFunction);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FirstOrDefaultAsync(m => m.Id == id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hashFunction = await _context.HashFunctions.FindAsync(id);
            if (hashFunction != null)
            {
                _context.HashFunctions.Remove(hashFunction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HashFunctionExists(int id)
        {
            return _context.HashFunctions.Any(e => e.Id == id);
        }

        public async Task<IActionResult> TestRandomness()
        {
            return await TestRandomnessPage(); // Загружаем страницу с хеш-функциями
        }

        [HttpPost]
        public async Task<IActionResult> TestRandomness(int hashFunctionId, string hash, string[] tests)
        {
            // Загружаем список хеш-функций
            ViewBag.HashFunctions = await _context.HashFunctions.ToListAsync();

            if (string.IsNullOrEmpty(hash))
            {
                return View("TestsPage");
            }

            string binaryHash = _randomnessTestingService.ConvertHexToBinary(hash);
            var hashFunction = await _context.HashFunctions.FindAsync(hashFunctionId);
            if (hashFunction == null)
            {
                return NotFound("Хеш-функция не найдена!");
            }

            var results = new List<TestResult>();

            if (tests.Contains("Monobit"))
            {
                double score = _randomnessTestingService.MonobitTest(binaryHash);
                results.Add(new TestResult
                {
                    HashFunctionId = hashFunctionId,
                    TestType = "Монобит-тест",
                    Score = score
                });
            }

            if (tests.Contains("BlockFrequency"))
            {
                double score = _randomnessTestingService.BlockFrequencyTest(binaryHash);
                results.Add(new TestResult
                {
                    HashFunctionId = hashFunctionId,
                    TestType = "Тест на частоту в блоках",
                    Score = score
                });
            }

            if (tests.Contains("Poker"))
            {
                double score = _randomnessTestingService.PokerTest(binaryHash);
                results.Add(new TestResult
                {
                    HashFunctionId = hashFunctionId,
                    TestType = "Покер-тест",
                    Score = score
                });
            }

            _context.TestResults.AddRange(results);
            await _context.SaveChangesAsync();

            ViewBag.Hash = hash;
            ViewBag.BinaryHash = binaryHash;
            ViewBag.Results = results;
            ViewBag.HashFunctionName = hashFunction.Name;

            return View("TestsPage");
        }
        public async Task<IActionResult> TestRandomnessPage()
        {
            var functions = await _context.HashFunctions.ToListAsync();

            if (functions == null || functions.Count == 0)
            {
                ViewBag.Message = "Нет доступных хеш-функций!";
                ViewBag.HashFunctions = new List<HashFunction>(); // Пустой список, чтобы не было null
            }
            else
            {
                ViewBag.HashFunctions = functions;
            }

            return View("TestsPage");
        }
    }
}
