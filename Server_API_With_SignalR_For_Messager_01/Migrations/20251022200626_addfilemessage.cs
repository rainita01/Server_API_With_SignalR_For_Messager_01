using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server_API_With_SignalR_For_Messager_01.Migrations
{
    /// <inheritdoc />
    public partial class addfilemessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "Messages",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileMessage_Title",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileData",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "FileMessage_Title",
                table: "Messages");
        }
    }
}
