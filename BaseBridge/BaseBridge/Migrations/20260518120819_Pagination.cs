using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseBridge.Migrations
{
    /// <inheritdoc />
    public partial class Pagination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaginationMandatory",
                table: "Endpoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaginationMandatory",
                table: "Endpoints");
        }
    }
}
