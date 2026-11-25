using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class CurtidaComentarioConfiguration : IEntityTypeConfiguration<CurtidaComentario>
{
    public void Configure(EntityTypeBuilder<CurtidaComentario> builder)
    {
        builder.HasOne<Comentario>()
            .WithMany()
            .HasForeignKey(cc => cc.IdComentario)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cc => new { cc.IdUsuario, cc.IdComentario })
            .IsUnique()
            .HasFilter("[IdComentario] IS NOT NULL")
            .HasDatabaseName("IX_Curtidas_Usuario_Comentario");
    }
}
