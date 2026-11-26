using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class ModeradorConfiguration : IEntityTypeConfiguration<Moderador>
{
    public void Configure(EntityTypeBuilder<Moderador> builder)
    {
        builder.ToTable("Moderadores");

        builder.Property(e => e.CodigoCredencial)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.CodigoCredencial)
            .IsUnique();

        builder.Property(e => e.SetorModeracao)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.TotalAcoesModeracao)
            .IsRequired()
            .HasDefaultValue(0);
    }
}
