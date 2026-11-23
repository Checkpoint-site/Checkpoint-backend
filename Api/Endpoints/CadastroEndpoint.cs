using Application.Dtos.Cadastro.Requests;
using Application.Dtos.Cadastro.Responses;
using Application.Interfaces;

namespace Api.Endpoints.Cadastro;

public static class CadastroEndpoint
{
    public static void MapCadastroEndpoint(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/cadastro")
            .WithTags("Autenticação");

        group.MapPost("/verificar-disponibilidade", async (
            VerificarEmailRequestDto request,
            ICadastroService service) =>
        {
            var resposta = await service.VerificarDisponibilidadeAsync(request);
            return TypedResults.Ok(resposta);
        })
        .WithName("VerificarDisponibilidadeEmail")
        .WithSummary("Verifica a disponibilidade de um e-mail")
        .WithDescription("Retorna se o e-mail informado já está em uso ou se está livre para cadastro.")
        .Produces<VerificarEmailResponseDto>(200)
        .ProducesProblem(400);

        group.MapPost("/solicitar-codigo", async (
            SolicitarCodigoRequestDto request,
            ICadastroService service) =>
        {
            var resposta = await service.SolicitarCodigoAsync(request);
            return TypedResults.Ok(resposta);
        })
        .WithName("SolicitarCodigoCadastro")
        .WithSummary("Solicita envio do código de verificação")
        .WithDescription("Gera e envia um código numérico de 6 dígitos para o e-mail do jogador.")
        .Produces<SolicitarCodigoResponseDto>(200)
        .ProducesProblem(400)
        .ProducesProblem(409);

        group.MapPost("", async (
            CadastroRequestDto request,
            ICadastroService service) =>
        {
            var resposta = await service.CadastrarAsync(request);
            return TypedResults.Created(string.Empty, resposta);
        })
        .WithName("Cadastro")
        .WithSummary("Cadastra um novo usuário no sistema")
        .WithDescription("Valida o código de verificação recebido e registra um novo jogador com nome, e-mail e senha.")
        .Produces<CadastroResponseDto>(201)
        .ProducesProblem(400)
        .ProducesProblem(409);
    }
}
