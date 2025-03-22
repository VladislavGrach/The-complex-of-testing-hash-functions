using System;
using System.Collections.Generic;
using System.Linq;
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
            List<int> values = new List<int>();

            for (int i = 0; i < n; i++)
            {
                int value = Convert.ToInt32(bits.Substring(i * 16, 16), 2);
                values.Add(value);
            }

            values.Sort();

            List<int> spacings = new List<int>();
            for (int i = 1; i < values.Count; i++)
            {
                spacings.Add(values[i] - values[i - 1]);
            }

            double meanSpacing = spacings.Average();
            double expectedSpacing = (double)UInt16.MaxValue / n; // Ожидаемое среднее расстояние

            return Math.Abs(meanSpacing - expectedSpacing) / expectedSpacing; // Чем ближе к 0, тем лучше
        }

        // Тест подсчёта единиц
        public double CountOnesTest(string bits)
        {
            int onesCount = bits.Count(c => c == '1');
            double expected = bits.Length / 2.0;

            return Math.Abs(onesCount - expected) / Math.Sqrt(expected); // χ²-подобная мера отклонения
        }

        // Тест рангов матриц
        public double RanksOfMatricesTest(string bits)
        {
            int size = 32; // 32x32 битная матрица
            int numMatrices = bits.Length / (size * size);

            if (numMatrices == 0)
            {
                return 0; // Если не можем сформировать матрицу, тест не имеет смысла
            }

            int fullRankCount = 0;

            for (int i = 0; i < numMatrices; i++)
            {
                int[,] matrix = new int[size, size];

                for (int row = 0; row < size; row++)
                {
                    for (int col = 0; col < size; col++)
                    {
                        matrix[row, col] = bits[i * size * size + row * size + col] == '1' ? 1 : 0;
                    }
                }

                if (GetMatrixRank(matrix) == size)
                {
                    fullRankCount++;
                }
            }

            double expectedFullRank = numMatrices * 0.2888; // Ожидаемая доля полных рангов
            double result = expectedFullRank > 0 ? Math.Abs(fullRankCount - expectedFullRank) / Math.Sqrt(expectedFullRank) : 0;

            return result;
        }

        // Метод для нахождения ранга бинарной матрицы
        private int GetMatrixRank(int[,] matrix)
        {
            int rank = 0;
            int size = matrix.GetLength(0);
            bool[] rowUsed = new bool[size];

            for (int col = 0; col < size; col++)
            {
                int row = -1;
                for (int i = 0; i < size; i++)
                {
                    if (!rowUsed[i] && matrix[i, col] == 1)
                    {
                        row = i;
                        break;
                    }
                }

                if (row == -1) continue;

                rank++;
                rowUsed[row] = true;

                for (int i = 0; i < size; i++)
                {
                    if (i != row && matrix[i, col] == 1)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            matrix[i, j] ^= matrix[row, j];
                        }
                    }
                }
            }

            return rank;
        }

        // Тест на перестановки
        public double OverlappingPermutationsTest(string bits)
        {
            int n = bits.Length / 3;
            if (n < 3) return double.MaxValue; // Недостаточно данных

            Dictionary<string, int> frequencies = new Dictionary<string, int>();

            for (int i = 0; i < n; i++)
            {
                string triple = bits.Substring(i * 3, 3);
                if (frequencies.ContainsKey(triple))
                    frequencies[triple]++;
                else
                    frequencies[triple] = 1;
            }

            double expectedFreq = (double)n / 8; // 8 возможных троек (000, 001, ..., 111)
            double chiSquared = frequencies.Values.Sum(f => Math.Pow(f - expectedFreq, 2) / expectedFreq);

            return chiSquared;
        }

        // Тест серийности
        public double RunsTest(string bits)
        {
            List<int> runLengths = new List<int>();
            char lastBit = bits[0];
            int runLength = 1;

            for (int i = 1; i < bits.Length; i++)
            {
                if (bits[i] == lastBit)
                {
                    runLength++;
                }
                else
                {
                    runLengths.Add(runLength);
                    lastBit = bits[i];
                    runLength = 1;
                }
            }

            double meanRunLength = runLengths.Average();
            double expectedMean = 2.0; // Ожидаемая средняя длина серии

            return Math.Abs(meanRunLength - expectedMean) / Math.Sqrt(expectedMean);
        }
    }
}
