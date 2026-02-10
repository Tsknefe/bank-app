using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCvvHashSalt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cvv",
                table: "DebitCards");

            migrationBuilder.DropColumn(
                name: "Cvv",
                table: "CreditCards");

            migrationBuilder.AddColumn<byte[]>(
                name: "CvvHash",
                table: "DebitCards",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "CvvSalt",
                table: "DebitCards",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "CvvHash",
                table: "CreditCards",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "CvvSalt",
                table: "CreditCards",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "CardTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreditCardId = table.Column<Guid>(type: "uuid", nullable: true),
                    DebitCardId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardTransactions", x => x.Id);
                    table.CheckConstraint("CK_card_tx_exactly_one_card", "((\"CreditCardId\" IS NOT NULL AND \"DebitCardId\" IS NULL)\r\n            OR (\"CreditCardId\" IS NULL AND \"DebitCardId\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_CardTransactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CardTransactions_CreditCards_CreditCardId",
                        column: x => x.CreditCardId,
                        principalTable: "CreditCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CardTransactions_DebitCards_DebitCardId",
                        column: x => x.DebitCardId,
                        principalTable: "DebitCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreditCardPaymentInstructions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditCardId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ScheduledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardPaymentInstructions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditCardPaymentInstructions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditCardPaymentInstructions_CreditCards_CreditCardId",
                        column: x => x.CreditCardId,
                        principalTable: "CreditCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_AccountId",
                table: "CardTransactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_CreatedAt",
                table: "CardTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_CreditCardId",
                table: "CardTransactions",
                column: "CreditCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardTransactions_DebitCardId",
                table: "CardTransactions",
                column: "DebitCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardPaymentInstructions_AccountId",
                table: "CreditCardPaymentInstructions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardPaymentInstructions_CreditCardId",
                table: "CreditCardPaymentInstructions",
                column: "CreditCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardPaymentInstructions_Status_ScheduledAtUtc",
                table: "CreditCardPaymentInstructions",
                columns: new[] { "Status", "ScheduledAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardTransactions");

            migrationBuilder.DropTable(
                name: "CreditCardPaymentInstructions");

            migrationBuilder.DropColumn(
                name: "CvvHash",
                table: "DebitCards");

            migrationBuilder.DropColumn(
                name: "CvvSalt",
                table: "DebitCards");

            migrationBuilder.DropColumn(
                name: "CvvHash",
                table: "CreditCards");

            migrationBuilder.DropColumn(
                name: "CvvSalt",
                table: "CreditCards");

            migrationBuilder.AddColumn<string>(
                name: "Cvv",
                table: "DebitCards",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cvv",
                table: "CreditCards",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }
    }
}
