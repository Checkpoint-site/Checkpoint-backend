using Application.Dtos.Cadastro.Requests;
using Application.Strategies.Perfis;
using Domain.Dtos.Perfis.Respost;
using Domain.Enum;
using Domain.Exceptions.Common;

namespace Test.Application.Strategies;

public class RegraPerfilUsuarioTests
{
    [Fact]
    public void RegraPerfilRegular_PropriedadesBase_RetornaValoresCorretos()
    {
        var regra = new RegraPerfilRegular();

        Assert.Equal(TipoPerfilUsuario.Regular, regra.TipoPerfil);
        Assert.Equal("regular", regra.ChaveExibicao);
        Assert.Equal("Regular", regra.RoleJwt);
        Assert.Equal(15, regra.LimiteInicialListas);
        Assert.False(regra.PodeModerarConteudo);
    }

    [Fact]
    public void RegraPerfilRegular_CriarComandoAdicionarExperiencia_CalculaNovoNivelELimiteListas()
    {
        var regra = new RegraPerfilRegular();
        var usuario = new PerfilRegularRespostDto
        {
            Id = 10,
            Nome = "Gamer",
            Email = "gamer@checkpoint.com"
        };

        var comando = regra.CriarComandoAdicionarExperiencia(usuario, 100);

        Assert.Equal(10, comando.IdUsuario);
        Assert.Equal(2, comando.NivelGamer);
        Assert.Equal(20, comando.LimiteListas);
        Assert.Equal(0, comando.PontosExperiencia);
    }

    [Fact]
    public void RegraPerfilRegular_CriarComandoAdicionarExperienciaNegativa_LancaValidationException()
    {
        var regra = new RegraPerfilRegular();
        var usuario = new PerfilRegularRespostDto
        {
            Id = 10,
            Nome = "Gamer",
            Email = "gamer@checkpoint.com"
        };

        var ex = Assert.Throws<ValidationException>(() => regra.CriarComandoAdicionarExperiencia(usuario, -10));
        Assert.Equal("A pontuação de experiência deve ser positiva.", ex.Message);
    }

    [Fact]
    public void RegraPerfilModerador_PropriedadesBase_RetornaValoresCorretos()
    {
        var regra = new RegraPerfilModerador();

        Assert.Equal(TipoPerfilUsuario.Moderador, regra.TipoPerfil);
        Assert.Equal("moderador", regra.ChaveExibicao);
        Assert.Equal("Moderador", regra.RoleJwt);
        Assert.Equal(100, regra.LimiteInicialListas);
        Assert.True(regra.PodeModerarConteudo);
    }

    [Fact]
    public void RegraPerfilModerador_ValidarDadosCadastro_SemCredencialOuSetor_LancaValidationException()
    {
        var regra = new RegraPerfilModerador();

        var semCredencial = new CadastroRequestDto
        {
            Nome = "Mod",
            Email = "mod@checkpoint.com",
            Senha = "senha",
            Codigo = "123456",
            TipoPerfil = "Moderador",
            CodigoCredencial = null,
            SetorModeracao = "Reviews"
        };

        var exCredencial = Assert.Throws<ValidationException>(() => regra.ValidarDadosCadastro(semCredencial));
        Assert.Equal("O código de credencial é obrigatório para moderadores.", exCredencial.Message);

        var semSetor = semCredencial with { CodigoCredencial = "MOD-01", SetorModeracao = " " };
        var exSetor = Assert.Throws<ValidationException>(() => regra.ValidarDadosCadastro(semSetor));
        Assert.Equal("O setor de moderação é obrigatório para moderadores.", exSetor.Message);
    }

    [Fact]
    public void RegraPerfilModerador_CriarComandoSuspensao_GeraComandoValidoEBloqueiaCriacaoDeLista()
    {
        var regra = new RegraPerfilModerador();
        var moderador = new PerfilModeradorRespostDto
        {
            Id = 1,
            Nome = "Staff",
            Email = "staff@checkpoint.com",
            CodigoCredencial = "MOD-01",
            SetorModeracao = "Comunidade",
            TotalAcoesModeracao = 4
        };

        var alvo = new PerfilRegularRespostDto
        {
            Id = 2,
            Nome = "Infrator",
            Email = "infrator@checkpoint.com"
        };

        var comando = regra.CriarComandoSuspensao(moderador, alvo, 7, "Violação de regras");
        var alvoSuspenso = alvo with { SuspensoAte = comando.SuspensoAte };

        Assert.Equal(1, comando.IdModerador);
        Assert.Equal(2, comando.IdUsuarioAlvo);
        Assert.True(regra.EstaSuspenso(alvoSuspenso));
        Assert.False(regra.PodeCriarLista(alvoSuspenso));
        Assert.Equal(5, comando.NovoTotalAcoesModeracao);
    }

    [Fact]
    public void RegraPerfilModerador_SuspenderPropriaConta_LancaValidationException()
    {
        var regra = new RegraPerfilModerador();
        var moderador = new PerfilModeradorRespostDto
        {
            Id = 1,
            Nome = "Staff",
            Email = "staff@checkpoint.com",
            CodigoCredencial = "MOD-01",
            SetorModeracao = "Comunidade"
        };

        var ex = Assert.Throws<ValidationException>(() =>
            regra.CriarComandoSuspensao(moderador, moderador, 5, "Auto punição"));

        Assert.Equal("Um moderador não pode suspender a própria conta.", ex.Message);
    }

    [Fact]
    public void RegraPerfilModerador_CriarComandoReativacao_RemoveSuspensaoEIncrementaAcoes()
    {
        var regra = new RegraPerfilModerador();
        var moderador = new PerfilModeradorRespostDto
        {
            Id = 1,
            Nome = "Staff",
            Email = "staff@checkpoint.com",
            CodigoCredencial = "MOD-01",
            SetorModeracao = "Comunidade",
            TotalAcoesModeracao = 2
        };

        var alvo = new PerfilRegularRespostDto
        {
            Id = 2,
            Nome = "Infrator",
            Email = "infrator@checkpoint.com",
            SuspensoAte = DateTime.UtcNow.AddDays(5)
        };

        var comando = regra.CriarComandoReativacao(moderador, alvo);
        var alvoReativado = alvo with { SuspensoAte = comando.SuspensoAte };

        Assert.Null(comando.SuspensoAte);
        Assert.False(regra.EstaSuspenso(alvoReativado));
        Assert.True(regra.PodeCriarLista(alvoReativado));
        Assert.Equal(3, comando.NovoTotalAcoesModeracao);
    }
}
