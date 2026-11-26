using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTiposUsuarioRegularEModerador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoPerfil",
                table: "Usuarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeSeguidores",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspensoAte",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Moderadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CodigoCredencial = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SetorModeracao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalAcoesModeracao = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Moderadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Moderadores_Usuarios_Id",
                        column: x => x.Id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosRegulares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    NivelGamer = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PontosExperiencia = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LimiteListas = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    BiografiaGamer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRegulares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosRegulares_Usuarios_Id",
                        column: x => x.Id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Moderadores_CodigoCredencial",
                table: "Moderadores",
                column: "CodigoCredencial",
                unique: true,
                filter: "[CodigoCredencial] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Moderadores");

            migrationBuilder.DropTable(
                name: "UsuariosRegulares");

            migrationBuilder.DropColumn(
                name: "FotoPerfil",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "QuantidadeSeguidores",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SuspensoAte",
                table: "Usuarios");
        }
    }
}
