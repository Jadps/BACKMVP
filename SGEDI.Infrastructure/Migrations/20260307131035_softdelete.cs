using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGEDI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class softdelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "Tenants",
                newName: "Borrado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Borrado",
                table: "Tenants",
                newName: "Activo");
        }
    }
}
