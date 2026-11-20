using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class CategorizationRuleConfiguration : IEntityTypeConfiguration<CategorizationRule>
{
    public void Configure(EntityTypeBuilder<CategorizationRule> builder)
    {
        builder.ToTable("CategorizationRules", table =>
        {
            table.HasCheckConstraint("CK_CategorizationRules_MatchType", "[MatchType] IN (1, 2, 3)");
            table.HasCheckConstraint(
                "CK_CategorizationRules_Priority",
                $"[Priority] BETWEEN {CategorizationRule.MinPriority} AND {CategorizationRule.MaxPriority}");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Keyword).IsRequired().HasMaxLength(CategorizationRule.MaxKeywordLength);
        builder.Property(r => r.MatchType).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(r => r.Priority).IsRequired();
        builder.Property(r => r.IsActive).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        // Az aktív szabályok prioritás szerinti lekérése.
        builder.HasIndex(r => new { r.UserId, r.IsActive, r.Priority });

        // A felhasználói fiók törlése külön üzleti folyamat, ezért nincs kaszkádolt törlés.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // A szabály által használt kategória nem törölhető.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
