using System;
using System.Linq;

namespace The_complex_of_testing_hash_functions.Services
{
    public class TestingService
    {
        // Монобит-тест: возвращает статистику S (чем меньше, тем лучше)
        public double MonobitTest(string bits)
        {
            int n = bits.Length;
            int sum = bits.Count(bit => bit == '1') - bits.Count(bit => bit == '0');

            double S = Math.Abs(sum) / Math.Sqrt(n);
            return S; // Чем меньше, тем случайнее
        }

        // Тест на частоту в блоках: возвращает хи-квадрат
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

        // Покер-тест: возвращает x (чем меньше, тем лучше)
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
            double x = (Math.Pow(2, m) / k) * sum - k;
            return x;
        }

        // Преобразование hex в binary
        public string ConvertHexToBinary(string hex)
        {
            return string.Join("", hex.Select(c =>
                Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }
    }
}