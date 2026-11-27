using System.Text.Json;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Configuration;

public class RegistroDiarioConfiguration : IEntityTypeConfiguration<RegistroDiario>
{
    public void Configure(EntityTypeBuilder<RegistroDiario> builder)
    {
        var tagsConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var tagsComparer = new ValueComparer<List<string>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            c => c.Aggregate(0, (acc, item) => HashCode.Combine(acc, item)),
            c => c.ToList());

        builder.ToTable("RegistrosDiario");

        builder.HasKey(r => r.IdRegistroDiario);

        builder.Property(r => r.Jogado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.NotaFogueiras);

        builder.Property(r => r.Tags)
            .HasConversion(tagsConverter)
            .Metadata.SetValueComparer(tagsComparer);

        builder.Property(r => r.Tags)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValueSql("'[]'");

        builder.Property(r => r.DataCriacao)
            .IsRequired();

        builder.Property(r => r.DataAtualizacao)
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(r => r.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Jogo>()
            .WithMany()
            .HasForeignKey(r => r.IdJogo)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.IdUsuario, r.IdJogo })
            .IsUnique();
    }
}
