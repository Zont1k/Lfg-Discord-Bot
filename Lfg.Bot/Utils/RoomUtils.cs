using Lfg.Bot.Services;
using Lfg.Database;

namespace Lfg.Bot.Utils;

public class RoomUtils
{
    private readonly StateCacheService _cache;
    private readonly ILogger<RoomUtils> _logger;

    public RoomUtils(StateCacheService cache, ILogger<RoomUtils> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<Room> CreateRoom(SocketGuild guild, IGuildUser creator, GameCategory game, int? memberLimit = null, string? topic = null)
    {
        var settings = await _cache.GetSettingsAsync(guild.Id);

        var vc = await guild.CreateVoiceChannelAsync($"{game.Name} - {creator.DisplayName}", x => x.CategoryId = settings.RoomsCategoryId);

        var room = new Room
        {
            GameCategoryId = game.Id,
            OwnerId = creator.Id,
            IsDeleted = false,
            Id = Guid.NewGuid(),
            MemberLimit = memberLimit ?? settings.DefaultRoomMemberLimit,
            Topic = topic,
            VoiceChannelId = vc.Id
        };

        await _cache.AddRoomAsync(room);
        return room;
    }
}