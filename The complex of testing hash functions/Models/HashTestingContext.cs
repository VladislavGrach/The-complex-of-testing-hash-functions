using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace The_complex_of_testing_hash_functions.Models
{
    public class HashTestingContext : DbContext
    {
        public HashTestingContext(DbContextOptions<HashTestingContext> options) : base(options) { }

        public DbSet<HashFunction> HashFunctions { get; set; }
        public DbSet<RainbowTable> RainbowTables { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
    }
}
