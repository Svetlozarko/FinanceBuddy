using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceCalc.Migrations
{
    /// <inheritdoc />
    public partial class AddSavingGoalAndInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GoalAmount",
                table: "SavingGoals",
                newName: "TargetAmount");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SavingGoals",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentAmount",
                table: "SavingGoals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "SavingGoals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "SavingGoals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SavingGoals_UserId",
                table: "SavingGoals",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingGoals_AspNetUsers_UserId",
                table: "SavingGoals",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingGoals_AspNetUsers_UserId",
                table: "SavingGoals");

            migrationBuilder.DropIndex(
                name: "IX_SavingGoals_UserId",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "CurrentAmount",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "SavingGoals");

            migrationBuilder.RenameColumn(
                name: "TargetAmount",
                table: "SavingGoals",
                newName: "GoalAmount");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SavingGoals",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
