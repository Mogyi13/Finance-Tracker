using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class ImportErrorConfiguration : IEntityTypeConfiguration<ImportError>
{
    public void Configure(EntityTypeBuilder<ImportError> builder)
    {
        builder.ToTable("ImportErrors");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RowNumber).IsRequired();
        builder.Property(e => e.ErrorCode).IsRequired().HasMaxLength(ImportError.MaxErrorCodeLength);
        builder.Property(e => e.ErrorMessage).IsRequired().HasMaxLength(ImportError.MaxErrorMessageLength);

        // Importhibák lekérdezése sorszám szerint.
        builder.HasIndex(e => new { e.ImportBatchId, e.RowNumber });

        // Az importfolyamat törlésekor a hozzá tartozó hibák is törlődnek.
        builder.HasOne<ImportBatch>()
            .WithMany()
            .HasForeignKey(e => e.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
