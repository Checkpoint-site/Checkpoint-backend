using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<SolicitacaoModerador> SolicitacoesModerador { get; set; }
    public DbSet<Jogo> Jogos { get; set; }
    public DbSet<Catalogo> Catalogos { get; set; }
    public DbSet<Favoritos> Favoritos { get; set; }
    public DbSet<PretendeJogar> PretendeJogar { get; set; }
    public DbSet<EngajamentoLogger> EngajamentoLoggers { get; set; }
    public DbSet<GeneroLogger> GeneroLoggers { get; set; }
    public DbSet<JogoLogger> JogoLoggers { get; set; }
    public DbSet<UsuarioLogger> UsuarioLoggers { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comentario> Comentarios { get; set; }
    public DbSet<Curtida> Curtidas { get; set; }
    public DbSet<CurtidaPost> CurtidasPosts { get; set; }
    public DbSet<CurtidaComentario> CurtidasComentarios { get; set; }
    public DbSet<RegistroDiario> RegistrosDiario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
