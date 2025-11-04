    using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server_API_With_SignalR_For_Messager_01.Migrations
{
    /// <inheritdoc />
    public partial class addIsseenToMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "Messages");
        }

    }
}
