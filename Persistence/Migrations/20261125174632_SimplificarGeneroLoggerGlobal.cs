using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplificarGeneroLoggerGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM [EngajamentoLoggers] WHERE [IdLogger] IN (SELECT [IdLogger] FROM [GeneroLoggers] WHERE [IdUsuario] IS NOT NULL);");

            migrationBuilder.DropForeignKey(
                name: "FK_GeneroLoggers_Usuarios_IdUsuario",
                table: "GeneroLoggers");

            migrationBuilder.DropIndex(
                name: "IX_GeneroLoggers_Genero_Global",
                table: "GeneroLoggers");

            migrationBuilder.DropIndex(
                name: "IX_GeneroLoggers_Usuario_Genero",
                table: "GeneroLoggers");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "GeneroLoggers");

            migrationBuilder.CreateIndex(
                name: "IX_GeneroLoggers_Genero",
                table: "GeneroLoggers",
                column: "Genero",
                unique: true,
                filter: "[Genero] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeneroLoggers_Genero",
                table: "GeneroLoggers");

            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "GeneroLoggers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneroLoggers_Genero_Global",
                table: "GeneroLoggers",
                column: "Genero",
                unique: true,
                filter: "[IdUsuario] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GeneroLoggers_Usuario_Genero",
                table: "GeneroLoggers",
                columns: new[] { "IdUsuario", "Genero" },
                unique: true,
                filter: "[IdUsuario] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneroLoggers_Usuarios_IdUsuario",
                table: "GeneroLoggers",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
