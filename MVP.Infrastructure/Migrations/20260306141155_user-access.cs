using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MVP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Usuarios_Roles_RolId", table: "Usuarios");
            migrationBuilder.DropIndex(name: "IX_Usuarios_RolId", table: "Usuarios");

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Dominio = table.Column<string>(type: "text", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });


            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Roles",
                type: "integer",
                nullable: true);

            migrationBuilder.DropColumn(name: "ChatId", table: "Usuarios");
            migrationBuilder.DropColumn(name: "Firmante", table: "Usuarios");
            migrationBuilder.DropColumn(name: "PersonalID", table: "Usuarios");
            migrationBuilder.DropColumn(name: "RolId", table: "Usuarios");
            migrationBuilder.DropColumn(name: "SuperUser", table: "Usuarios");
            migrationBuilder.DropColumn(name: "UbicacionId", table: "Usuarios");

            migrationBuilder.CreateIndex(name: "IX_Usuarios_TenantId", table: "Usuarios", column: "TenantId");
            migrationBuilder.CreateIndex(name: "IX_Roles_TenantId", table: "Roles", column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Tenants_TenantId",
                table: "Roles",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Tenants_TenantId",
                table: "Usuarios",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Tenants_TenantId",
                table: "Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Tenants_TenantId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_TenantId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Roles");

            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "Usuarios",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Firmante",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PersonalID",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RolId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SuperUser",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UbicacionId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Roles_RolId",
                table: "Usuarios",
                column: "RolId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
