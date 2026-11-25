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

        builder.HasIndex(g => g.Genero)
            .IsUnique()
            .HasDatabaseName("IX_GeneroLoggers_Genero");
    }
}
