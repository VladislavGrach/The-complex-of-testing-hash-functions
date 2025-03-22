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


        public HashFunctionsController(HashTestingContext context, INistTestingService nistService, IDiehardTestingService diehardService)
        {
            _context = context;
            _nistService = (NistTestingService?)nistService;
            _diehardService = diehardService;
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
            return await ListOfHashFunctions(); // Загружаем страницу с хеш-функциями
        }

        [HttpPost]
        public async Task<IActionResult> TestRandomness(int hashFunctionId, string hash, string[] tests)
        {
            if (string.IsNullOrEmpty(hash) || tests == null || tests.Length == 0)
            {
                ViewBag.Error = "Введите хеш и выберите хотя бы один тест!";
                return await ListOfHashFunctions();  // Перезагружаем страницу с сообщением об ошибке
            }

            string binaryHash;
            try
            {
                binaryHash = _nistService.ConvertHexToBinary(hash);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Ошибка при преобразовании хеша: {ex.Message}";
                return await ListOfHashFunctions();
            }

            var hashFunction = await _context.HashFunctions.FindAsync(hashFunctionId);
            if (hashFunction == null)
            {
                ViewBag.Error = "Выбранная хеш-функция не найдена!";
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
                    Console.WriteLine("✅ 1");
                    AddTestResult("Монобит-тест", _nistService.MonobitTest(binaryHash));
                }

                if (tests.Contains("FrequencyWithinBlock"))
                {
                    Console.WriteLine("✅ 2");
                    AddTestResult("Частотный тест в блоках", _nistService.FrequencyTestWithinBlock(binaryHash));
                }

                if (tests.Contains("Runs"))
                {
                    Console.WriteLine("✅ 3");
                    AddTestResult("Тест на серийность", _nistService.RunsTest(binaryHash));
                }

                if (tests.Contains("LongestRunOfOnes"))
                {
                    Console.WriteLine("✅ 4");
                    AddTestResult("Тест на самую длинную последовательность единиц", _nistService.LongestRunOfOnesTest(binaryHash));
                }

                if (tests.Contains("BinaryMatrixRank"))
                {
                    Console.WriteLine("✅ 5");
                    AddTestResult("Тест ранга бинарной матрицы", _nistService.BinaryMatrixRankTest(binaryHash));
                }


                if (tests.Contains("DiscreteFourierTransformTest"))
                {
                    Console.WriteLine("✅ 6");
                    AddTestResult("Дискретное преобразование Фурье", _nistService.DiscreteFourierTransformTest(binaryHash));
                }


                if (tests.Contains("NonOverlappingTemplateMatching"))
                {
                    Console.WriteLine("✅ 7");
                    AddTestResult("Тест на несовпадающие шаблоны", _nistService.NonOverlappingTemplateMatchingTest(binaryHash));
                }


                if (tests.Contains("OverlappingTemplateMatching"))
                {
                    Console.WriteLine("✅ 8");
                    AddTestResult("Тест на совпадающие шаблоны", _nistService.OverlappingTemplateMatchingTest(binaryHash));
                }


                if (tests.Contains("MaurersUniversal"))
                {
                    Console.WriteLine("✅ 9");
                    AddTestResult("Универсальный тест Маурера", _nistService.MaurersUniversalTest(binaryHash));
                }


                if (tests.Contains("LinearComplexity"))
                {
                    Console.WriteLine("✅ 10");
                    AddTestResult("Тест линейной сложности", _nistService.LinearComplexityTest(binaryHash));
                }


                if (tests.Contains("Serial"))
                {
                    Console.WriteLine("✅ 11");
                    AddTestResult("Серийный тест", _nistService.SerialTest(binaryHash));
                }


                if (tests.Contains("ApproximateEntropy"))
                {
                    Console.WriteLine("✅ 12");
                    AddTestResult("Тест приближенной энтропии", _nistService.ApproximateEntropyTest(binaryHash));
                }


                if (tests.Contains("CusumTest"))
                {
                    Console.WriteLine("✅ 13");
                    AddTestResult("Тест накопленной суммы (Cusum)", _nistService.CusumTest(binaryHash));
                }


                if (tests.Contains("RandomExcursions"))
                {
                    Console.WriteLine("✅ 14");
                    AddTestResult("Тест случайных экскурсий", _nistService.RandomExcursionsTest(binaryHash));
                }

                if (tests.Contains("RandomExcursionsVariant"))
                {
                    Console.WriteLine("✅ 15");
                    double score = _nistService.RandomExcursionsVariantTest(binaryHash).Values.Sum();
                    AddTestResult("Тест вариантов случайных экскурсий", score);
                }

                if (tests.Contains("LempelZivCompression"))
                {
                    Console.WriteLine("✅ 16");
                    AddTestResult("Тест Лемпеля-Зива", _nistService.LempelZivCompressionTest(binaryHash));
                }

                if (tests.Contains("BirthdaySpacings"))
                {
                    Console.WriteLine("✅ 17");
                    AddTestResult("Тест дней рождения", _diehardService.BirthdaySpacingsTest(binaryHash));
                }

                if (tests.Contains("CountOnes"))
                {
                    Console.WriteLine("✅ 18");
                    AddTestResult("Тест подсчёта единиц", _diehardService.CountOnesTest(binaryHash));
                }

                if (tests.Contains("RanksOfMatrices"))
                {
                    Console.WriteLine("✅ 19");
                    AddTestResult("Тест рангов матриц", _diehardService.RanksOfMatricesTest(binaryHash));
                }

                if (tests.Contains("OverlappingPermutations"))
                {
                    Console.WriteLine("✅ 20");
                    AddTestResult("Тест на перестановки", _diehardService.OverlappingPermutationsTest(binaryHash));
                }

                if (tests.Contains("RunsDiehard"))
                {
                    Console.WriteLine("✅ 21");
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
