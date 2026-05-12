using ControleDePatio.Models;
using Microsoft.EntityFrameworkCore;

namespace ControleDePatio.Data
{
    public class AppDbContext : DbContext
    {
        // Construtor obrigatório do Entity Framework
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Estas duas linhas representam as nossas tabelas no banco de dados!
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
        public DbSet<Vaga> Vagas { get; set; }

    }
}
