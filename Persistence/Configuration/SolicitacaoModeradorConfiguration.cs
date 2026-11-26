using Domain.Enum;
using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration;

public class SolicitacaoModeradorConfiguration : IEntityTypeConfiguration<SolicitacaoModerador>
{
    public void Configure(EntityTypeBuilder<SolicitacaoModerador> builder)
    {
        builder.ToTable("SolicitacoesModerador");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Motivo)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40)
            .HasDefaultValue(StatusSolicitacaoModerador.Pendente)
            .HasSentinel((StatusSolicitacaoModerador)0);

        builder.Property(e => e.MotivoRecusa)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(e => e.DataCriacao)
            .IsRequired();

        builder.Property(e => e.DataAtualizacao)
            .IsRequired();

        builder.HasOne(e => e.UsuarioSolicitante)
            .WithMany()
            .HasForeignKey(e => e.IdUsuarioSolicitante)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PrimeiroAprovador)
            .WithMany()
            .HasForeignKey(e => e.IdPrimeiroAprovador)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SegundoAprovador)
            .WithMany()
            .HasForeignKey(e => e.IdSegundoAprovador)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AvaliadorRecusa)
            .WithMany()
            .HasForeignKey(e => e.IdAvaliadorRecusa)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.IdUsuarioSolicitante, e.Status });
    }
}
