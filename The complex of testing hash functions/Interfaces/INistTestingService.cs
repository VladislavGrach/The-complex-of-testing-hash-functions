namespace The_complex_of_testing_hash_functions.Interfaces
{
    public interface INistTestingService
    {
        double MonobitTest(string bits);
        double FrequencyTestWithinBlock(string binary, int blockSize = 128);
        double RunsTest(string bits);
        double LongestRunOfOnesTest(string bits);
        double BinaryMatrixRankTest(string bits);
        double DiscreteFourierTransformTest(string bits);
        double NonOverlappingTemplateMatchingTest(string binary, string template = "000111");
        double OverlappingTemplateMatchingTest(string bits, int m = 10);
        double MaurersUniversalTest(string bits);
        double LempelZivCompressionTest(string bits);
        double LinearComplexityTest(string bits, int M = 500);
        double SerialTest(string bits, int m = 2);
        double ApproximateEntropyTest(string bits, int m = 2);
        double CusumTest(string bits);
        int RandomExcursionsTest(string bits);
        Dictionary<int, int> RandomExcursionsVariantTest(string bits);
    }
}
