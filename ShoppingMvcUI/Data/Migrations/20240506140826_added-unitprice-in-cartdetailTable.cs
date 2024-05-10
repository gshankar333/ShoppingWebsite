﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingMvcUI.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedunitpriceincartdetailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "UnitPrice",
                table: "CartDetail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "CartDetail");
        }
    }
}
