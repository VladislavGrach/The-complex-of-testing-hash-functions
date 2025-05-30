using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using The_complex_of_testing_hash_functions.Interfaces;
using The_complex_of_testing_hash_functions.Models;
using The_complex_of_testing_hash_functions.Services;

namespace The_complex_of_testing_hash_functions.Controllers
{
    public class HashFunctionsController : Controller
    {
        private readonly HashTestingContext _context;
        private readonly NistTestingService _nistService;
        private readonly IDiehardTestingService _diehardService;
        private readonly ILogger<HashFunctionsController> _logger;

        public HashFunctionsController(
            HashTestingContext context,
            INistTestingService nistService,
            IDiehardTestingService diehardService,
            ILogger<HashFunctionsController> logger)
        {
            _context = context;
            _nistService = (NistTestingService?)nistService;
            _diehardService = diehardService;
            _logger = logger;
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
            return await ListOfHashFunctions(); // Загружаем страницу с хэш-функциями
        }

        [HttpPost]
        public async Task<IActionResult> TestRandomness(int hashFunctionId, string hash, string[] tests)
        {
            if (string.IsNullOrEmpty(hash) || tests == null || tests.Length == 0)
            {
                ViewBag.Error = "Введите хэш и выберите хотя бы один тест!";
                return await ListOfHashFunctions();  // Перезагружаем страницу с сообщением об ошибке
            }

            string binaryHash;
            try
            {
                binaryHash = _nistService.ConvertHexToBinary(hash);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка при преобразовании хэша: {ex.Message}";
                return await ListOfHashFunctions();
            }

            var hashFunction = await _context.HashFunctions.FindAsync(hashFunctionId);
            if (hashFunction == null)
            {
                ViewBag.Error = "Выбранная хэш-функция не найдена!";
                return await ListOfHashFunctions();
            }

            var results = new List<TestResult>();

            void AddTestResult(string testName, double score)
            {
                if (double.IsFinite(score))
                {
                    results.Add(new TestResult
                    {
                        HashFunctionId = hashFunctionId,
                        TestType = testName,
                        Score = score
                    });
                }
                else
                {
                    Console.WriteLine($"⚠️ Результат {testName} отброшен, так как score = {score}");
                }
            }

            try
            {
                // Применяем тесты
                if (tests.Contains("Monobit"))
                {
                    _logger.LogInformation("✅ 1 - Монобит-тест запущен");
                    AddTestResult("Монобит-тест", _nistService.MonobitTest(binaryHash));
                }

                if (tests.Contains("FrequencyWithinBlock"))
                {
                    _logger.LogInformation("✅ 2 - Тест на частоту в блоках запущен");
                    AddTestResult("Тест на частоту в блоках", _nistService.FrequencyTestWithinBlock(binaryHash));
                }

                if (tests.Contains("Runs"))
                {
                    _logger.LogInformation("✅ 3 - Тест на серийность запущен");
                    AddTestResult("Тест на серийность", _nistService.RunsTest(binaryHash));
                }

                if (tests.Contains("LongestRunOfOnes"))
                {
                    _logger.LogInformation("✅ 4 - Тест на самую длинную последовательность единиц запущен");
                    AddTestResult("Тест на самую длинную последовательность единиц", _nistService.LongestRunOfOnesTest(binaryHash));
                }

                if (tests.Contains("BinaryMatrixRank"))
                {
                    _logger.LogInformation("✅ 5 - Тест ранга бинарной матрицы запущен");
                    AddTestResult("Тест ранга бинарной матрицы", _nistService.BinaryMatrixRankTest(binaryHash));
                }


                if (tests.Contains("DiscreteFourierTransformTest"))
                {
                    _logger.LogInformation("✅ 6 - Дискретное преобразование Фурье запущен");
                    AddTestResult("Дискретное преобразование Фурье", _nistService.DiscreteFourierTransformTest(binaryHash));
                }


                if (tests.Contains("NonOverlappingTemplateMatching"))
                {
                    _logger.LogInformation("✅ 7 - Тест на несовпадающие шаблоны запущен");
                    AddTestResult("Тест на несовпадающие шаблоны", _nistService.NonOverlappingTemplateMatchingTest(binaryHash));
                }


                if (tests.Contains("OverlappingTemplateMatching"))
                {
                    _logger.LogInformation("✅ 8 - Тест на совпадающие шаблоны запущен");
                    AddTestResult("Тест на совпадающие шаблоны", _nistService.OverlappingTemplateMatchingTest(binaryHash));
                }


                if (tests.Contains("MaurersUniversal"))
                {
                    _logger.LogInformation("✅ 9 - Универсальный тест Маурера запущен");
                    AddTestResult("Универсальный тест Маурера", _nistService.MaurersUniversalTest(binaryHash));
                }


                if (tests.Contains("LinearComplexity"))
                {
                    _logger.LogInformation("✅ 10 - Тест линейной сложности запущен");
                    AddTestResult("Тест линейной сложности", _nistService.LinearComplexityTest(binaryHash));
                }


                if (tests.Contains("Serial"))
                {
                    _logger.LogInformation("✅ 11 - Серийный тест запущен");
                    AddTestResult("Серийный тест", _nistService.SerialTest(binaryHash));
                }


                if (tests.Contains("ApproximateEntropy"))
                {
                    _logger.LogInformation("✅ 12 - Тест приближенной энтропии запущен");
                    AddTestResult("Тест приближенной энтропии", _nistService.ApproximateEntropyTest(binaryHash));
                }


                if (tests.Contains("CusumTest"))
                {
                    _logger.LogInformation("✅ 13 - Тест накопленной суммы запущен");
                    AddTestResult("Тест накопленной суммы", _nistService.CusumTest(binaryHash));
                }


                if (tests.Contains("RandomExcursions"))
                {
                    _logger.LogInformation("✅ 14 - Тест случайных экскурсий запущен");
                    AddTestResult("Тест случайных экскурсий", _nistService.RandomExcursionsTest(binaryHash));
                }

                if (tests.Contains("RandomExcursionsVariant"))
                {
                    _logger.LogInformation("✅ 15 - Тест вариантов случайных экскурсий запущен");
                    AddTestResult("Тест вариантов случайных экскурсий", _nistService.RandomExcursionsVariantTest(binaryHash));
                }

                if (tests.Contains("LempelZivCompression"))
                {
                    _logger.LogInformation("✅ 16 - Тест Лемпеля-Зива запущен");
                    AddTestResult("Тест Лемпеля-Зива", _nistService.LempelZivCompressionTest(binaryHash));
                }

                if (tests.Contains("BirthdaySpacings"))
                {
                    _logger.LogInformation("✅ 17 - Тест дней рождения запущен");
                    AddTestResult("Тест дней рождения", _diehardService.BirthdaySpacingsTest(binaryHash));
                }

                if (tests.Contains("CountOnes"))
                {
                    _logger.LogInformation("✅ 18 - Тест подсчёта единиц запущен");
                    AddTestResult("Тест подсчёта единиц", _diehardService.CountOnesTest(binaryHash));
                }

                if (tests.Contains("RanksOfMatrices"))
                {
                    _logger.LogInformation("✅ 19 - Тест рангов матриц запущен");
                    AddTestResult("Тест рангов матриц", _diehardService.RanksOfMatricesTest(binaryHash));
                }

                if (tests.Contains("OverlappingPermutations"))
                {
                    _logger.LogInformation("✅ 20 - Тест на перестановки запущен");
                    AddTestResult("Тест на перестановки", _diehardService.OverlappingPermutationsTest(binaryHash));
                }

                if (tests.Contains("RunsDiehard"))
                {
                    _logger.LogInformation("✅ 21 - Тест серийности запущен");
                    AddTestResult("Тест серийности", _diehardService.RunsTest(binaryHash));
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка выполнения тестов: {ex.Message}";
                return await ListOfHashFunctions();
            }

            if (results.Any())
            {
                _context.TestResults.AddRange(results);
                await _context.SaveChangesAsync();
            }
            else
            {
                ViewBag.Error = "⚠️ Все тесты вернули некорректные значения! Данные не были сохранены.";
            }

            // Передаём данные во ViewBag для отображения в интерфейсе
            ViewBag.Results = results;
            ViewBag.Hash = hash;
            ViewBag.BinaryHash = binaryHash;
            ViewBag.HashFunctionName = hashFunction.Name;
            ViewBag.HashFunctions = await _context.HashFunctions.ToListAsync();

            return View("TestsPage");
        }

        public async Task<IActionResult> ListOfHashFunctions()
        {
            var functions = await _context.HashFunctions.ToListAsync();

            if (functions == null || functions.Count == 0)
            {
                ViewBag.Message = "Нет доступных хэш-функций!";
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
