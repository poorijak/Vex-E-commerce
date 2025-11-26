using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vex_E_commerce.Migrations
{
    /// <inheritdoc />
    public partial class addcustomerIdinorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_AspNetUsers_CustomerId",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "orders",
                newName: "customerId");

            migrationBuilder.RenameIndex(
                name: "IX_orders_CustomerId",
                table: "orders",
                newName: "IX_orders_customerId");

            migrationBuilder.AlterColumn<string>(
                name: "customerId",
                table: "orders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_AspNetUsers_customerId",
                table: "orders",
                column: "customerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_AspNetUsers_customerId",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "customerId",
                table: "orders",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_orders_customerId",
                table: "orders",
                newName: "IX_orders_CustomerId");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerId",
                table: "orders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_orders_AspNetUsers_CustomerId",
                table: "orders",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
