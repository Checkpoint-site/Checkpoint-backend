using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class EngajamentoLoggerConfiguration : IEntityTypeConfiguration<EngajamentoLogger>
{
    public void Configure(EntityTypeBuilder<EngajamentoLogger> builder)
    {
        builder.ToTable("EngajamentoLoggers");

        builder.HasKey(l => l.IdLogger);

        builder.Property(l => l.QuantidadeCurtida)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(l => l.PresencaPost)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(l => l.PresencaComentario)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(l => l.PresencaFavoritos)
            .HasConversion<long>()
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(l => l.UltimaAtualizacao)
            .IsRequired();
    }
}
