using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using The_complex_of_testing_hash_functions.Models;
using SHA3.Net;

namespace The_complex_of_testing_hash_functions.Services
{
    public class RainbowTableService
    {
        private readonly HashTestingContext _context;
        private readonly List<string> _popularPasswords;

        public RainbowTableService(HashTestingContext context)
        {
            _context = context;
            _popularPasswords = LoadPopularPasswords();
        }

        private List<string> LoadPopularPasswords()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "popular_passwords.txt");
            return File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        }

        public RainbowTable GenerateRainbowTable(int hashFunctionId, int chainLength, int tableSize)
        {
            var hashFunction = _context.HashFunctions.FirstOrDefault(h => h.Id == hashFunctionId);
            if (hashFunction == null)
                throw new ArgumentException("Хеш-функция не найдена");

            // Проверяем, существует ли уже таблица для этой хеш-функции
            bool tableExists = _context.RainbowTables.Any(rt => rt.HashFunctionId == hashFunctionId);

            var rainbowTable = new RainbowTable
            {
                Name = $"{hashFunction.AlgorithmType}_Table",
                ChainLength = chainLength,
                TableSize = tableSize,
                HashFunctionId = hashFunctionId
            };

            _context.RainbowTables.Add(rainbowTable);
            _context.SaveChanges();

            List<RainbowEntry> entries = new List<RainbowEntry>();
            HashSet<string> usedHashes = new HashSet<string>();

            // Если таблицы не было, добавляем популярные пароли
            if (!tableExists)
            {
                foreach (var popularPassword in _popularPasswords.Take(tableSize))
                {
                    AddEntry(popularPassword, hashFunction.AlgorithmType, chainLength, rainbowTable.Id, entries, usedHashes, true);
                }
            }

            // Если места ещё остались, добавляем случайные пароли
            while (entries.Count < tableSize)
            {
                string plainText = GenerateRandomString();
                AddEntry(plainText, hashFunction.AlgorithmType, chainLength, rainbowTable.Id, entries, usedHashes);
            }

            _context.RainbowEntries.AddRange(entries);
            _context.SaveChanges();

            return rainbowTable;
        }

        private void AddEntry(string plainText, string algorithm, int chainLength, int tableId, List<RainbowEntry> entries, HashSet<string> usedHashes, bool forceAdd = false)
        {
            string hash = ComputeHash(plainText, algorithm);

            for (int j = 0; j < chainLength; j++)
            {
                plainText = ReduceHash(hash, j);
                hash = ComputeHash(plainText, algorithm);
            }

            // Если forceAdd == true, добавляем даже при дубликате
            if (forceAdd || !usedHashes.Contains(hash))
            {
                entries.Add(new RainbowEntry
                {
                    PlainText = plainText,
                    HashValue = hash,
                    RainbowTableId = tableId
                });

                usedHashes.Add(hash);
            }
        }

        private string ComputeHash(string input, string algorithm)
        {
            using HashAlgorithm hashAlgorithm = algorithm switch
            {
                "MD5" => MD5.Create(),
                "SHA-1" => SHA1.Create(),
                "SHA-256" => SHA256.Create(),
                //"SHA-512" => SHA512.Create(),
                //"SHA3-256" => Sha3.Sha3256(),
                //"SHA3-512" => Sha3.Sha3512(),
                "SHA-3" => Sha3.Sha3512(),
                _ => throw new ArgumentException($"Неверный алгоритм хеширования: {algorithm}")
            };

            byte[] hashBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        // Метод редукции хеша в текст
        private string ReduceHash(string hash, int step)
        {
            int minLength = 6;
            int maxLength = 16;
            int length = (step % (maxLength - minLength + 1)) + minLength; // от 6 до 16 символов

            int startIndex = (step * 13) % Math.Max(1, hash.Length - length);
            string reduced = hash.Substring(startIndex, length);

            // Вставляем символ из хеша и добавляем два случайных символа
            return $"{reduced}{hash[(startIndex + step) % hash.Length]}{step % 100}";
        }

        private string GenerateRandomString(int length = 6)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using var rng = RandomNumberGenerator.Create();
            byte[] data = new byte[length];

            rng.GetBytes(data);
            return new string(data.Select(b => chars[b % chars.Length]).ToArray());
        }

        // Метод сохранения результатов теста
        private void SaveTestResult(int hashFunctionId, string testType, double attempts)
        {
            Console.WriteLine($"[DEBUG] Сохраняем результат: HashFunctionId={hashFunctionId}, TestType={testType}, Attempts={attempts}");
            var testResult = new TestResult
            {
                HashFunctionId = hashFunctionId,
                TestType = testType,
                Score = attempts, // Количество попыток поиска
                TestDate = DateTime.UtcNow
            };

            _context.TestResults.Add(testResult);
            _context.SaveChanges();
            Console.WriteLine($"[DEBUG] Успешно сохранено: Id={testResult.Id}, Score={testResult.Score}");
        }

        public class SearchResult
        {
            public string PlainText { get; set; }
            public int ChainLength { get; set; }
        }

        // Метод поиска пароля
        public SearchResult? FindPasswordByHash(string inputHash, int hashFunctionId)
        {
            var hashFunction = _context.HashFunctions.FirstOrDefault(h => h.Id == hashFunctionId);
            if (hashFunction == null) return null;

            var rainbowTables = _context.RainbowTables.Where(rt => rt.HashFunctionId == hashFunctionId).ToList();
            if (!rainbowTables.Any()) return null;

            string algorithm = hashFunction.AlgorithmType;
            int attempts = 0;

            foreach (var rainbowTable in rainbowTables)
            {
                var allEntries = _context.RainbowEntries
                    .Where(e => e.RainbowTableId == rainbowTable.Id)
                    .OrderBy(e => e.Id)
                    .ToList();

                foreach (var entry in allEntries)
                {
                    attempts++;

                    if (entry.HashValue == inputHash)
                    {
                        SaveTestResult(hashFunctionId, "Поиск пароля", attempts);
                        return new SearchResult
                        {
                            PlainText = entry.PlainText,
                            ChainLength = 0
                        };
                    }
                }
            }

            SaveTestResult(hashFunctionId, "Поиск пароля", attempts);
            return null;
        }
    }
}
