using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace The_complex_of_testing_hash_functions.Models
{
    public class RainbowTable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PlainText { get; set; } = string.Empty;

        [Required]
        public string HashValue { get; set; } = string.Empty;

        [Required]
        [ForeignKey("HashFunction")]
        public int HashFunctionId { get; set; }

        public HashFunction? HashFunction { get; set; }
    }
}
