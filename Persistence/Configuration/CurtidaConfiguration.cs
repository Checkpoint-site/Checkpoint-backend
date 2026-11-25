using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class CurtidaConfiguration : IEntityTypeConfiguration<Curtida>
{
    public void Configure(EntityTypeBuilder<Curtida> builder)
    {
        builder.ToTable("Curtidas");

        builder.HasKey(c => c.IdCurtida);

        builder.HasDiscriminator<string>("TipoCurtida")
            .HasValue<CurtidaPost>("Post")
            .HasValue<CurtidaComentario>("Comentario");

        builder.Property<string>("TipoCurtida")
            .HasMaxLength(30);

        builder.Property(c => c.DataCriacao)
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
