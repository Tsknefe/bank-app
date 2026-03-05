using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDebitCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DebitCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CardNo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cvv = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitCards_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DebitCards_AccountId",
                table: "DebitCards",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitCards_CardNo",
                table: "DebitCards",
                column: "CardNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DebitCards");
        }
    }
}
