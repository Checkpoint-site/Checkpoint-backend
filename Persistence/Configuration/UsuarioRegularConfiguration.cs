using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class UsuarioRegularConfiguration : IEntityTypeConfiguration<UsuarioRegular>
{
    public void Configure(EntityTypeBuilder<UsuarioRegular> builder)
    {
        builder.ToTable("UsuariosRegulares");

        builder.Property(e => e.NivelGamer)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.PontosExperiencia)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.LimiteListas)
            .IsRequired()
            .HasDefaultValue(15);

        builder.Property(e => e.BiografiaGamer)
            .IsRequired(false)
            .HasMaxLength(500);
    }
}
