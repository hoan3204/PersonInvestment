using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalInvestmentSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class Upadatewallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Wallets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Wallets");
        }
    }
}
