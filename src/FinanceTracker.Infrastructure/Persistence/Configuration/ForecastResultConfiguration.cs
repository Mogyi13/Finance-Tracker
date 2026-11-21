using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class ForecastResultConfiguration : IEntityTypeConfiguration<ForecastResult>
{
    public void Configure(EntityTypeBuilder<ForecastResult> builder)
    {
        builder.ToTable("ForecastResults", table =>
        {
            table.HasCheckConstraint("CK_ForecastResults_ForecastType", "[ForecastType] IN (1, 2)");
            table.HasCheckConstraint("CK_ForecastResults_Method", "[Method] IN (1, 2)");
            table.HasCheckConstraint("CK_ForecastResults_Period", "[PeriodStart] <= [PeriodEnd]");
            table.HasCheckConstraint("CK_ForecastResults_Currency_Length", "LEN([Currency]) = 3");
        });

        builder.HasKey(f => f.Id);

        builder.Property(f => f.ForecastType).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(f => f.Method).HasConversion<byte>().HasColumnType("tinyint").IsRequired();
        builder.Property(f => f.Currency).HasMaxLength(3).IsFixedLength().IsUnicode(false).IsRequired();
        builder.Property(f => f.PeriodStart).HasColumnType("date").IsRequired();
        builder.Property(f => f.PeriodEnd).HasColumnType("date").IsRequired();
        builder.Property(f => f.PredictedAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasIndex(f => new { f.UserId, f.ForecastType, f.PeriodStart });

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Származtatott adat: a hivatkozott modellverzió törlésekor a hivatkozás megszűnik.
        builder.HasOne<MlModelVersion>()
            .WithMany()
            .HasForeignKey(f => f.ModelVersionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
