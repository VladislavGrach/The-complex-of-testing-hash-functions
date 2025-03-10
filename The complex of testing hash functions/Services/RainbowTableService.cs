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
            HashSet<string> usedHashes = new HashSet<string>(); // Исключаем дубликаты

            // 1. Добавляем популярные пароли в первую очередь
            foreach (var popularPassword in _popularPasswords.Take(tableSize))
            {
                AddEntry(popularPassword, hashFunction.AlgorithmType, chainLength, rainbowTable.Id, entries, usedHashes, true);
            }

            // 2. Если после популярных паролей места ещё остались, добавляем случайные
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
            int length = 6;
            int startIndex = (step * 7) % (hash.Length - length); // Смещение сильнее меняется
            string reduced = new string(hash.Skip(startIndex).Take(length).ToArray());

            // Добавляем цифру шага и символ из хеша
            return $"{reduced}{hash[step % hash.Length]}{step % 10}";
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
            var testResult = new TestResult
            {
                HashFunctionId = hashFunctionId,
                TestType = testType,
                Score = attempts, // Количество попыток поиска
                TestDate = DateTime.UtcNow
            };

            _context.TestResults.Add(testResult);
            _context.SaveChanges();
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

            var rainbowTable = _context.RainbowTables.FirstOrDefault(rt => rt.HashFunctionId == hashFunctionId);
            if (rainbowTable == null) return null;

            string algorithm = hashFunction.AlgorithmType;

            if (rainbowTable.ChainLength == 0)
            {
                var directMatch = _context.RainbowEntries.FirstOrDefault(e => e.HashValue == inputHash);
                if (directMatch != null)
                {
                    return new SearchResult
                    {
                        PlainText = directMatch.PlainText,
                        ChainLength = 0
                    };
                }
                return null;
            }

            for (int i = rainbowTable.ChainLength - 1; i >= 0; i--)
            {
                string hash = inputHash;

                for (int j = i; j < rainbowTable.ChainLength; j++)
                {
                    hash = ReduceHash(hash, j);
                    hash = ComputeHash(hash, algorithm);
                }

                var match = _context.RainbowEntries.FirstOrDefault(e => e.HashValue == hash);
                if (match != null)
                {
                    string recoveredText = match.PlainText;

                    for (int j = 0; j < rainbowTable.ChainLength; j++)
                    {
                        string currentHash = ComputeHash(recoveredText, algorithm);
                        if (currentHash == inputHash)
                        {
                            return new SearchResult
                            {
                                PlainText = recoveredText,
                                ChainLength = rainbowTable.ChainLength
                            };
                        }
                        recoveredText = ReduceHash(currentHash, j);
                    }
                }
            }
            return null;
        }
    }
}
