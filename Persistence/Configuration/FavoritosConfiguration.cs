using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class FavoritosConfiguration : IEntityTypeConfiguration<Favoritos>
{
    public void Configure(EntityTypeBuilder<Favoritos> builder)
    {
        builder.ToTable("Favoritos");

        builder.Property(f => f.ExibirNoTopoPerfil)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(f => f.ObservacaoDestaque)
            .HasMaxLength(300);

        builder.HasOne<Jogo>()
            .WithMany()
            .HasForeignKey(f => f.JogoDestaquePerfilId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
