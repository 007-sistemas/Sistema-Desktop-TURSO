using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace BiometricSystem.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; } // Exemplo de entidade

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbDir = Path.Combine(appData, "BiometricSystem");
            Directory.CreateDirectory(dbDir);
            var dbPath = Path.Combine(dbDir, "biometric.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
}
