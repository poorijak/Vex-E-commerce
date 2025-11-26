using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vex_E_commerce.Migrations
{
    /// <inheritdoc />
    public partial class addvaraintidinorderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orderItems_ProductVariants_variantId",
                table: "orderItems");

            migrationBuilder.RenameColumn(
                name: "variantId",
                table: "orderItems",
                newName: "VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_orderItems_variantId",
                table: "orderItems",
                newName: "IX_orderItems_VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_orderItems_ProductVariants_VariantId",
                table: "orderItems",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orderItems_ProductVariants_VariantId",
                table: "orderItems");

            migrationBuilder.RenameColumn(
                name: "VariantId",
                table: "orderItems",
                newName: "variantId");

            migrationBuilder.RenameIndex(
                name: "IX_orderItems_VariantId",
                table: "orderItems",
                newName: "IX_orderItems_variantId");

            migrationBuilder.AddForeignKey(
                name: "FK_orderItems_ProductVariants_variantId",
                table: "orderItems",
                column: "variantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
