using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BostonScientificAVS.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemMaster",
                columns: table => new
                {
                    GTIN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Catalog_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Shelf_Life = table.Column<int>(type: "int", nullable: true),
                    Label_Spec = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IFU = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Edit_Date_Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Edit_By = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_by = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMaster", x => x.GTIN);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Transaction_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product_Label_GTIN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    Carton_Label_GTIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DB_GTIN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    WO_Lot_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Product_Lot_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Carton_Lot_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    WO_Catalog_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DB_Catalog_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Shelf_Life = table.Column<int>(type: "int", maxLength: 4, nullable: false),
                    WO_Mfg_Date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Calculated_Use_By = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Product_Use_By = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Carton_Use_By = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DB_Label_Spec = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Product_Label_Spec = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Carton_Label_Spec = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DB_IFU = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Scanned_IFU = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    User = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Date_Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rescan_Initated = table.Column<bool>(type: "bit", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Failure_Reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Transaction_Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserFullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserRole = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmpID",
                table: "Users",
                column: "EmpID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemMaster");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
