using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace BiometricSystem.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; } // Exemplo de entidade
        public DbSet<ParametrosSistema> Parametros { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbDir = Path.Combine(appData, "BiometricSystem");
            Directory.CreateDirectory(dbDir);
            var dbPath = Path.Combine(dbDir, "biometric.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        public static void EnsureDatabaseCreated()
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }
        }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
}
