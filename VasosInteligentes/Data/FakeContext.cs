using Microsoft.EntityFrameworkCore;
using VasosInteligentes.Models;

namespace VasosInteligentes.Data;

public class FakeContext : DbContext
{
    public DbSet<Vaso> Vasos { get; set; }
    public DbSet<Planta> Plantas { get; set; }
    public DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Base Temporario");
    }

public DbSet<VasosInteligentes.Models.User> User { get; set; } = default!;
}