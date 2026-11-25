using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class UsuarioLoggerConfiguration : IEntityTypeConfiguration<UsuarioLogger>
{
    public void Configure(EntityTypeBuilder<UsuarioLogger> builder)
    {
        builder.ToTable("UsuarioLoggers");

        builder.Property(u => u.IdUsuario)
            .IsRequired();

        builder.Property(u => u.CurtidasRecebidas)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(u => u.QuantidadeSeguidores)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.HasOne<Usuario>()
            .WithOne()
            .HasForeignKey<UsuarioLogger>(u => u.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.IdUsuario)
            .IsUnique();
    }
}
