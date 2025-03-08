using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace The_complex_of_testing_hash_functions.Models
{
    public class RainbowTable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // Название радужной таблицы

        [Required]
        public int ChainLength { get; set; } // Длина цепочки редукций

        [Required]
        public int TableSize { get; set; } // Количество записей в таблице

        [Required]
        [ForeignKey("HashFunction")]
        public int HashFunctionId { get; set; }

        public HashFunction? HashFunction { get; set; }
    }
}

