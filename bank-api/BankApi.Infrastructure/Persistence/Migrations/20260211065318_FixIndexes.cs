using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AutoPayAccountId",
                table: "CreditCards",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DueDay",
                table: "CreditCards",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_CreditCards_AutoPayAccountId",
                table: "CreditCards",
                column: "AutoPayAccountId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_creditcard_dueday",
                table: "CreditCards",
                sql: "\"DueDay\" >=1 AND \"DueDay\" <=28");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardPaymentInstructions_CreditCardId_ScheduledAtUtc",
                table: "CreditCardPaymentInstructions",
                columns: new[] { "CreditCardId", "ScheduledAtUtc" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditCards_Accounts_AutoPayAccountId",
                table: "CreditCards",
                column: "AutoPayAccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditCards_Accounts_AutoPayAccountId",
                table: "CreditCards");

            migrationBuilder.DropIndex(
                name: "IX_CreditCards_AutoPayAccountId",
                table: "CreditCards");

            migrationBuilder.DropCheckConstraint(
                name: "CK_creditcard_dueday",
                table: "CreditCards");

            migrationBuilder.DropIndex(
                name: "IX_CreditCardPaymentInstructions_CreditCardId_ScheduledAtUtc",
                table: "CreditCardPaymentInstructions");

            migrationBuilder.DropColumn(
                name: "AutoPayAccountId",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "DueDay",
                table: "CreditCards");
        }
    }
}
