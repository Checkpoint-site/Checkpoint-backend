using System.Text.Json;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Configuration;

public class CatalogoConfiguration : IEntityTypeConfiguration<Catalogo>
{
    public void Configure(EntityTypeBuilder<Catalogo> builder)
    {
        var jogosConverter = new ValueConverter<List<int>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<int>()
                : JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>());

        var jogosComparer = new ValueComparer<List<int>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            c => c.Aggregate(0, (acc, item) => HashCode.Combine(acc, item)),
            c => c.ToList());

        builder.ToTable("Catalogos");

        builder.HasKey(c => c.IdCatalogo);

        builder.Property(c => c.IdUsuario)
            .IsRequired();

        builder.HasIndex(c => c.IdUsuario);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.Jogos)
            .HasConversion(jogosConverter)
            .Metadata.SetValueComparer(jogosComparer);

        builder.Property(c => c.Jogos)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(c => c.DataAtualizacao)
            .IsRequired();
    }
}
