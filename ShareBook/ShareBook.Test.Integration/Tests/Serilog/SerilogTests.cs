using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace ShareBook.Test.Integration.Tests.Serilog
{
    public class SerilogTests : IClassFixture<ShareBookSerilog>
    {
        private readonly ILogger<SerilogTests> _logger;

        public SerilogTests(ShareBookSerilog factory)
        {
            var scope = factory.Services.CreateScope();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<SerilogTests>>();
        }

        [Fact]
        public void LoggerWithSerilog()
        {
            ShareBookSerilog.InMemorySink.Events.Clear();
            _logger.LogDebug("Log de teste.");

            Assert.Contains(ShareBookSerilog.InMemorySink.Events, log =>
                log.MessageTemplate.Text.Contains("Log de teste"));
        }

        [Fact]
        public void LoggerCatchException()
        {
            ShareBookSerilog.InMemorySink.Events.Clear();

            try
            {
                throw new InvalidOperationException("Erro simulado no teste.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção capturada.");
            }

            Assert.Contains(ShareBookSerilog.InMemorySink.Events, log =>
                log.Exception is InvalidOperationException &&
                log.MessageTemplate.Text.Contains("Exceção capturada."));
        }

        [Fact]
        public void LoggerCatchWarning()
        {
            ShareBookSerilog.InMemorySink.Events.Clear();

            _logger.LogWarning("Este é um aviso.");

            Assert.Contains(ShareBookSerilog.InMemorySink.Events, log =>
                log.Level == LogEventLevel.Warning &&
                log.MessageTemplate.Text.Contains("aviso"));
        }
    }
}