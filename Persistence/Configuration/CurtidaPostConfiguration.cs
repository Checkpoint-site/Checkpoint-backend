using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class CurtidaPostConfiguration : IEntityTypeConfiguration<CurtidaPost>
{
    public void Configure(EntityTypeBuilder<CurtidaPost> builder)
    {
        builder.HasOne<Post>()
            .WithMany()
            .HasForeignKey(cp => cp.IdPost)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cp => new { cp.IdUsuario, cp.IdPost })
            .IsUnique()
            .HasFilter("[IdPost] IS NOT NULL")
            .HasDatabaseName("IX_Curtidas_Usuario_Post");
    }
}
