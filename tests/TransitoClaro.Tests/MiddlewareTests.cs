using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TransitoClaro.Api.Domain;
using TransitoClaro.Api.Infra;

namespace TransitoClaro.Tests;

public class MiddlewareTests
{
    private static async Task<int> Executar(Exception? excecao)
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();
        var middleware = new MiddlewareDeExcecoes(
            _ => excecao is null ? Task.CompletedTask : throw excecao,
            NullLogger<MiddlewareDeExcecoes>.Instance);

        await middleware.InvokeAsync(contexto);
        return contexto.Response.StatusCode;
    }

    [Fact]
    public async Task SemErro_DeveManterStatus200() => Assert.Equal(200, await Executar(null));

    [Fact]
    public async Task NaoEncontrado_DeveRetornar404() => Assert.Equal(404, await Executar(new NaoEncontradoException("x")));

    [Fact]
    public async Task DomainException_DeveRetornar400() => Assert.Equal(400, await Executar(new DomainException("x")));

    [Fact]
    public async Task DbUpdateException_DeveRetornar409() => Assert.Equal(409, await Executar(new DbUpdateException("x")));

    [Fact]
    public async Task ErroInesperado_DeveRetornar500() => Assert.Equal(500, await Executar(new InvalidOperationException("x")));
}
