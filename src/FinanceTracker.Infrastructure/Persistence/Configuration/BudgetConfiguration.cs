using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets", table =>
        {
            table.HasCheckConstraint("CK_Budgets_Amount_Positive", "[Amount] > 0");
            table.HasCheckConstraint("CK_Budgets_PeriodType", "[PeriodType] IN (1, 2, 3)");
            table.HasCheckConstraint("CK_Budgets_Dates", "[EndDate] IS NULL OR [StartDate] <= [EndDate]");
            table.HasCheckConstraint("CK_Budgets_Currency_Length", "LEN([Currency]) = 3");
        });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.Currency).HasMaxLength(3).IsFixedLength().IsUnicode(false).IsRequired();
        builder.Property(b => b.PeriodType).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(b => b.StartDate).HasColumnType("date").IsRequired();
        builder.Property(b => b.EndDate).HasColumnType("date");
        builder.Property(b => b.CreatedAt).IsRequired();

        builder.HasIndex(b => new { b.UserId, b.CategoryId });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // A költségkeret által használt kategória nem törölhető (a törlési szabály ezt előre ellenőrzi).
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
