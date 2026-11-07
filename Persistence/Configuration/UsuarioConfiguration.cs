using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Model;

namespace Persistence.Configuration;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
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
    }
}
