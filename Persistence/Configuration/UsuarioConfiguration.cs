using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Models;

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

        builder.Property(e => e.Senha)
        .IsRequired()
        .HasMaxLength(256);

        builder.HasIndex(e => e.Email)
        .IsUnique();
    }
}
