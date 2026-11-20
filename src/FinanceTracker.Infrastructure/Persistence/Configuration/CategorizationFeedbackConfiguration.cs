using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class CategorizationFeedbackConfiguration : IEntityTypeConfiguration<CategorizationFeedback>
{
    public void Configure(EntityTypeBuilder<CategorizationFeedback> builder)
    {
        builder.ToTable("CategorizationFeedback");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasIndex(f => f.TransactionId);

        // A tranzakció törlésével a hozzá tartozó visszajelzés is törlődik (a tranzakció nélkül nincs értelmezhető tanítópélda).
        builder.HasOne<Transaction>()
            .WithMany()
            .HasForeignKey(f => f.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // A visszajelzésben szereplő kategóriák nem törölhetők, amíg a napló hivatkozik rájuk.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(f => f.PredictedCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(f => f.CorrectedCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
