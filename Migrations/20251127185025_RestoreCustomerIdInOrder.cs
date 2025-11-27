using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vex_E_commerce.Migrations
{
    /// <inheritdoc />
    public partial class RestoreCustomerIdInOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) เพิ่ม column customerId กลับเข้าไปใน orders
            migrationBuilder.AddColumn<string>(
                name: "customerId",
                table: "orders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: ""); // ถ้ามีข้อมูลเดิมอยู่ จะใส่ค่าเริ่มต้นเป็น "" ให้หมด

            // 2) สร้าง Index สำหรับ customerId
            migrationBuilder.CreateIndex(
                name: "IX_orders_customerId",
                table: "orders",
                column: "customerId");

            // 3) ผูก FK กลับไปที่ AspNetUsers(Id)
            migrationBuilder.AddForeignKey(
                name: "FK_orders_AspNetUsers_customerId",
                table: "orders",
                column: "customerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id"
            // ไม่ใส่ onDelete => SQL Server จะเป็น NO ACTION (ไม่ cascade)
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ถ้า rollback migration ออก ให้ลบ FK / Index / Column นี้
            migrationBuilder.DropForeignKey(
                name: "FK_orders_AspNetUsers_customerId",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_customerId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customerId",
                table: "orders");
        }
    }
}
