namespace BlazorShop.Infrastructure.Services
{
    using BlazorShop.Domain.Contracts.Authentication;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1); // Run every hour

        public TokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("?? Token Cleanup Service started. Running every {Interval}.", _cleanupInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Error occurred while cleaning up expired tokens.");
                }

                // Wait for the next cleanup cycle
                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("?? Token Cleanup Service stopped.");
        }

        private async Task CleanupExpiredTokensAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var tokenManager = scope.ServiceProvider.GetRequiredService<IAppTokenManager>();

            _logger.LogInformation("?? Starting expired token cleanup...");

            var removedCount = await tokenManager.RemoveExpiredTokensAsync();

            if (removedCount > 0)
            {
                _logger.LogWarning("???  Removed {Count} expired refresh tokens.", removedCount);
            }
            else
            {
                _logger.LogInformation("? No expired tokens found. Database is clean.");
            }
        }
    }
}
