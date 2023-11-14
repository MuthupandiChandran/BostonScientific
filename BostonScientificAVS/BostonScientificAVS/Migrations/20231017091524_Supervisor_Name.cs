using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BostonScientificAVS.Migrations
{
    /// <inheritdoc />
    public partial class Supervisor_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Supervisor_Name",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Supervisor_Name",
                table: "Transaction");
        }
    }
}
