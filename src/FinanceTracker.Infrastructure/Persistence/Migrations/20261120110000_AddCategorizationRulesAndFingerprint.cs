using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategorizationRulesAndFingerprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UC-13: tárolt tranzakciós ujjlenyomat. A már meglévő tranzakcióknál NULL marad;
            // a duplikációfelismerés ezeket menet közben számolja ki.
            migrationBuilder.AddColumn<string>(
                name: "Fingerprint",
                table: "Transactions",
                type: "char(64)",
                unicode: false,
                fixedLength: true,
                maxLength: 64,
                nullable: true);

            // UC-15, UC-16: kategorizálási szabályok.
            migrationBuilder.CreateTable(
                name: "CategorizationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Keyword = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MatchType = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorizationRules", x => x.Id);
                    table.CheckConstraint("CK_CategorizationRules_MatchType", "[MatchType] IN (1, 2, 3)");
                    table.CheckConstraint("CK_CategorizationRules_Priority", "[Priority] BETWEEN 0 AND 1000000");
                    table.ForeignKey(
                        name: "FK_CategorizationRules_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategorizationRules_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_Fingerprint",
                table: "Transactions",
                columns: new[] { "UserId", "Fingerprint" },
                filter: "[Fingerprint] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CategorizationRules_CategoryId",
                table: "CategorizationRules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorizationRules_UserId_IsActive_Priority",
                table: "CategorizationRules",
                columns: new[] { "UserId", "IsActive", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorizationRules");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_Fingerprint",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Fingerprint",
                table: "Transactions");
        }
    }
}
