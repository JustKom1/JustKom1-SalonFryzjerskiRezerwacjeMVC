using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rezerwacje.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSalonDayOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalonDayOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalonDayOverrides", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalonDayOverrides");
        }
    }
}
