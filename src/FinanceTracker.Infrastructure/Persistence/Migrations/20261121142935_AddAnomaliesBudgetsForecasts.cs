using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnomaliesBudgetsForecasts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    PeriodType = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.CheckConstraint("CK_Budgets_Amount_Positive", "[Amount] > 0");
                    table.CheckConstraint("CK_Budgets_Currency_Length", "LEN([Currency]) = 3");
                    table.CheckConstraint("CK_Budgets_Dates", "[EndDate] IS NULL OR [StartDate] <= [EndDate]");
                    table.CheckConstraint("CK_Budgets_PeriodType", "[PeriodType] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_Budgets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgets_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialAnomalies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnomalyType = table.Column<byte>(type: "tinyint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAnomalies", x => x.Id);
                    table.CheckConstraint("CK_FinancialAnomalies_AnomalyType", "[AnomalyType] IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_FinancialAnomalies_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialAnomalies_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForecastResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForecastType = table.Column<byte>(type: "tinyint", nullable: false),
                    Method = table.Column<byte>(type: "tinyint", nullable: false),
                    Currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    PredictedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModelVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastResults", x => x.Id);
                    table.CheckConstraint("CK_ForecastResults_Currency_Length", "LEN([Currency]) = 3");
                    table.CheckConstraint("CK_ForecastResults_ForecastType", "[ForecastType] IN (1, 2)");
                    table.CheckConstraint("CK_ForecastResults_Method", "[Method] IN (1, 2)");
                    table.CheckConstraint("CK_ForecastResults_Period", "[PeriodStart] <= [PeriodEnd]");
                    table.ForeignKey(
                        name: "FK_ForecastResults_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastResults_MLModelVersions_ModelVersionId",
                        column: x => x.ModelVersionId,
                        principalTable: "MLModelVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CategoryId",
                table: "Budgets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_UserId_CategoryId",
                table: "Budgets",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAnomalies_TransactionId_AnomalyType",
                table: "FinancialAnomalies",
                columns: new[] { "TransactionId", "AnomalyType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAnomalies_UserId_IsReviewed",
                table: "FinancialAnomalies",
                columns: new[] { "UserId", "IsReviewed" });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastResults_ModelVersionId",
                table: "ForecastResults",
                column: "ModelVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastResults_UserId_ForecastType_PeriodStart",
                table: "ForecastResults",
                columns: new[] { "UserId", "ForecastType", "PeriodStart" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropTable(
                name: "FinancialAnomalies");

            migrationBuilder.DropTable(
                name: "ForecastResults");
        }
    }
}
