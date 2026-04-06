using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalInvestmentSystem.Web.Migrations
{
    /// <inheritdoc />
    public partial class updateTrash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "InvestmentProducts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "InvestmentProducts");
        }
    }
}
