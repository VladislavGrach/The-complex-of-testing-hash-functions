using System;
using System.Linq;
using The_complex_of_testing_hash_functions.Interfaces;

namespace The_complex_of_testing_hash_functions.Services
{
    public class NistTestingService : INistTestingService
    {
        // Монобит-тест
        public double MonobitTest(string bits)
        {
            int n = bits.Length;
            int sum = bits.Count(bit => bit == '1') - bits.Count(bit => bit == '0');
            double S = Math.Abs(sum) / Math.Sqrt(n);
            return S; // Чем меньше, тем случайнее
        }

        // Частотный тест в блоках
        public double FrequencyTestWithinBlock(string binary, int blockSize = 128)
        {
            int n = binary.Length;
            int numBlocks = n / blockSize;
            if (numBlocks == 0) return double.NaN; // Недостаточно данных
            double sumChiSquare = 0;

            for (int i = 0; i < numBlocks; i++)
            {
                string block = binary.Substring(i * blockSize, blockSize);
                int onesCount = block.Count(c => c == '1');
                double pi = (double)onesCount / blockSize;
                double chiSquare = 4.0 * blockSize * (pi - 0.5) * (pi - 0.5);
                sumChiSquare += chiSquare;
            }

            double pValue = Math.Exp(-sumChiSquare / 2); // Оценка вероятности
            return pValue;
        }

        // Тест на серийность
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
            int maxRun = 0, currentRun = 0;

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

            return maxRun;  // Возвращаем длину самой длинной последовательности
        }

        // Тест ранга бинарной матрицы
        public double BinaryMatrixRankTest(string bits)
        {
            int n = bits.Length;

            // Выбираем максимально возможный размер матрицы
            int matrixSize = (int)Math.Sqrt(n);
            if (matrixSize < 2) return -1;  // Если слишком короткая строка

            int[,] matrix = new int[matrixSize, matrixSize];
            int index = 0;

            for (int i = 0; i < matrixSize; i++)
            {
                for (int j = 0; j < matrixSize; j++)
                {
                    matrix[i, j] = bits[index++] - '0';
                }
            }

            return ComputeRank(matrix) / (double)matrixSize; // Возвращаем нормализованный ранг
        }

        // Дискретное преобразование Фурье
        public double DiscreteFourierTransformTest(string bits)
        {
            int n = bits.Length;
            double[] sequence = bits.Select(bit => bit == '1' ? 1.0 : -1.0).ToArray();
            double[] spectrum = new double[n];

            for (int k = 0; k < n; k++)
            {
                for (int t = 0; t < n; t++)
                {
                    spectrum[k] += sequence[t] * Math.Cos(2 * Math.PI * k * t / n);
                }
            }

            double peak = spectrum.Max();
            return peak / Math.Sqrt(n);
        }

        //  Тест на несовпадающие шаблоны
        public double NonOverlappingTemplateMatchingTest(string binary, string template = "000111")
        {
            int templateLength = template.Length;
            int matches = 0;

            for (int i = 0; i <= binary.Length - templateLength; i += templateLength) // Без наложения
            {
                if (binary.Substring(i, templateLength) == template)
                {
                    matches++;
                }
            }

            return matches;
        }

        // Тест на совпадающие шаблоны
        public double OverlappingTemplateMatchingTest(string bits, int m = 10)
        {
            int pattern = Convert.ToInt32(new string('1', m), 2);  // Шаблон 1111111111
            int count = 0;

            for (int i = 0; i <= bits.Length - m; i++)
            {
                int subPattern = Convert.ToInt32(bits.Substring(i, m), 2);
                if (subPattern == pattern) count++;
            }

            return count;
        }

        // Универсальный тест Маурера
        public double MaurersUniversalTest(string bits)
        {
            int n = bits.Length;
            if (n < 32) return -1;  // Минимальный порог длины

            int L = 8;
            while ((1 << L) > n / 4 && L > 4) // Делаем L не слишком маленьким
            {
                L--;
            }

            int Q = Math.Min(1 << L, n / (2 * L));  // Q не превышает половины доступных блоков
            int K = n / L - Q;

            if (K <= 0)
            {
                Q = Math.Max(1, n / (2 * L));  // Уменьшаем Q, но не ниже 1
                K = n / L - Q;

                if (K <= 0)  // Если после корректировки всё равно проблема
                {
                    return -1;
                }
            }

            var table = new int[Q];
            int sum = 0;

            for (int i = Q; i < Q + K; i++)
            {
                if (i * L + L > bits.Length) break; // Проверка выхода за границы строки

                int index = Convert.ToInt32(bits.Substring(i * L, L), 2);
                if (index >= Q)
                {
                    index = Q - 1;  // Коррекция индекса
                }

                sum += i + 1 - table[index];
                table[index] = i + 1;
            }

            double result = sum / (double)K;

            if (double.IsInfinity(result) || double.IsNaN(result))
            {
                return -1;
            }

            return result;
        }

        // Тест Лемпеля-Зива
        public double LempelZivCompressionTest(string bits)
        {
            HashSet<string> dictionary = new();
            string current = "";

            foreach (char bit in bits)
            {
                current += bit;
                if (!dictionary.Contains(current))
                {
                    dictionary.Add(current);
                    current = "";
                }
            }

            return dictionary.Count / (double)bits.Length;
        }

