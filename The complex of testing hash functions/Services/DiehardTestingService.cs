using The_complex_of_testing_hash_functions.Interfaces;

namespace The_complex_of_testing_hash_functions.Services
{
    public class DiehardTestingService : IDiehardTestingService
    {
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
    }
}
