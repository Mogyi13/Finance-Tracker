using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
{
    public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
    {
        builder.ToTable("RecurringTransactions", table =>
        {
            table.HasCheckConstraint("CK_RecurringTransactions_ExpectedAmount_Positive", "[ExpectedAmount] > 0");
            table.HasCheckConstraint("CK_RecurringTransactions_Frequency", "[Frequency] IN (1, 2, 3, 4)");
            table.HasCheckConstraint("CK_RecurringTransactions_Confidence", "[Confidence] BETWEEN 0 AND 1");
            table.HasCheckConstraint("CK_RecurringTransactions_Currency_Length", "LEN([Currency]) = 3");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Description).HasMaxLength(RecurringTransaction.MaxDescriptionLength).IsRequired();
        builder.Property(r => r.ExpectedAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.Currency).HasMaxLength(3).IsFixedLength().IsUnicode(false).IsRequired();
        builder.Property(r => r.Frequency).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(r => r.LastOccurrence).HasColumnType("date").IsRequired();
        builder.Property(r => r.Confidence).HasPrecision(5, 4).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        // Egy felhasználónál ugyanaz a minta (leírás, pénznem, kategória) csak egyszer szerepelhet.
        builder.HasIndex(r => new { r.UserId, r.Currency, r.Description, r.CategoryId })
                .IsUnique()
                .HasFilter(null);

        builder.HasIndex(r => r.CategoryId);

        // A fiók törlése külön üzleti folyamat, ezért nincs kaszkádolt törlés.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Származtatott adat: a kategória törlésekor a hivatkozás megszűnik, a minta megmarad (a következő elemzés újraszámolja).
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
