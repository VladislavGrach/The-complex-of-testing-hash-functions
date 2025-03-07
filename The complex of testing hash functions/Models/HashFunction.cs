using System.ComponentModel.DataAnnotations;

namespace The_complex_of_testing_hash_functions.Models
{
    public class HashFunction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Algorithm { get; set; } = string.Empty; // Например, "SHA-256"

        public string? Description { get; set; }
    }
}
