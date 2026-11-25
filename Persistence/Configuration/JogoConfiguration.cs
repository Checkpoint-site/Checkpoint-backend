using System.Text.Json;
using Domain.Enum;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Configuration;

public class JogoConfiguration : IEntityTypeConfiguration<Jogo>
{
    public void Configure(EntityTypeBuilder<Jogo> builder)
    {
        var generosConverter = new ValueConverter<List<Generos>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<Generos>()
                : JsonSerializer.Deserialize<List<Generos>>(v, (JsonSerializerOptions?)null) ?? new List<Generos>());

        var generosComparer = new ValueComparer<List<Generos>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            c => c.Aggregate(0, (acc, item) => HashCode.Combine(acc, (int)item)),
            c => c.ToList());

        builder.ToTable("Jogos");

        builder.HasKey(j => j.IdJogo);

        builder.Property(j => j.SteamAppId)
            .IsRequired();

        builder.HasIndex(j => j.SteamAppId)
            .IsUnique();

        builder.Property(j => j.Nome)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasIndex(j => j.Nome);

        builder.Property(j => j.Descricao)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(j => j.Imagem)
            .IsRequired()
            .HasMaxLength(600);

        builder.Property(j => j.Desenvolvedora)
            .IsRequired()
            .HasMaxLength(250);

        builder.HasIndex(j => j.Desenvolvedora);

        builder.Property(j => j.Publicadora)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(j => j.DataLancamentoTexto)
            .HasMaxLength(80);

        builder.HasIndex(j => j.AnoLancamento);

        builder.Property(j => j.Generos)
            .HasConversion(generosConverter)
            .Metadata.SetValueComparer(generosComparer);

        builder.Property(j => j.Generos)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(j => j.Gratuito)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(j => j.EmBreve)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(j => j.TotalRecomendacoes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(j => j.SincronizadoEm)
            .IsRequired();
    }
}
