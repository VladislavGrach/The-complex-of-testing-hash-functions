using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace The_complex_of_testing_hash_functions.Models
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime TestDate { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey("HashFunction")]
        public int HashFunctionId { get; set; }

        public HashFunction? HashFunction { get; set; }

        [Required]
        public string TestType { get; set; } = string.Empty; // "Лавинный эффект"

        [Required]
        public double Score { get; set; } // Оценка устойчивости
    }
}
