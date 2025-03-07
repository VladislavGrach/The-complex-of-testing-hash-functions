using System.ComponentModel.DataAnnotations;

namespace The_complex_of_testing_hash_functions.Models
{
    public class HashFunction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // "Message Digest 5"

        [Required]
        public string AlgorithmType { get; set; } = string.Empty; // "MD5"

        public string? Description { get; set; }
    }
}
