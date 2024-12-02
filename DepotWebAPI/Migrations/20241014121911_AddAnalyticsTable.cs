using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepotWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Analytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Home = table.Column<int>(type: "INTEGER", nullable: false),
                    Store = table.Column<int>(type: "INTEGER", nullable: false),
                    Implementation = table.Column<int>(type: "INTEGER", nullable: false),
                    AboutUs = table.Column<int>(type: "INTEGER", nullable: false),
                    Contact = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analytics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Analytics");
        }
    }
}
