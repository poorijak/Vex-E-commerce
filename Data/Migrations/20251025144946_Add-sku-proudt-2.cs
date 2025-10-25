using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vex_E_commerce.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addskuproudt2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "ProductVariants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sku",
                table: "ProductVariants");
        }
    }
}
