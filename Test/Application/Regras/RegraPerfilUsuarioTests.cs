using Application.Dtos.Cadastro.Requests;
using Application.Regras.Perfis;
using Domain.Exceptions.Common;
using Domain.Model;
using Xunit;

namespace Test.Application.Regras;

public class RegraPerfilUsuarioTests
{
    [Fact]
    public void RegraPerfilRegular_PropriedadesBase_RetornaValoresCorretos()
    {
        var regra = new RegraPerfilRegular();

        Assert.Equal("Regular", regra.ObterRole());
        Assert.Equal(15, regra.ObterLimiteInicialListas());
        Assert.False(regra.PodeModerarConteudo());
    }

    [Fact]
    public void RegraPerfilRegular_AdicionarExperiencia_AumentaNivelELimiteListas()
    {
        var regra = new RegraPerfilRegular();
        var usuario = new UsuarioRegular
        {
            Nome = "Gamer",
            Email = "gamer@checkpoint.com",
            Senha = "hash"
        };

        regra.AdicionarExperiencia(usuario, 100);

        Assert.Equal(2, usuario.NivelGamer);
        Assert.Equal(20, usuario.LimiteListas);
        Assert.Equal(0, usuario.PontosExperiencia);
    }

    [Fact]
    public void RegraPerfilRegular_AdicionarExperienciaNegativa_LancaValidationException()
    {
        var regra = new RegraPerfilRegular();
        var usuario = new UsuarioRegular
        {
            Nome = "Gamer",
            Email = "gamer@checkpoint.com",
            Senha = "hash"
        };

        var ex = Assert.Throws<ValidationException>(() => regra.AdicionarExperiencia(usuario, -10));
        Assert.Equal("A pontuação de experiência deve ser positiva.", ex.Message);
    }

    [Fact]
    public void RegraPerfilModerador_PropriedadesBase_RetornaValoresCorretos()
    {
        var regra = new RegraPerfilModerador();

        Assert.Equal("Moderador", regra.ObterRole());
        Assert.Equal(100, regra.ObterLimiteInicialListas());
        Assert.True(regra.PodeModerarConteudo());
    }

    [Fact]
    public void RegraPerfilModerador_ValidarDadosCadastro_SemCredencial_LancaValidationException()
    {
        var regra = new RegraPerfilModerador();
        var request = new CadastroRequestDto
        {
            Nome = "Mod",
            Email = "mod@checkpoint.com",
            Senha = "senha",
            Codigo = "123456",
            TipoPerfil = "Moderador",
            CodigoCredencial = null,
            SetorModeracao = "Reviews"
        };

        var ex = Assert.Throws<ValidationException>(() => regra.ValidarDadosCadastro(request));
        Assert.Equal("O código de credencial é obrigatório para moderadores.", ex.Message);
    }

    [Fact]
    public void RegraPerfilModerador_SuspenderUsuario_AplicaSuspensaoEIncrementaAcoes()
    {
        var regra = new RegraPerfilModerador();
        var moderador = new Moderador
        {
            Id = 1,
            Nome = "Staff",
            Email = "staff@checkpoint.com",
            Senha = "hash",
            CodigoCredencial = "MOD-01",
            SetorModeracao = "Comunidade"
        };

        var alvo = new UsuarioRegular
        {
            Id = 2,
            Nome = "Infrator",
            Email = "infrator@checkpoint.com",
            Senha = "hash"
        };

        regra.SuspenderUsuario(moderador, alvo, 7, "Violação de regras");

        Assert.True(regra.EstaSuspenso(alvo));
        Assert.False(regra.PodeCriarLista(alvo));
        Assert.Equal(1, moderador.TotalAcoesModeracao);
    }

    [Fact]
    public void RegraPerfilModerador_SuspenderPropriaConta_LancaValidationException()
    {
        var regra = new RegraPerfilModerador();
        var moderador = new Moderador
        {
            Id = 1,
            Nome = "Staff",
            Email = "staff@checkpoint.com",
            Senha = "hash",
            CodigoCredencial = "MOD-01",
            SetorModeracao = "Comunidade"
        };

        var ex = Assert.Throws<ValidationException>(() =>
            regra.SuspenderUsuario(moderador, moderador, 5, "Auto punição"));

        Assert.Equal("Um moderador não pode suspender a própria conta.", ex.Message);
    }

    [Fact]
    public void RegraPerfilModerador_ReativarUsuario_RemoveSuspensao()
    {
        var regra = new RegraPerfilModerador();
        var moderador = new Moderador
        {
            Id = 1,
            Nome = "Staff",
            Email = "staff@checkpoint.com",
            Senha = "hash",
            CodigoCredencial = "MOD-01",
            SetorModeracao = "Comunidade"
        };

        var alvo = new UsuarioRegular
        {
            Id = 2,
            Nome = "Infrator",
            Email = "infrator@checkpoint.com",
            Senha = "hash",
            SuspensoAte = DateTime.UtcNow.AddDays(5)
        };

        regra.ReativarUsuario(moderador, alvo);

        Assert.False(regra.EstaSuspenso(alvo));
        Assert.True(regra.PodeCriarLista(alvo));
        Assert.Equal(1, moderador.TotalAcoesModeracao);
    }

    [Fact]
    public void Factory_Criar_RetornaInstanciaPolimorficaCorreta()
    {
        var regraRegular = RegraPerfilUsuario.Criar("Regular");
        var regraModerador = RegraPerfilUsuario.Criar("Moderador");
        var regraPadrao = RegraPerfilUsuario.Criar(null);

        Assert.IsType<RegraPerfilRegular>(regraRegular);
        Assert.IsType<RegraPerfilModerador>(regraModerador);
        Assert.IsType<RegraPerfilRegular>(regraPadrao);
    }
}
