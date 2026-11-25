using Application.Dtos.Cadastro.Requests;
using Application.Dtos.Cadastro.Responses;
using Application.Interfaces;
using Application.Regras.Perfis;
using Application.Utils;
using Application.Utils.Hash;
using Application.Utils.Token;
using Domain.Exceptions.Common;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using static Application.Mappers.CadastroMapper;

namespace Application.Services;

public class CadastroService(
    ICadastroRepository repository,
    ICodigoVerificacaoService codigoVerificacaoService,
    IEmailService emailService,
    IModeracaoFotoService moderacaoFotoService
) : ICadastroService
{
    public async Task<VerificarEmailResponseDto> VerificarDisponibilidadeAsync(VerificarEmailRequestDto request)
    {
        var emailExiste = await repository.EmailExisteAsync(request.Email);

        return new VerificarEmailResponseDto
        {
            Disponivel = !emailExiste
        };
    }

    public async Task<SolicitarCodigoResponseDto> SolicitarCodigoAsync(SolicitarCodigoRequestDto request)
    {
        var emailExiste = await repository.EmailExisteAsync(request.Email);
        if (emailExiste)
        {
            throw new ConflictException("Este e-mail já está em uso.");
        }

        var codigo = CodigoVerificacaoUtil.GerarCodigo();

        await codigoVerificacaoService.SalvarCodigoAsync(request.Email, codigo, TimeSpan.FromMinutes(10));

        await emailService.EnviarEmailAsync(
            request.Email,
            "Seu código de verificação Checkpoint",
            $"Seu código de verificação para criação de conta é: {codigo}. Ele expira em 10 minutos."
        );

        return new SolicitarCodigoResponseDto
        {
            Mensagem = "Código de verificação enviado com sucesso."
        };
    }

    public async Task<CadastroResponseDto> CadastrarAsync(CadastroRequestDto request)
    {
        var codigoSalvo = await codigoVerificacaoService.ObterCodigoAsync(request.Email);

        if (string.IsNullOrWhiteSpace(codigoSalvo) || !codigoSalvo.Equals(request.Codigo, StringComparison.Ordinal))
        {
            throw new ValidationException("Código de verificação inválido ou expirado.");
        }

        if (!string.IsNullOrWhiteSpace(request.FotoPerfil))
        {
            bool fotoSegura = await moderacaoFotoService.FotoEhSeguraAsync(request.FotoPerfil);
            if (!fotoSegura)
            {
                throw new ValidationException("A foto de perfil contém conteúdo impróprio ou ofensivo.");
            }
        }

        var regra = RegraPerfilUsuario.Criar(request.TipoPerfil);
        regra.ValidarDadosCadastro(request);

        await codigoVerificacaoService.RemoverCodigoAsync(request.Email);

        var command = ToCommand(request);
        var senhaHash = Hash.GerarHashSenha(command.Senha);

        var commandComHash = command with
        {
            Email = command.Email,
            Senha = senhaHash,
            LimiteListasInicial = regra.ObterLimiteInicialListas()
        };

        var usuarioCriado = await repository.CadastroRepositoryAsync(commandComHash)
            ?? throw new ConflictException("Este e-mail já está em uso.");

        string token = Token.GerarToken(
            usuarioCriado.Id,
            usuarioCriado.Email,
            regra.ObterRole()
        );

        return new CadastroResponseDto
        {
            Token = token
        };
    }
}
