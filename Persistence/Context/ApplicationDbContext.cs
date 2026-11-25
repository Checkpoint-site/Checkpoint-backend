using Microsoft.EntityFrameworkCore;
using Domain.Model;

namespace Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<UsuarioRegular> UsuariosRegulares { get; set; }
    public DbSet<Moderador> Moderadores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}


