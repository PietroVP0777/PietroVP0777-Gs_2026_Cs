namespace TransitoClaro.Api.Domain;

/// <summary>Linha ou corredor de transporte. Criação e alteração passam pelas regras do domínio.</summary>
public class Linha : EntidadeBase
{
    public const int MaxNome = 80;
    public const int MinTexto = 2;
    public const int MaxCor = 30;

    public string Nome { get; private set; } = string.Empty;
    public TipoTransporte Tipo { get; private set; }
    public string Operadora { get; private set; } = string.Empty;
    public string? Cor { get; private set; }

    public ICollection<Estacao> Estacoes { get; private set; } = new List<Estacao>();
    public ICollection<RelatoCidadao> Relatos { get; private set; } = new List<RelatoCidadao>();

    private Linha() { } // exigido pelo EF Core

    public static Linha Criar(string nome, TipoTransporte tipo, string operadora, string? cor)
    {
        var linha = new Linha();
        linha.Atualizar(nome, tipo, operadora, cor);
        return linha;
    }

    public void Atualizar(string nome, TipoTransporte tipo, string operadora, string? cor)
    {
        nome = nome?.Trim() ?? string.Empty;
        operadora = operadora?.Trim() ?? string.Empty;
        cor = string.IsNullOrWhiteSpace(cor) ? null : cor.Trim();

        if (nome.Length < MinTexto || nome.Length > MaxNome)
            throw new DomainException($"O nome da linha deve ter entre {MinTexto} e {MaxNome} caracteres.");
        if (operadora.Length < MinTexto || operadora.Length > MaxNome)
            throw new DomainException($"A operadora deve ter entre {MinTexto} e {MaxNome} caracteres.");
        if (!Enum.IsDefined(tipo))
            throw new DomainException("Tipo de transporte inválido.");
        if (cor is { Length: > MaxCor })
            throw new DomainException($"A cor deve ter no máximo {MaxCor} caracteres.");

        Nome = nome;
        Tipo = tipo;
        Operadora = operadora;
        Cor = cor;
    }
}
