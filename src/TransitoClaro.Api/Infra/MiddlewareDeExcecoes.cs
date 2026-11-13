using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitoClaro.Api.Domain;

namespace TransitoClaro.Api.Infra;

/// <summary>Converte exceções em respostas HTTP padronizadas (ProblemDetails).</summary>
public class MiddlewareDeExcecoes
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MiddlewareDeExcecoes> _logger;

    public MiddlewareDeExcecoes(RequestDelegate next, ILogger<MiddlewareDeExcecoes> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _next(contexto);
        }
        catch (NaoEncontradoException ex)
        {
            await Responder(contexto, StatusCodes.Status404NotFound, "Recurso não encontrado", ex.Message);
        }
        catch (DomainException ex)
        {
            await Responder(contexto, StatusCodes.Status400BadRequest, "Requisição inválida", ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Falha ao gravar no banco de dados.");
            await Responder(contexto, StatusCodes.Status409Conflict, "Conflito ao salvar",
                "Não foi possível salvar os dados. Verifique se o registro já existe ou se há dependências.");
        }
        catch (OperationCanceledException) when (contexto.RequestAborted.IsCancellationRequested)
        {
            // Cliente cancelou a requisição: nada a responder.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar {Metodo} {Caminho}.",
                contexto.Request.Method, contexto.Request.Path);
            await Responder(contexto, StatusCodes.Status500InternalServerError, "Erro interno",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }
    }

    private static Task Responder(HttpContext contexto, int status, string titulo, string detalhe)
    {
        if (contexto.Response.HasStarted)
            return Task.CompletedTask;

        contexto.Response.StatusCode = status;
        return contexto.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalhe
        });
    }
}
