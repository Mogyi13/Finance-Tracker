using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches", table =>
        {
            table.HasCheckConstraint("CK_ImportBatches_Status", "[Status] BETWEEN 1 AND 5");
            table.HasCheckConstraint(
                "CK_ImportBatches_Counts",
                "[TotalRows] >= 0 AND [SuccessfulRows] >= 0 AND [FailedRows] >= 0 AND [DuplicateRows] >= 0");
        });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.FileName).IsRequired().HasMaxLength(ImportBatch.MaxFileNameLength);
        builder.Property(b => b.Status).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(b => b.TotalRows).IsRequired();
        builder.Property(b => b.SuccessfulRows).IsRequired();
        builder.Property(b => b.FailedRows).IsRequired();
        builder.Property(b => b.DuplicateRows).IsRequired();
        builder.Property(b => b.CreatedAt).IsRequired();

        // Importelőzmények listázása.
        builder.HasIndex(b => new { b.UserId, b.CreatedAt });

        // A felhasználói fiók törlése külön üzleti folyamat, ezért nincs kaszkádolt törlés.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
