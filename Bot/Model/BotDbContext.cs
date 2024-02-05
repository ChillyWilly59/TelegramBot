using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Model
{
    public class BotDbContext : DbContext
    {
        public DbSet<FavoriteCities> FavoriteCities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=BotDB;Trusted_Connection=True;TrustServerCertificate=true");
        }
    }
}
