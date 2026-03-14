using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class refactorUid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Uid",
                table: "AuditLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE \"AuditLogs\" SET \"Uid\" = md5(random()::text || clock_timestamp()::text)::uuid WHERE \"Uid\" = '00000000-0000-0000-0000-000000000000'");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Uid",
                table: "Usuarios",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Uid",
                table: "Tenants",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Uid",
                table: "Roles",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_Uid",
                table: "Modulos",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_Uid",
                table: "Documentos",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Uid",
                table: "AuditLogs",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Archivos_Uid",
                table: "Archivos",
                column: "Uid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Uid",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Uid",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Uid",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Modulos_Uid",
                table: "Modulos");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_Uid",
                table: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Uid",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_Archivos_Uid",
                table: "Archivos");

            migrationBuilder.DropColumn(
                name: "Uid",
                table: "AuditLogs");
        }
    }
}
