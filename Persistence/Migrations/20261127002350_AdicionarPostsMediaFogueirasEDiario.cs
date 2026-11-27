using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPostsMediaFogueirasEDiario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MediaFogueiras",
                table: "UsuarioLoggers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SomaFogueirasDadas",
                table: "UsuarioLoggers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "TotalAvaliacoes",
                table: "UsuarioLoggers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<double>(
                name: "Fogueiras",
                table: "Posts",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusJornada",
                table: "Posts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValueSql: "'[]'");

            migrationBuilder.AddColumn<double>(
                name: "MediaFogueiras",
                table: "Jogos",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalAvaliacoes",
                table: "Jogos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "SomaFogueiras",
                table: "JogoLoggers",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L);

            migrationBuilder.AddColumn<double>(
                name: "MediaFogueiras",
                table: "JogoLoggers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "TotalAvaliacoes",
                table: "JogoLoggers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "ComentarioDeletado",
                table: "Comentarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TemSpoiler",
                table: "Comentarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RegistrosDiario",
                columns: table => new
                {
                    IdRegistroDiario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdJogo = table.Column<int>(type: "int", nullable: false),
                    Jogado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NotaFogueiras = table.Column<double>(type: "float", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "'[]'"),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosDiario", x => x.IdRegistroDiario);
                    table.ForeignKey(
                        name: "FK_RegistrosDiario_Jogos_IdJogo",
                        column: x => x.IdJogo,
                        principalTable: "Jogos",
                        principalColumn: "IdJogo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrosDiario_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDiario_IdJogo",
                table: "RegistrosDiario",
                column: "IdJogo");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDiario_IdUsuario_IdJogo",
                table: "RegistrosDiario",
                columns: new[] { "IdUsuario", "IdJogo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosDiario");

            migrationBuilder.DropColumn(
                name: "MediaFogueiras",
                table: "UsuarioLoggers");

            migrationBuilder.DropColumn(
                name: "SomaFogueirasDadas",
                table: "UsuarioLoggers");

            migrationBuilder.DropColumn(
                name: "TotalAvaliacoes",
                table: "UsuarioLoggers");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "StatusJornada",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "MediaFogueiras",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "TotalAvaliacoes",
                table: "Jogos");

            migrationBuilder.DropColumn(
                name: "MediaFogueiras",
                table: "JogoLoggers");

            migrationBuilder.DropColumn(
                name: "TotalAvaliacoes",
                table: "JogoLoggers");

            migrationBuilder.DropColumn(
                name: "ComentarioDeletado",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "TemSpoiler",
                table: "Comentarios");

            migrationBuilder.AlterColumn<int>(
                name: "Fogueiras",
                table: "Posts",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<long>(
                name: "SomaFogueiras",
                table: "JogoLoggers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);
        }
    }
}
