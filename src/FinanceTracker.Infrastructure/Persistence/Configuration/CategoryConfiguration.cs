using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Type).HasConversion<byte>().HasColumnType("tinyint");
        builder.Property(c => c.IsSystemCategory).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        // Egy felhasználónál azonos típusú, azonos nevű kategória nem fordulhat elő kétszer.
        builder.HasIndex(c => new { c.UserId, c.Name, c.Type }).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            SystemCategory("00000000-0000-0000-0000-000000000001", "Fizetés", CategoryType.Income),
            SystemCategory("00000000-0000-0000-0000-000000000002", "Egyéb bevétel", CategoryType.Income),
            SystemCategory("00000000-0000-0000-0000-000000000011", "Élelmiszer", CategoryType.Expense),
            SystemCategory("00000000-0000-0000-0000-000000000012", "Lakhatás", CategoryType.Expense),
            SystemCategory("00000000-0000-0000-0000-000000000013", "Közlekedés", CategoryType.Expense),
            SystemCategory("00000000-0000-0000-0000-000000000014", "Szórakozás", CategoryType.Expense),
            SystemCategory("00000000-0000-0000-0000-000000000015", "Egészség", CategoryType.Expense),
            SystemCategory("00000000-0000-0000-0000-000000000016", "Egyéb kiadás", CategoryType.Expense));
    }

    private static Category SystemCategory(string id, string name, CategoryType type) => new()
    {
        Id = Guid.Parse(id),
        UserId = null,
        Name = name,
        Type = type,
        IsSystemCategory = true,
        CreatedAt = SeedDate
    };
}
