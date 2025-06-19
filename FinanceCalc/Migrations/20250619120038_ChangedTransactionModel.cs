using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceCalc.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTransactionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpenseCategoryId",
                table: "Transaction",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "ExpenseCategory",
                table: "Transaction",
                newName: "Category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Transaction",
                newName: "ExpenseCategoryId");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Transaction",
                newName: "ExpenseCategory");
        }
    }
}
