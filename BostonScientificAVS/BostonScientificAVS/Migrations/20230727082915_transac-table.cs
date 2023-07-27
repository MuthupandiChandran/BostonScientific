using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BostonScientificAVS.Migrations
{
    /// <inheritdoc />
    public partial class transactable : Migration
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
                    Product_Label_GTIN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Carton_Label_GTIN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DB_GTIN = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    WO_Lot_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Product_Lot_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Carton_Lot_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    WO_Catalog_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DB_Catalog_Num = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Shelf_Life = table.Column<int>(type: "int", maxLength: 4, nullable: false),
                    WO_Mfg_Date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Calculated_Use_By = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Product_Use_By = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Carton_Use_By = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DB_Label_Spec = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Product_Label_Spec = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Carton_Label_Spec = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DB_IFU = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Scanned_IFU = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    User = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Date_Time = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rescan_Initated = table.Column<bool>(type: "bit", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Failure_Reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Product_Label_GTIN);
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
