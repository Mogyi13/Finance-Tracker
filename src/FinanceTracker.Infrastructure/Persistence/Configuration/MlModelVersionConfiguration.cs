using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class MlModelVersionConfiguration : IEntityTypeConfiguration<MlModelVersion>
{
    public void Configure(EntityTypeBuilder<MlModelVersion> builder)
    {
        builder.ToTable("MLModelVersions", table =>
        {
            // Csak a feltételeknek megfelelő verzió lehet aktív.
            table.HasCheckConstraint("CK_MLModelVersions_ActiveMeetsCriteria", "[IsActive] = 0 OR [MeetsCriteria] = 1");
            table.HasCheckConstraint(
                "CK_MLModelVersions_Metrics",
                "[Accuracy] BETWEEN 0 AND 1 AND [Precision] BETWEEN 0 AND 1 AND [Recall] BETWEEN 0 AND 1 AND [F1Score] BETWEEN 0 AND 1");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Version).IsRequired().HasMaxLength(MlModelVersion.MaxVersionLength);
        builder.Property(m => m.ModelPath).IsRequired().HasMaxLength(MlModelVersion.MaxModelPathLength);
        builder.Property(m => m.Accuracy).HasPrecision(8, 6).IsRequired();
        builder.Property(m => m.Precision).HasPrecision(8, 6).IsRequired();
        builder.Property(m => m.Recall).HasPrecision(8, 6).IsRequired();
        builder.Property(m => m.F1Score).HasPrecision(8, 6).IsRequired();
        builder.Property(m => m.BaselineAccuracy).HasPrecision(8, 6);
        builder.Property(m => m.AveragePredictionMs).HasPrecision(12, 4).IsRequired();
        builder.Property(m => m.IsActive).IsRequired();
        builder.Property(m => m.MeetsCriteria).IsRequired();
        builder.Property(m => m.TrainingDataCount).IsRequired();
        builder.Property(m => m.TestDataCount).IsRequired();
        builder.Property(m => m.EvaluationJson).HasColumnType("nvarchar(max)");
        builder.Property(m => m.CreatedAt).IsRequired();

        builder.HasIndex(m => m.Version).IsUnique();

        // Egyszerre legfeljebb egy aktív modell lehet.
        builder.HasIndex(m => m.IsActive)
            .IsUnique()
            .HasDatabaseName("UX_MLModelVersions_SingleActive")
            .HasFilter("[IsActive] = 1");
    }
}
