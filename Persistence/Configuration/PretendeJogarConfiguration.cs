using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class PretendeJogarConfiguration : IEntityTypeConfiguration<PretendeJogar>
{
    public void Configure(EntityTypeBuilder<PretendeJogar> builder)
    {
        builder.ToTable("PretendeJogar");

        builder.Property(p => p.NivelPrioridade)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(p => p.NotificarEmPromocao)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.MetaAnoConclusao);
    }
}
