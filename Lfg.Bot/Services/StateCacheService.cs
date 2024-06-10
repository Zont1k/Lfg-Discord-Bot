using System.Collections.Concurrent;
using Lfg.Database;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Lfg.Bot.Services;

public class StateCacheService
{
    private readonly LfgDbContext _dbContext;
    private readonly DiscordSocketClient _client;

    public ConcurrentDictionary<ulong, GuildSettings> SettingsCache;
    public ConcurrentDictionary<Guid, Room> RoomsCache;

    public StateCacheService(LfgDbContext dbContext, DiscordSocketClient client)
    {
        _dbContext = dbContext;
        _client = client;
        SettingsCache = new ();
        RoomsCache = new ();
    }

    public async ValueTask<GuildSettings> GetSettingsAsync(ulong guildId)
    {
        if (SettingsCache.TryGetValue(guildId, out var guildSettings))
            return guildSettings;

        var search = await _dbContext.GuildSettings
            .Find(Builders<GuildSettings>.Filter.Eq(x => x.Id, guildId))
            .FirstOrDefaultAsync();

        if (search is not null)
        {
            SettingsCache.TryAdd(guildId, search);
            return search;
        }

        return await CreateSettingsAsync(guildId);
    }

    public async Task<GuildSettings> CreateSettingsAsync(ulong guildId)
    {
        var settings = new GuildSettings
        {
            Id = guildId,
            RoomLimit = LfgConfig.RoomLimit,
            DefaultRoomMemberLimit = LfgConfig.DefaultMemberLimit,
            GameLimit = LfgConfig.FreeGameLimit,
            RoomCreateChannelLimit = LfgConfig.FreeRoomCreateChannelLimit,
            LfgEnabled = false,
            PremiumLevel = PremiumLevel.Free,
        };

        await _dbContext.GuildSettings.InsertOneAsync(settings);
        SettingsCache.TryAdd(guildId, settings);

        return settings;
    }

    public async Task<GuildSettings> UpdateSettingsAsync(GuildSettings settings, Action<GuildSettings> func)
    {
        func(settings);

        await _dbContext.GuildSettings.ReplaceOneAsync(x => x.Id == settings.Id, settings);
        return settings;
    }

    public async Task<Room?> GetRoomAsync(Guid id)
    {
        if (RoomsCache.TryGetValue(id, out var room))
            return room;

        var search = await _dbContext.Rooms
            .Find(Builders<Room>.Filter.Eq(x => x.Id, id))
            .FirstOrDefaultAsync();

        if (search is not null)
        {
            RoomsCache.TryAdd(id, search);
            return search;
        }

        return null;
    }

    public Task AddRoomAsync(Room room)
    {
        RoomsCache.TryAdd(room.Id, room);
        return _dbContext.Rooms.InsertOneAsync(room);
    }

    public async Task<Room> UpdateRoomAsync(Room room, Action<Room> func)
    {
        func(room);
        await _dbContext.Rooms.ReplaceOneAsync(x => x.Id == room.Id, room);
        return room;
    }
}