        // Тест линейной сложности
        public double LinearComplexityTest(string bits, int M = 500)
        {
            int n = bits.Length;
            if (n < 2) return -1; // Минимальная длина

            // Если строка короче M, уменьшаем размер блока
            M = Math.Min(M, n);
            int N = n / M;
            if (N == 0) return -1; // Если после изменения всё равно не подходит

            int[] C = new int[M + 1];
            int[] B = new int[M + 1];
            C[0] = B[0] = 1;

            int L = 0, m = -1, d;

            for (int nIdx = 0; nIdx < N; nIdx++)
            {
                d = 0;

                for (int i = 0; i <= L && (nIdx * M + i) < bits.Length; i++)
                {
                    d ^= C[i] * (bits[nIdx * M + i] - '0');
                }

                if (d == 1)
                {
                    int[] T = (int[])C.Clone();
                    for (int i = 0; i < B.Length && (i + nIdx - m) < M; i++)
                    {
                        C[i + nIdx - m] ^= B[i];
                    }
                    if (2 * L <= nIdx)
                    {
                        L = nIdx + 1 - L;
                        m = nIdx;
                        B = T;
                    }
                }
            }

            double complexity = (double)L / M;

            if (double.IsNaN(complexity) || double.IsInfinity(complexity))
            {
                return -1;
            }

            return complexity;
        }

        // Серийный тест
        public double SerialTest(string bits, int m = 2)
        {
            int n = bits.Length;
            Dictionary<string, int> freq = new();

            for (int i = 0; i <= n - m; i++)
            {
                string substring = bits.Substring(i, m);
                if (!freq.ContainsKey(substring)) freq[substring] = 0;
                freq[substring]++;
            }

            double chiSquare = 0;
            double expected = (double)n / Math.Pow(2, m);

            foreach (var kv in freq.Values)
            {
                chiSquare += Math.Pow(kv - expected, 2) / expected;
            }

            return chiSquare;
        }

        // Тест приближенной энтропии
        public double ApproximateEntropyTest(string bits, int m = 2)
        {
            int n = bits.Length;
            Dictionary<string, int> freqM = new();
            Dictionary<string, int> freqM1 = new();

            for (int i = 0; i < n - m; i++)
            {
                string subM = bits.Substring(i, m);
                string subM1 = bits.Substring(i, m + 1);

                if (freqM.ContainsKey(subM)) freqM[subM]++;
                else freqM[subM] = 1;

                if (freqM1.ContainsKey(subM1)) freqM1[subM1]++;
                else freqM1[subM1] = 1;
            }

            double entropyM = freqM.Values.Sum(f => f * Math.Log(f)) / n;
            double entropyM1 = freqM1.Values.Sum(f => f * Math.Log(f)) / n;

            return entropyM - entropyM1;
        }

        // Тест накопленных сумм
        public double CusumTest(string bits)
        {
            int n = bits.Length;
            int sum = 0, maxDeviation = 0;

            foreach (char bit in bits)
            {
                sum += bit == '1' ? 1 : -1;
                maxDeviation = Math.Max(maxDeviation, Math.Abs(sum));
            }

            return maxDeviation / Math.Sqrt(n);
        }

        // Тест случайных экскурсий
        public int RandomExcursionsTest(string bits)
        {
            int sum = 0, zeroCrossings = 0;

            foreach (char bit in bits)
            {
                sum += bit == '1' ? 1 : -1;
                if (sum == 0) zeroCrossings++;
            }

            // Ограничиваем результат, чтобы избежать больших чисел
            return Math.Min(zeroCrossings, 10);
        }

        // Тест вариантов случайных экскурсий
        public Dictionary<int, int> RandomExcursionsVariantTest(string bits)
        {
            int sum = 0;
            Dictionary<int, int> visits = new();

            foreach (char bit in bits)
            {
                sum += bit == '1' ? 1 : -1;
                if (!visits.ContainsKey(sum)) visits[sum] = 0;
                visits[sum]++;
            }

            return visits
                .Where(kv => Math.Abs(kv.Key) <= 5)  // Ограничиваем максимум
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private int ComputeRank(int[,] matrix)
        {
            int rank = 0;
            int size = matrix.GetLength(0);

            for (int i = 0; i < size; i++)
            {
                if (matrix[i, i] == 0)
                {
                    for (int j = i + 1; j < size; j++)
                    {
                        if (matrix[j, i] == 1)
                        {
                            for (int k = 0; k < size; k++)
                            {
                                matrix[i, k] ^= matrix[j, k];
                            }
                            break;
                        }
                    }
                }
                if (matrix[i, i] == 1)
                {
                    rank++;
                    for (int j = i + 1; j < size; j++)
                    {
                        if (matrix[j, i] == 1)
                        {
                            for (int k = 0; k < size; k++)
                            {
                                matrix[j, k] ^= matrix[i, k];
                            }
                        }
                    }
                }
            }

            return rank;
        }

        private void SwapRows(int[][] matrix, int row1, int row2)
        {
            int[] temp = matrix[row1];
            matrix[row1] = matrix[row2];
            matrix[row2] = temp;
        }

        // Преобразование hex в binary
        public string ConvertHexToBinary(string hex)
        {
            return string.Join("", hex.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
        }
    }
}