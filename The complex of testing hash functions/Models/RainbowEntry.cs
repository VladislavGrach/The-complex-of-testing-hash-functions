using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace The_complex_of_testing_hash_functions.Models
{
    public class RainbowEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PlainText { get; set; } = string.Empty;

        [Required]
        public string HashValue { get; set; } = string.Empty;

        [Required]
        [ForeignKey("RainbowTable")]
        public int RainbowTableId { get; set; }

        public RainbowTable? RainbowTable { get; set; }
    }
}