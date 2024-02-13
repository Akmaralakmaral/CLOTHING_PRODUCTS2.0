using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLOTHING_PRODUCTS.Migrations
{
    /// <inheritdoc />
    public partial class SaleProduct20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaleProducts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinishedProductID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<double>(type: "float", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleProducts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SaleProducts_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleProducts_FinishedProducts_FinishedProductID",
                        column: x => x.FinishedProductID,
                        principalTable: "FinishedProducts",
                        principalColumn: "FinishedProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleProducts_EmployeeID",
                table: "SaleProducts",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_SaleProducts_FinishedProductID",
                table: "SaleProducts",
                column: "FinishedProductID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleProducts");
        }
    }
}
