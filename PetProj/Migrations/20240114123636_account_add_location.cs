using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetProj.Migrations
{
    /// <inheritdoc />
    public partial class account_add_location : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountRegistrationLocation",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountRegistrationLocation",
                table: "Accounts");
        }
    }
}
