using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", table =>
        {
            table.HasCheckConstraint("CK_Transactions_Amount_Positive", "[Amount] > 0");
            table.HasCheckConstraint("CK_Transactions_Type", "[Type] IN (1, 2)");
            table.HasCheckConstraint("CK_Transactions_Source", "[Source] IN (1, 2)");
            table.HasCheckConstraint("CK_Transactions_CategorizationStatus", "[CategorizationStatus] BETWEEN 0 AND 3");
            table.HasCheckConstraint("CK_Transactions_Currency_Length", "LEN([Currency]) = 3");
        });

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.Currency).HasMaxLength(3).IsFixedLength().IsUnicode(false).IsRequired();
        builder.Property(t => t.Type).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.TransactionDate).HasColumnType("date").IsRequired();
        builder.Property(t => t.Source).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(t => t.CategorizationStatus).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();

        // UC-13: SHA-256 ujjlenyomat (64 hexa karakter); a régebbi tranzakcióknál NULL.
        builder.Property(t => t.Fingerprint).HasMaxLength(64).IsFixedLength().IsUnicode(false);

        // Felhasználói tranzakciók listázása, kategóriánkénti elemzés, bevétel/kiadás szűrés.
        builder.HasIndex(t => new { t.UserId, t.TransactionDate });
        builder.HasIndex(t => new { t.UserId, t.CategoryId, t.TransactionDate });
        builder.HasIndex(t => new { t.UserId, t.Type, t.TransactionDate });

        // Duplikációfelismerés (UC-13). Szűrt, de nem egyedi index: két valódi, azonos adatú tranzakció is létezhet
        // (például egy napon kétszer vásárolt azonos jegy), ezért a duplikációt az import menet közben értékeli.
        builder.HasIndex(t => new { t.UserId, t.Fingerprint })
            .HasFilter("[Fingerprint] IS NOT NULL");

        // A felhasználói fiók törlése külön üzleti folyamat, ezért nincs kaszkádolt törlés.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // A tranzakcióra hivatkozott kategória nem törölhető.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
