using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentCar.Migrations
{
    public partial class NewEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "TrMaintenance",
                columns: table => new
                {
                    Maintenance_Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Maintanance_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Car_id = table.Column<string>(type: "nvarchar(36)", nullable: false),
                    Employee_id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrMaintenance", x => x.Maintenance_Id);
                    table.ForeignKey(
                        name: "FK_TrMaintenance_AspNetUsers_Employee_id",
                        column: x => x.Employee_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrMaintenance_MsCar_Car_id",
                        column: x => x.Car_id,
                        principalTable: "MsCar",
                        principalColumn: "Car_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrRental",
                columns: table => new
                {
                    Rental_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Rental_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Return_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Payment_status = table.Column<bool>(type: "bit", nullable: false),
                    Customer_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Car_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrRental", x => x.Rental_id);
                    table.ForeignKey(
                        name: "FK_TrRental_AspNetUsers_Customer_id",
                        column: x => x.Customer_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrRental_MsCar_Car_id",
                        column: x => x.Car_id,
                        principalTable: "MsCar",
                        principalColumn: "Car_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LtPayments",
                columns: table => new
                {
                    Payment_Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Payment_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Payment_method = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rental_id = table.Column<string>(type: "nvarchar(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LtPayments", x => x.Payment_Id);
                    table.ForeignKey(
                        name: "FK_LtPayments_TrRental_Rental_id",
                        column: x => x.Rental_id,
                        principalTable: "TrRental",
                        principalColumn: "Rental_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LtPayments_Rental_id",
                table: "LtPayments",
                column: "Rental_id");

            migrationBuilder.CreateIndex(
                name: "IX_TrMaintenance_Car_id",
                table: "TrMaintenance",
                column: "Car_id");

            migrationBuilder.CreateIndex(
                name: "IX_TrMaintenance_Employee_id",
                table: "TrMaintenance",
                column: "Employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_TrRental_Car_id",
                table: "TrRental",
                column: "Car_id");

            migrationBuilder.CreateIndex(
                name: "IX_TrRental_Customer_id",
                table: "TrRental",
                column: "Customer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LtPayments");

            migrationBuilder.DropTable(
                name: "TrMaintenance");

            migrationBuilder.DropTable(
                name: "TrRental");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
