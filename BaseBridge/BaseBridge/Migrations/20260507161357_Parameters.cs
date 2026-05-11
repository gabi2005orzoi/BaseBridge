using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseBridge.Migrations
{
    /// <inheritdoc />
    public partial class Parameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EndpointParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EndpointDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndpointParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndpointParameters_Endpoints_EndpointDataId",
                        column: x => x.EndpointDataId,
                        principalTable: "Endpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EndpointParameters_EndpointDataId",
                table: "EndpointParameters",
                column: "EndpointDataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EndpointParameters");
        }
    }
}
