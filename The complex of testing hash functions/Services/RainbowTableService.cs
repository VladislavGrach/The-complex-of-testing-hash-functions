using System.Security.Cryptography;
using System.Text;
using The_complex_of_testing_hash_functions.Models;
using SHA3.Net;
using Org.BouncyCastle.Crypto.Digests;


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
                throw new ArgumentException("Хэш-функция не найдена");

            // Проверяем, существует ли уже таблица для этой хэш-функции
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

        private void AddEntry(string plainTextStart, string algorithm, int chainLength, int tableId, List<RainbowEntry> entries, HashSet<string> usedHashes, bool forceAdd = false)
        {
            string plainText = plainTextStart;
            string hash = ComputeHash(plainText, algorithm);

            for (int j = 0; j < chainLength; j++)
            {
                plainText = ReduceHash(hash, j);
                hash = ComputeHash(plainText, algorithm);
            }

            if (forceAdd || !usedHashes.Contains(hash))
            {
                entries.Add(new RainbowEntry
                {
                    PlainText = plainTextStart,
                    HashValue = hash,
                    RainbowTableId = tableId
                });

                usedHashes.Add(hash);
            }
        }

        private string ComputeHash(string input, string algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes;

            switch (algorithm)
            {
                case "MD5":
                    using (var md5 = MD5.Create())
                        hashBytes = md5.ComputeHash(inputBytes);
                    break;

                case "SHA-1":
                    using (var sha1 = SHA1.Create())
                        hashBytes = sha1.ComputeHash(inputBytes);
                    break;

                case "SHA-256":
                    using (var sha256 = SHA256.Create())
                        hashBytes = sha256.ComputeHash(inputBytes);
                    break;

                case "SHA-3":
                    using (var sha3 = Sha3.Sha3512())
                        hashBytes = sha3.ComputeHash(inputBytes);
                    break;

                case "STRIBOG-256":
                    hashBytes = ComputeGostHash(inputBytes, 256);
                    break;

                case "STRIBOG-512":
                    hashBytes = ComputeGostHash(inputBytes, 512);
                    break;

                default:
                    throw new ArgumentException($"Неверный алгоритм хэширования: {algorithm}");
            }

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        // Метод редукции хэша в текст
        private string ReduceHash(string hash, int step)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int minLength = 4;
            int maxLength = 16;

            // Вычисляем длину строки на текущем шаге
            int length = (step % (maxLength - minLength + 1)) + minLength;

            // Смешиваем хэш и шаг для получения устойчивого значения
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                // Индекс в пределах длины хэша
                int index = (step * 31 + i * 17) % hash.Length;

                // Преобразуем символ из хэша в индекс символа из алфавита
                char c = hash[index];
                int charIndex = (int)c % chars.Length;
                result.Append(chars[charIndex]);
            }

            return result.ToString();
        }

        private string GenerateRandomString()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int minLength = 4;
            int maxLength = 16;

            int length = RandomNumberBetween(minLength, maxLength);

            using var rng = RandomNumberGenerator.Create();
            byte[] data = new byte[length];

            rng.GetBytes(data);
            return new string(data.Select(b => chars[b % chars.Length]).ToArray());
        }

        // Вспомогательный метод получения случайного числа
        private int RandomNumberBetween(int min, int max)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] uint32Buffer = new byte[4];
            rng.GetBytes(uint32Buffer);
            uint randomValue = BitConverter.ToUInt32(uint32Buffer, 0);
            return (int)(min + (randomValue % (max - min + 1)));
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

            var rainbowTables = _context.RainbowTables
                .Where(rt => rt.HashFunctionId == hashFunctionId)
                .ToList();

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

                    // Попробуем восстановить всю цепочку из этого entry
                    string current = entry.PlainText;
                    string hash = ComputeHash(current, algorithm);

                    if (hash == inputHash)
                    {
                        SaveTestResult(hashFunctionId, "Поиск пароля", attempts);
                        return new SearchResult { PlainText = current, ChainLength = 0 };
                    }

                    for (int i = 0; i < rainbowTable.ChainLength; i++)
                    {
                        string reduced = ReduceHash(hash, i);
                        current = reduced;
                        hash = ComputeHash(current, algorithm);

                        if (hash == inputHash)
                        {
                            SaveTestResult(hashFunctionId, "Поиск пароля", attempts);
                            return new SearchResult
                            {
                                PlainText = entry.PlainText, // начальный элемент
                                ChainLength = i + 1
                            };
                        }
                    }
                }
            }

            SaveTestResult(hashFunctionId, "Поиск пароля", attempts);
            return null;
        }


        private byte[] ComputeGostHash(byte[] inputBytes, int bits)
        {
            Org.BouncyCastle.Crypto.IDigest digest;

            if (bits == 256)
            {
                digest = new Gost3411_2012_256Digest();
            }
            else
            {
                digest = new Gost3411_2012_512Digest();
            }

            digest.BlockUpdate(inputBytes, 0, inputBytes.Length);

            byte[] result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);

            return result;
        }
    }
}
