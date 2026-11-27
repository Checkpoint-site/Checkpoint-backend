using System.Text.Json;
using Domain.Enum;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Configuration;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        var tipoPostConverter = new ValueConverter<List<Generos>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<Generos>()
                : JsonSerializer.Deserialize<List<Generos>>(v, (JsonSerializerOptions?)null) ?? new List<Generos>());

        var tipoPostComparer = new ValueComparer<List<Generos>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            c => c.Aggregate(0, (acc, item) => HashCode.Combine(acc, (int)item)),
            c => c.ToList());

        var tagsConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var tagsComparer = new ValueComparer<List<string>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            c => c.Aggregate(0, (acc, item) => HashCode.Combine(acc, item)),
            c => c.ToList());

        builder.ToTable("Posts");

        builder.HasKey(p => p.IdPost);

        builder.Property(p => p.Titulo)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(p => p.Fogueiras)
            .IsRequired();

        builder.Property(p => p.Conteudo)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(p => p.TipoPost)
            .HasConversion(tipoPostConverter)
            .Metadata.SetValueComparer(tipoPostComparer);

        builder.Property(p => p.TipoPost)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(p => p.StatusJornada)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.Tags)
            .HasConversion(tagsConverter)
            .Metadata.SetValueComparer(tagsComparer);

        builder.Property(p => p.Tags)
            .HasColumnType("nvarchar(max)")
            .IsRequired()
            .HasDefaultValueSql("'[]'");

        builder.Property(p => p.QuantidadeCurtidas)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.QuantidadeComentarios)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.TemSpoiler)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.PostDeletado)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.DataCriacao)
            .IsRequired();

        builder.Property(p => p.DataAtualizacao);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Jogo>()
            .WithMany()
            .HasForeignKey(p => p.IdJogo)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.IdUsuario, p.PostDeletado });
        builder.HasIndex(p => p.IdJogo);
    }
}
