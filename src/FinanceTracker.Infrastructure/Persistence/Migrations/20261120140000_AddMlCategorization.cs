using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMlCategorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UC-17: kézi kategóriajavítások naplója (visszajelzés a modellértékeléshez és tanításhoz).
            migrationBuilder.CreateTable(
                name: "CategorizationFeedback",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PredictedCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CorrectedCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorizationFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategorizationFeedback_Categories_CorrectedCategoryId",
                        column: x => x.CorrectedCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategorizationFeedback_Categories_PredictedCategoryId",
                        column: x => x.PredictedCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategorizationFeedback_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // UC-20: modellverziók és kiértékelési eredményeik (a modellfájl nem az adatbázisban van).
            migrationBuilder.CreateTable(
                name: "MLModelVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModelPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Accuracy = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: false),
                    Precision = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: false),
                    Recall = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: false),
                    F1Score = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TrainingDataCount = table.Column<int>(type: "int", nullable: false),
                    TestDataCount = table.Column<int>(type: "int", nullable: false),
                    AveragePredictionMs = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    BaselineAccuracy = table.Column<decimal>(type: "decimal(8,6)", precision: 8, scale: 6, nullable: true),
                    MeetsCriteria = table.Column<bool>(type: "bit", nullable: false),
                    EvaluationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLModelVersions", x => x.Id);
                    table.CheckConstraint("CK_MLModelVersions_ActiveMeetsCriteria", "[IsActive] = 0 OR [MeetsCriteria] = 1");
                    table.CheckConstraint("CK_MLModelVersions_Metrics", "[Accuracy] BETWEEN 0 AND 1 AND [Precision] BETWEEN 0 AND 1 AND [Recall] BETWEEN 0 AND 1 AND [F1Score] BETWEEN 0 AND 1");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategorizationFeedback_CorrectedCategoryId",
                table: "CategorizationFeedback",
                column: "CorrectedCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorizationFeedback_PredictedCategoryId",
                table: "CategorizationFeedback",
                column: "PredictedCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorizationFeedback_TransactionId",
                table: "CategorizationFeedback",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_MLModelVersions_Version",
                table: "MLModelVersions",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_MLModelVersions_SingleActive",
                table: "MLModelVersions",
                column: "IsActive",
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorizationFeedback");

            migrationBuilder.DropTable(
                name: "MLModelVersions");
        }
    }
}
