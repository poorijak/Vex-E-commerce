using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vex_E_commerce.Data.Migrations
{
    /// <inheritdoc />
    public partial class addtotalstock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalStock",
                table: "Products",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalStock",
                table: "Products");
        }
    }
}
