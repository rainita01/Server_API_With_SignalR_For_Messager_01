using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server_API_With_SignalR_For_Messager_01.Migrations
{
    /// <inheritdoc />
    public partial class updateConversationGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminsId",
                table: "Conversations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Conversations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminsId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Conversations");
        }
    }
}
