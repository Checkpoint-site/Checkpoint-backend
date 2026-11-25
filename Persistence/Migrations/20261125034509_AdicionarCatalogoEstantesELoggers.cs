using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCatalogoEstantesELoggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Catalogos",
                columns: table => new
                {
                    IdCatalogo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Jogos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogos", x => x.IdCatalogo);
                    table.ForeignKey(
                        name: "FK_Catalogos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EngajamentoLoggers",
                columns: table => new
                {
                    IdLogger = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuantidadeCurtida = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    PresencaPost = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    PresencaComentario = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    PresencaFavoritos = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    UltimaAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngajamentoLoggers", x => x.IdLogger);
                });

            migrationBuilder.CreateTable(
                name: "Jogos",
                columns: table => new
                {
                    IdJogo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SteamAppId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Generos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Imagem = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    Desenvolvedora = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Publicadora = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DataLancamentoTexto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    AnoLancamento = table.Column<int>(type: "int", nullable: true),
                    NotaMetacritic = table.Column<int>(type: "int", nullable: true),
                    TotalRecomendacoes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Gratuito = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmBreve = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SincronizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogos", x => x.IdJogo);
                });

            migrationBuilder.CreateTable(
                name: "PretendeJogar",
                columns: table => new
                {
                    IdCatalogo = table.Column<int>(type: "int", nullable: false),
                    NivelPrioridade = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    NotificarEmPromocao = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MetaAnoConclusao = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PretendeJogar", x => x.IdCatalogo);
                    table.ForeignKey(
                        name: "FK_PretendeJogar_Catalogos_IdCatalogo",
                        column: x => x.IdCatalogo,
                        principalTable: "Catalogos",
                        principalColumn: "IdCatalogo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneroLoggers",
                columns: table => new
                {
                    IdLogger = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: true),
                    Genero = table.Column<int>(type: "int", nullable: false),
                    PresencaPretendeJogar = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneroLoggers", x => x.IdLogger);
                    table.ForeignKey(
                        name: "FK_GeneroLoggers_EngajamentoLoggers_IdLogger",
                        column: x => x.IdLogger,
                        principalTable: "EngajamentoLoggers",
                        principalColumn: "IdLogger",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneroLoggers_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioLoggers",
                columns: table => new
                {
                    IdLogger = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    CurtidasRecebidas = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    QuantidadeSeguidores = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioLoggers", x => x.IdLogger);
                    table.ForeignKey(
                        name: "FK_UsuarioLoggers_EngajamentoLoggers_IdLogger",
                        column: x => x.IdLogger,
                        principalTable: "EngajamentoLoggers",
                        principalColumn: "IdLogger",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioLoggers_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    IdCatalogo = table.Column<int>(type: "int", nullable: false),
                    JogoDestaquePerfilId = table.Column<int>(type: "int", nullable: true),
                    ExibirNoTopoPerfil = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ObservacaoDestaque = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => x.IdCatalogo);
                    table.ForeignKey(
                        name: "FK_Favoritos_Catalogos_IdCatalogo",
                        column: x => x.IdCatalogo,
                        principalTable: "Catalogos",
                        principalColumn: "IdCatalogo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Jogos_JogoDestaquePerfilId",
                        column: x => x.JogoDestaquePerfilId,
                        principalTable: "Jogos",
                        principalColumn: "IdJogo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JogoLoggers",
                columns: table => new
                {
                    IdLogger = table.Column<int>(type: "int", nullable: false),
                    IdJogo = table.Column<int>(type: "int", nullable: false),
                    PresencaPretendeJogar = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    SomaFogueiras = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JogoLoggers", x => x.IdLogger);
                    table.ForeignKey(
                        name: "FK_JogoLoggers_EngajamentoLoggers_IdLogger",
                        column: x => x.IdLogger,
                        principalTable: "EngajamentoLoggers",
                        principalColumn: "IdLogger",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JogoLoggers_Jogos_IdJogo",
                        column: x => x.IdJogo,
                        principalTable: "Jogos",
                        principalColumn: "IdJogo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    IdPost = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdJogo = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Fogueiras = table.Column<int>(type: "int", nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    TipoPost = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantidadeCurtidas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    QuantidadeComentarios = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PostDeletado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.IdPost);
                    table.ForeignKey(
                        name: "FK_Posts_Jogos_IdJogo",
                        column: x => x.IdJogo,
                        principalTable: "Jogos",
                        principalColumn: "IdJogo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Posts_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    IdComentario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPost = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    QuantidadeCurtidas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.IdComentario);
                    table.ForeignKey(
                        name: "FK_Comentarios_Posts_IdPost",
                        column: x => x.IdPost,
                        principalTable: "Posts",
                        principalColumn: "IdPost",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentarios_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Curtidas",
                columns: table => new
                {
                    IdCurtida = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoCurtida = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdComentario = table.Column<int>(type: "int", nullable: true),
                    IdPost = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curtidas", x => x.IdCurtida);
                    table.ForeignKey(
                        name: "FK_Curtidas_Comentarios_IdComentario",
                        column: x => x.IdComentario,
                        principalTable: "Comentarios",
                        principalColumn: "IdComentario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Curtidas_Posts_IdPost",
                        column: x => x.IdPost,
                        principalTable: "Posts",
                        principalColumn: "IdPost",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Curtidas_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Catalogos_IdUsuario",
                table: "Catalogos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_IdPost",
                table: "Comentarios",
                column: "IdPost");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_IdUsuario",
                table: "Comentarios",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_IdComentario",
                table: "Curtidas",
                column: "IdComentario");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_IdPost",
                table: "Curtidas",
                column: "IdPost");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_IdUsuario",
                table: "Curtidas",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_Usuario_Comentario",
                table: "Curtidas",
                columns: new[] { "IdUsuario", "IdComentario" },
                unique: true,
                filter: "[IdComentario] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_Usuario_Post",
                table: "Curtidas",
                columns: new[] { "IdUsuario", "IdPost" },
                unique: true,
                filter: "[IdPost] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_JogoDestaquePerfilId",
                table: "Favoritos",
                column: "JogoDestaquePerfilId");

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

            migrationBuilder.CreateIndex(
                name: "IX_JogoLoggers_IdJogo",
                table: "JogoLoggers",
                column: "IdJogo",
                unique: true,
                filter: "[IdJogo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_AnoLancamento",
                table: "Jogos",
                column: "AnoLancamento");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_Desenvolvedora",
                table: "Jogos",
                column: "Desenvolvedora");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_Nome",
                table: "Jogos",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_Jogos_SteamAppId",
                table: "Jogos",
                column: "SteamAppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_IdJogo",
                table: "Posts",
                column: "IdJogo");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_IdUsuario_PostDeletado",
                table: "Posts",
                columns: new[] { "IdUsuario", "PostDeletado" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioLoggers_IdUsuario",
                table: "UsuarioLoggers",
                column: "IdUsuario",
                unique: true,
                filter: "[IdUsuario] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Curtidas");

            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "GeneroLoggers");

            migrationBuilder.DropTable(
                name: "JogoLoggers");

            migrationBuilder.DropTable(
                name: "PretendeJogar");

            migrationBuilder.DropTable(
                name: "UsuarioLoggers");

            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "Catalogos");

            migrationBuilder.DropTable(
                name: "EngajamentoLoggers");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Jogos");
        }
    }
}
