using ControleDePatio.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleDePatio.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<Vaga> Vagas { get; set; }

    }
}
