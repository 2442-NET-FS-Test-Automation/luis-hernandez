using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy.Data.Migrations
{
    /// <inheritdoc />
    public partial class FullfilmentEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Dispatchers_DispatcherId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DispatcherId",
                table: "Orders");

            migrationBuilder.DeleteData(
                table: "FulfillmentEvents",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "DispatcherId",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "DispatcherId",
                table: "FulfillmentEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "FulfillmentEvents",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DispatcherId", "FulfilledAtUtc", "Type" },
                values: new object[] { 1, new DateTime(2026, 6, 21, 15, 30, 0, 0, DateTimeKind.Utc), "Fulfilled" });

            migrationBuilder.CreateIndex(
                name: "IX_FulfillmentEvents_DispatcherId",
                table: "FulfillmentEvents",
                column: "DispatcherId");

            migrationBuilder.AddForeignKey(
                name: "FK_FulfillmentEvents_Dispatchers_DispatcherId",
                table: "FulfillmentEvents",
                column: "DispatcherId",
                principalTable: "Dispatchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FulfillmentEvents_Dispatchers_DispatcherId",
                table: "FulfillmentEvents");

            migrationBuilder.DropIndex(
                name: "IX_FulfillmentEvents_DispatcherId",
                table: "FulfillmentEvents");

            migrationBuilder.DropColumn(
                name: "DispatcherId",
                table: "FulfillmentEvents");

            migrationBuilder.AddColumn<int>(
                name: "DispatcherId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "FulfillmentEvents",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FulfilledAtUtc", "Type" },
                values: new object[] { new DateTime(2026, 6, 20, 10, 0, 0, 0, DateTimeKind.Utc), "Created" });

            migrationBuilder.InsertData(
                table: "FulfillmentEvents",
                columns: new[] { "Id", "FulfilledAtUtc", "OrderId", "Type" },
                values: new object[] { 2, new DateTime(2026, 6, 21, 15, 30, 0, 0, DateTimeKind.Utc), 1, "Shipped" });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                column: "DispatcherId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                column: "DispatcherId",
                value: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DispatcherId",
                table: "Orders",
                column: "DispatcherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Dispatchers_DispatcherId",
                table: "Orders",
                column: "DispatcherId",
                principalTable: "Dispatchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
