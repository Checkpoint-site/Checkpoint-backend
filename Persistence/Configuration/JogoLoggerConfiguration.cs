using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class JogoLoggerConfiguration : IEntityTypeConfiguration<JogoLogger>
{
    public void Configure(EntityTypeBuilder<JogoLogger> builder)
    {
        builder.ToTable("JogoLoggers");

        builder.Property(j => j.IdJogo)
            .IsRequired();

        builder.Property(j => j.PresencaPretendeJogar)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(j => j.SomaFogueiras)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.HasOne<Jogo>()
            .WithOne()
            .HasForeignKey<JogoLogger>(j => j.IdJogo)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(j => j.IdJogo)
            .IsUnique();
    }
}
