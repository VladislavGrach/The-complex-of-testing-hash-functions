using System;
using System.Linq;

namespace The_complex_of_testing_hash_functions.Services
{
    public class TestingService
    {
        // Монобит-тест
        public double MonobitTest(string bits)
        {
            int n = bits.Length;
            int sum = bits.Count(bit => bit == '1') - bits.Count(bit => bit == '0');

            double S = Math.Abs(sum) / Math.Sqrt(n);
            return S; // Чем меньше, тем случайнее
        }

        // Тест на частоту в блоках
        public double BlockFrequencyTest(string bits)
        {
            int blockSize = 8;
            int numBlocks = bits.Length / blockSize;
            double expectedFreq = blockSize / 2.0;

            double chiSquare = 0;
            for (int i = 0; i < numBlocks; i++)
            {
                string block = bits.Substring(i * blockSize, blockSize);
                int onesCount = block.Count(c => c == '1');
                chiSquare += Math.Pow(onesCount - expectedFreq, 2) / expectedFreq;
            }

            return chiSquare; // Чем меньше, тем случайнее
        }

        // Покер-тест
        public double PokerTest(string bits)
        {
            int m = 4;
            int k = bits.Length / m;
            var counts = new Dictionary<string, int>();

            for (int i = 0; i < k; i++)
            {
                string substring = bits.Substring(i * m, m);
                if (!counts.ContainsKey(substring))
                    counts[substring] = 0;
                counts[substring]++;
            }

            double sum = counts.Values.Sum(x => x * x);
            return (Math.Pow(2, m) / k) * sum - k;
        }

        // Тест на последовательности одинаковых битов
        public double RunsTest(string bits)
        {
            int onesCount = 0, zerosCount = 0;
            int runs = 0;
            char lastBit = bits[0];

            foreach (char bit in bits)
            {
                if (bit == '1') onesCount++;
                else zerosCount++;

                if (bit != lastBit)
                {
                    runs++;
                    lastBit = bit;
                }
            }

            double expectedRuns = ((2.0 * onesCount * zerosCount) / (onesCount + zerosCount)) + 1;
            return Math.Abs(runs - expectedRuns); // Чем меньше разница, тем случайнее
        }

        // Тест на самую длинную последовательность единиц в блоке
        public double LongestRunOfOnesTest(string bits)
        {
            int maxRun = 0;
            int currentRun = 0;
            foreach (char bit in bits)
            {
                if (bit == '1')
                {
                    currentRun++;
                    maxRun = Math.Max(maxRun, currentRun);
                }
                else
                {
                    currentRun = 0;
                }
            }
            double expectedRun = Math.Log(bits.Length) / Math.Log(2);
            return Math.Abs(maxRun - expectedRun);
        }

        // Тест на перестановки и преобразования
        public double PermutationsTest(string bits)
        {
            int n = bits.Length / 3;
            if (n < 3) return double.MaxValue; // Слишком короткая последовательность для теста

            List<string> triples = new List<string>();
            for (int i = 0; i < n; i++)
            {
                triples.Add(bits.Substring(i * 3, 3));
            }

            int distinctCount = triples.Distinct().Count();
            double expectedCount = Math.Pow(2, 3) * 0.75;
            return Math.Abs(distinctCount - expectedCount);
        }

        // Преобразование hex в binary
        public string ConvertHexToBinary(string hex)
        {
            return string.Join("", hex.Select(c =>
                Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }




        // Diehard

        private static Random random = new Random();

        // Тест дней рождения
        public double BirthdaySpacingsTest(string bits)
        {
            int n = bits.Length / 16; // Разбиваем на 16-битные числа
            HashSet<int> seen = new HashSet<int>();

            for (int i = 0; i < n; i++)
            {
                int value = Convert.ToInt32(bits.Substring(i * 16, 16), 2);
                if (seen.Contains(value)) return 5.0; // Если есть дубликаты, тест провален
                seen.Add(value);
            }

            return 0.0; // Чем меньше, тем лучше
        }

        // Тест равномерности распределения битов
        public double OverlappingSumsTest(string bits)
        {
            int blockSize = 8;
            int sum = 0;
            int count = 0;

            for (int i = 0; i < bits.Length - blockSize; i += blockSize)
            {
                string block = bits.Substring(i, blockSize);
                sum += Convert.ToInt32(block, 2);
                count++;
            }

            double avg = (double)sum / count;
            return Math.Abs(avg - (Math.Pow(2, blockSize - 1))); // Чем ближе к 128, тем случайнее
        }

        // Тест совпадения шаблонов
        public double OverlappingTemplateMatchingTest(string bits)
        {
            string pattern = "1101"; // Шаблон для поиска
            int count = 0;
            int maxPatterns = bits.Length / pattern.Length;

            for (int i = 0; i < bits.Length - pattern.Length; i++)
            {
                if (bits.Substring(i, pattern.Length) == pattern)
                    count++;
            }

            return Math.Abs(count - maxPatterns / 2.0); // Чем ближе к среднему, тем лучше
        }
    }
}