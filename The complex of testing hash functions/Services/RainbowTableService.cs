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

        public RainbowTableService(HashTestingContext context)
        {
            _context = context;
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
                HashFunctionId = hashFunctionId // Теперь заполняем ID!
            };

            var table = new Dictionary<string, string>(); // PlainText -> HashValue
            for (int i = 0; i < tableSize; i++)
            {
                string plainText = GenerateRandomString();
                string hash = ComputeHash(plainText, hashFunction.AlgorithmType);

                for (int j = 0; j < chainLength; j++)
                {
                    plainText = ReduceHash(hash, j);
                    hash = ComputeHash(plainText, hashFunction.AlgorithmType);
                }

                if (!table.ContainsKey(plainText))
                    table.Add(plainText, hash);
            }

            _context.RainbowTables.Add(rainbowTable);
            _context.SaveChanges();

            return rainbowTable;
        }

        private string ComputeHash(string input, string algorithm)
        {
            using HashAlgorithm hashAlgorithm = algorithm switch
            {
                "MD5" => MD5.Create(),
                "SHA-1" => SHA1.Create(),
                "SHA-256" => SHA256.Create(),
                //"SHA-512" => SHA512.Create(),
                "SHA3-256" => Sha3.Sha3256(),
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
            int length = 6; // Длина редукции
            return new string(hash.Skip(step).Take(length).ToArray());
        }

        private string GenerateRandomString(int length = 6)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
        }
    }
}
