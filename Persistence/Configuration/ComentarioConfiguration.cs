using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("Comentarios");

        builder.HasKey(c => c.IdComentario);

        builder.Property(c => c.Conteudo)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.QuantidadeCurtidas)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.DataCriacao)
            .IsRequired();

        builder.HasOne<Post>()
            .WithMany()
            .HasForeignKey(c => c.IdPost)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.IdPost);
    }
}
