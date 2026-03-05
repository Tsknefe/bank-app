using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCardTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "CardTransactions",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_CardTransactions_CreatedAt",
                table: "CardTransactions",
                newName: "IX_CardTransactions_CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "CardTransactions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_CardTransactions_CreatedAtUtc",
                table: "CardTransactions",
                newName: "IX_CardTransactions_CreatedAt");
        }
    }
}
