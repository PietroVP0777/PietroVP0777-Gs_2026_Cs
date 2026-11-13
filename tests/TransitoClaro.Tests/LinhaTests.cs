using TransitoClaro.Api.Domain;

namespace TransitoClaro.Tests;

public class LinhaTests
{
    [Fact]
    public void Criar_Valida_DeveAparar_EspacosEPreencherCampos()
    {
        var linha = Linha.Criar("  Linha 4-Amarela ", TipoTransporte.Metro, " ViaQuatro ", "  ");

        Assert.Equal("Linha 4-Amarela", linha.Nome);
        Assert.Equal("ViaQuatro", linha.Operadora);
        Assert.Null(linha.Cor);
    }

    [Theory]
    [InlineData("", "Operadora")]
    [InlineData("A", "Operadora")]
    [InlineData("Linha X", "")]
    public void Criar_ComDadosInvalidos_DeveLancarDomainException(string nome, string operadora)
        => Assert.Throws<DomainException>(() => Linha.Criar(nome, TipoTransporte.Trem, operadora, null));

    [Fact]
    public void Criar_ComTipoInvalido_DeveLancarDomainException()
        => Assert.Throws<DomainException>(() => Linha.Criar("Linha X", (TipoTransporte)99, "Operadora", null));

    [Fact]
    public void Atualizar_DeveAlterarOsDados()
    {
        var linha = Linha.Criar("Linha A", TipoTransporte.Brt, "SPTrans", null);

        linha.Atualizar("Linha B", TipoTransporte.Trem, "CPTM", "Rubi");

        Assert.Equal("Linha B", linha.Nome);
        Assert.Equal(TipoTransporte.Trem, linha.Tipo);
        Assert.Equal("Rubi", linha.Cor);
    }
}
