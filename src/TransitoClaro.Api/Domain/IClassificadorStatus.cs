namespace TransitoClaro.Api.Domain;

/// <summary>Estratégia que transforma relatos recentes em um status simples para o cidadão.</summary>
public interface IClassificadorStatus
{
    StatusLinha Classificar(IReadOnlyCollection<RelatoCidadao> relatosRecentes);
}

/// <summary>
/// Base abstrata (Template Method): o algoritmo de classificação é único e
/// cada estratégia concreta só define seus limiares.
/// </summary>
public abstract class ClassificadorBase : IClassificadorStatus
{
    protected abstract double AtrasoCritico { get; }
    protected abstract double AtrasoModerado { get; }
    protected abstract double ProporcaoCriticaLotado { get; }
    protected abstract double ProporcaoModeradaCheio { get; }
    protected abstract int FalhasCriticas { get; }

    public StatusLinha Classificar(IReadOnlyCollection<RelatoCidadao> relatosRecentes)
    {
        if (relatosRecentes.Count == 0)
            return StatusLinha.Normal;

        var atrasos = relatosRecentes
            .Where(r => r.Tipo == TipoOcorrencia.Atraso && r.MinutosAtraso.HasValue)
            .Select(r => r.MinutosAtraso!.Value)
            .ToList();
        double atrasoMedio = atrasos.Count > 0 ? atrasos.Average() : 0;

        var comLotacao = relatosRecentes.Where(r => r.NivelLotacao.HasValue).ToList();
        double propLotado = comLotacao.Count == 0 ? 0
            : comLotacao.Count(r => r.NivelLotacao == NivelLotacao.Lotado) / (double)comLotacao.Count;
        double propCheioOuMais = comLotacao.Count == 0 ? 0
            : comLotacao.Count(r => r.NivelLotacao >= NivelLotacao.Cheio) / (double)comLotacao.Count;

        int falhas = relatosRecentes.Count(r => r.Tipo == TipoOcorrencia.FalhaOperacional);

        if (atrasoMedio >= AtrasoCritico || propLotado >= ProporcaoCriticaLotado || falhas >= FalhasCriticas)
            return StatusLinha.Critico;

        if (atrasoMedio >= AtrasoModerado || propCheioOuMais >= ProporcaoModeradaCheio || falhas >= 1)
            return StatusLinha.Moderado;

        return StatusLinha.Normal;
    }
}

/// <summary>Estratégia padrão: limiares equilibrados.</summary>
public class ClassificadorPorRelatos : ClassificadorBase
{
    public const double AtrasoCriticoMin = 15;
    public const double AtrasoModeradoMin = 5;
    public const double ProporcaoLimite = 0.5;
    public const int FalhasCritico = 3;

    protected override double AtrasoCritico => AtrasoCriticoMin;
    protected override double AtrasoModerado => AtrasoModeradoMin;
    protected override double ProporcaoCriticaLotado => ProporcaoLimite;
    protected override double ProporcaoModeradaCheio => ProporcaoLimite;
    protected override int FalhasCriticas => FalhasCritico;
}

/// <summary>Estratégia rigorosa: alerta mais cedo (útil em horários de pico).</summary>
public class ClassificadorRigoroso : ClassificadorBase
{
    protected override double AtrasoCritico => 10;
    protected override double AtrasoModerado => 3;
    protected override double ProporcaoCriticaLotado => 0.4;
    protected override double ProporcaoModeradaCheio => 0.3;
    protected override int FalhasCriticas => 2;
}
