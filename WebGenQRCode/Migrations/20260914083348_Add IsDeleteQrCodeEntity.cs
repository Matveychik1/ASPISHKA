using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebGenQRCode.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeleteQrCodeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "tblQrCodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "tblQrCodes");
        }
    }
}
