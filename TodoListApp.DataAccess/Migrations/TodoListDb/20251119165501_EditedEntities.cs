using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.DataAccess.Migrations.TodoListDb
{
    public partial class EditedEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Comments");
        }
    }
}
