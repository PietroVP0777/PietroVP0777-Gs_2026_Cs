using TransitoClaro.Api.Domain;

namespace TransitoClaro.Tests;

public class ClassificadorRigorosoTests
{
    private static readonly DateTime Agora = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);

    private static RelatoCidadao Atraso(int min)
        => RelatoCidadao.Criar(1, null, TipoOcorrencia.Atraso, null, min, null, Agora);

    private static RelatoCidadao Falha()
        => RelatoCidadao.Criar(1, null, TipoOcorrencia.FalhaOperacional, null, null, null, Agora);

    [Fact]
    public void MesmoCenario_ClassificadoresDiferentes_DaoResultadosDiferentes()
    {
        var relatos = new[] { Atraso(11) };

        IClassificadorStatus padrao = new ClassificadorPorRelatos();
        IClassificadorStatus rigoroso = new ClassificadorRigoroso();

        Assert.Equal(StatusLinha.Moderado, padrao.Classificar(relatos));
        Assert.Equal(StatusLinha.Critico, rigoroso.Classificar(relatos));
    }

    [Fact]
    public void DuasFalhas_DeveSerCritico()
        => Assert.Equal(StatusLinha.Critico, new ClassificadorRigoroso().Classificar(new[] { Falha(), Falha() }));

    [Fact]
    public void SemRelatos_DeveSerNormal()
        => Assert.Equal(StatusLinha.Normal, new ClassificadorRigoroso().Classificar(new List<RelatoCidadao>()));
}
