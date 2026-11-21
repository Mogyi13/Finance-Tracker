using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class FinancialAnomalyConfiguration : IEntityTypeConfiguration<FinancialAnomaly>
{
    public void Configure(EntityTypeBuilder<FinancialAnomaly> builder)
    {
        builder.ToTable("FinancialAnomalies", table =>
        {
            table.HasCheckConstraint("CK_FinancialAnomalies_AnomalyType", "[AnomalyType] IN (1, 2)");
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AnomalyType).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(a => a.Score).HasPrecision(10, 4);
        builder.Property(a => a.Description).HasMaxLength(FinancialAnomaly.MaxDescriptionLength).IsRequired();
        builder.Property(a => a.DetectedAt).IsRequired();
        builder.Property(a => a.IsReviewed).IsRequired();

        builder.HasIndex(a => new { a.UserId, a.IsReviewed });

        // Egy tranzakcióhoz egy anomáliatípus egyszer tartozhat (az újraelemzés frissíti).
        builder.HasIndex(a => new { a.TransactionId, a.AnomalyType }).IsUnique();

        // A fiók törlése külön üzleti folyamat, ezért nincs kaszkádolt törlés a felhasználóra.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Származtatott adat: a tranzakció törlésével az anomália is törlődik.
        builder.HasOne<Transaction>()
            .WithMany()
            .HasForeignKey(a => a.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
