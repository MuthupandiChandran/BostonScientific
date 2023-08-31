using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BostonScientificAVS.Migrations
{
    /// <inheritdoc />
    public partial class Carton_Use_ByColmnUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Carton_Use_By",
                table: "Transaction",
                type: "DateTime",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );
            //migrationBuilder.AlterColumn<DateTime>("Transaction", "Carton_Use_By", c => c.DateTime(Nullable: false));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Carton_Use_By",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DateTime"
            );
        }
    }
}
