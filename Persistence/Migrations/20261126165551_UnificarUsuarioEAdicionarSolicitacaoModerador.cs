using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UnificarUsuarioEAdicionarSolicitacaoModerador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Moderadores");

            migrationBuilder.DropTable(
                name: "UsuariosRegulares");

            migrationBuilder.AddColumn<string>(
                name: "TipoPerfil",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Regular");

            migrationBuilder.CreateTable(
                name: "SolicitacoesModerador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuarioSolicitante = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValue: "Pendente"),
                    IdPrimeiroAprovador = table.Column<int>(type: "int", nullable: true),
                    DataPrimeiraAprovacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdSegundoAprovador = table.Column<int>(type: "int", nullable: true),
                    DataSegundaAprovacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdAvaliadorRecusa = table.Column<int>(type: "int", nullable: true),
                    MotivoRecusa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacoesModerador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacoesModerador_Usuarios_IdAvaliadorRecusa",
                        column: x => x.IdAvaliadorRecusa,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitacoesModerador_Usuarios_IdPrimeiroAprovador",
                        column: x => x.IdPrimeiroAprovador,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitacoesModerador_Usuarios_IdSegundoAprovador",
                        column: x => x.IdSegundoAprovador,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitacoesModerador_Usuarios_IdUsuarioSolicitante",
                        column: x => x.IdUsuarioSolicitante,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesModerador_IdAvaliadorRecusa",
                table: "SolicitacoesModerador",
                column: "IdAvaliadorRecusa");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesModerador_IdPrimeiroAprovador",
                table: "SolicitacoesModerador",
                column: "IdPrimeiroAprovador");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesModerador_IdSegundoAprovador",
                table: "SolicitacoesModerador",
                column: "IdSegundoAprovador");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesModerador_IdUsuarioSolicitante_Status",
                table: "SolicitacoesModerador",
                columns: new[] { "IdUsuarioSolicitante", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacoesModerador");

            migrationBuilder.DropColumn(
                name: "TipoPerfil",
                table: "Usuarios");

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
                    BiografiaGamer = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LimiteListas = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    NivelGamer = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PontosExperiencia = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
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
    }
}
