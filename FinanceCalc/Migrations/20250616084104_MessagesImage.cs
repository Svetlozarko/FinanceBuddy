using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceCalc.Migrations
{
    /// <inheritdoc />
    public partial class MessagesImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "InboxMessage",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "InboxMessage");
        }
    }
}
