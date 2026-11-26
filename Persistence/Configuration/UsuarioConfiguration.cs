using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Model;

namespace Persistence.Configuration;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Senha)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.ContaDeletada)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.SuspensoAte)
            .IsRequired(false);

        builder.Property(e => e.FotoPerfil)
            .IsRequired(false)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.QuantidadeSeguidores)
            .IsRequired()
            .HasDefaultValue(0);
    }
}
