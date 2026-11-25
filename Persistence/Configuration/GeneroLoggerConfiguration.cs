using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class GeneroLoggerConfiguration : IEntityTypeConfiguration<GeneroLogger>
{
    public void Configure(EntityTypeBuilder<GeneroLogger> builder)
    {
        builder.ToTable("GeneroLoggers");

        builder.Property(g => g.Genero)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(g => g.PresencaPretendeJogar)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(g => g.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.Genero)
            .IsUnique()
            .HasFilter("[IdUsuario] IS NULL")
            .HasDatabaseName("IX_GeneroLoggers_Genero_Global");

        builder.HasIndex(g => new { g.IdUsuario, g.Genero })
            .IsUnique()
            .HasFilter("[IdUsuario] IS NOT NULL")
            .HasDatabaseName("IX_GeneroLoggers_Usuario_Genero");
    }
}
