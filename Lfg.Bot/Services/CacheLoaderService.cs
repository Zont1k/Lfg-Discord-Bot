using Lfg.Database;

using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

namespace Lfg.Bot.Services;

public class CacheLoaderService : IHostedService
{
    private readonly LfgDbContext _dbContext;
    private readonly StateCacheService _cache;

    public CacheLoaderService(LfgDbContext dbContext, StateCacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Загрузка настроек в кэш
        _ = Task.Run(() =>
                _dbContext.GuildSettings.Find(x => true).
                    ForEachAsync(x =>
                    {
                        _cache.SettingsCache.AddOrUpdate(x.Id, id => x, (id, _) => x);
                    }, cancellationToken)
            , cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {

    }

}