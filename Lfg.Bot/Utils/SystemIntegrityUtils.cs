using Lfg.Database;
using Lfg.Bot.Services;
using Lfg.Bot.Messages;

namespace Lfg.Bot.Utils;

public class SystemIntegrityUtils
{
    private readonly DiscordSocketClient _client;
    private readonly StateCacheService _stateCacheService;
    private readonly ILogger<SystemIntegrityUtils> _logger;

    public SystemIntegrityUtils(DiscordSocketClient client, StateCacheService stateCacheService, ILogger<SystemIntegrityUtils> logger)
    {
        _client = client;
        _stateCacheService = stateCacheService;
        _logger = logger;
    }

    /// <returns>
    ///     <see langword="false" /> if guild is ok, <see langword="true" /> if guild is broken.
    /// </returns>
    public async Task<bool> CheckGuildAsync(ulong guildId)
    {
        var guild = _client.GetGuild(guildId);

        if (guild is null)
        {
            _logger.LogWarning("Guild {GuildId} not found", guildId);
            return false;
        }

        var settings = await _stateCacheService.GetSettingsAsync(guildId);

        if (settings?.LfgEnabled is false)
            return false;

        ICategoryChannel? roomsCategory = guild.GetChannel(settings.RoomsCategoryId ?? 0) as SocketCategoryChannel;
        ICategoryChannel? lfgCategory = guild.GetChannel(settings.LfgCategoryId ?? 0) as SocketCategoryChannel;
        ITextChannel? controlChannel = guild.GetChannel(settings.ControlChannelId ?? 0) as SocketTextChannel;
        IVoiceChannel? createVoiceChannel = guild.GetChannel(settings.CreateVoiceChannelId ?? 0) as SocketVoiceChannel;
        ITextChannel? requestChannel = guild.GetChannel(settings.RequestChannelId ?? 0) as SocketTextChannel;
        ITextChannel? searchChannel = guild.GetChannel(settings.SearchChannelId ?? 0) as SocketTextChannel;

        var isBroken = roomsCategory is null || lfgCategory is null || controlChannel is null || createVoiceChannel is null || requestChannel is null || searchChannel is null
                       || settings.RoomCreateChannels.Any(x => guild.GetVoiceChannel(x.ChannelId ?? 0) is null);

        return isBroken;
    }


    public async Task InitializeGuildAsync(ulong guildId)
    {
        var guild = _client.GetGuild(guildId);

        if (guild is null)
        {
            _logger.LogWarning("Guild {GuildId} not found", guildId);
            return;
        }

        var settings = await _stateCacheService.GetSettingsAsync(guildId);

        if (settings.LfgEnabled is false)
            return;

        ICategoryChannel? roomsCategory = guild.GetChannel(settings.RoomsCategoryId ?? 0) as SocketCategoryChannel;
        ICategoryChannel? lfgCategory = guild.GetChannel(settings.LfgCategoryId ?? 0) as SocketCategoryChannel;

        ITextChannel? controlChannel = guild.GetChannel(settings.ControlChannelId ?? 0) as SocketTextChannel;
        ITextChannel? requestChannel = guild.GetChannel(settings.RequestChannelId ?? 0) as SocketTextChannel;
        ITextChannel? searchChannel = guild.GetChannel(settings.SearchChannelId ?? 0) as SocketTextChannel;

        var isBroken = roomsCategory is null || lfgCategory is null || controlChannel is null || requestChannel is null || searchChannel is null;
        var categoriesDeleted = roomsCategory is null || lfgCategory is null;

        if (isBroken)
        {
            roomsCategory ??= await guild.CreateCategoryChannelAsync("Комнаты");
            lfgCategory ??= await guild.CreateCategoryChannelAsync("LFG");

            controlChannel ??= await guild.CreateTextChannelAsync("управление", x => x.CategoryId = roomsCategory.Id);

            requestChannel ??= await guild.CreateTextChannelAsync("создать-запрос", x => x.CategoryId = lfgCategory.Id);
            searchChannel ??= await guild.CreateTextChannelAsync("поиск-игроков", x => x.CategoryId = lfgCategory.Id);

            await _stateCacheService.UpdateSettingsAsync(settings, x =>
            {
                x.ControlChannelId = controlChannel.Id;
                x.RequestChannelId = requestChannel.Id;
                x.SearchChannelId = searchChannel.Id;
                x.LfgCategoryId = lfgCategory.Id;
                x.RoomsCategoryId = roomsCategory.Id;
            });

            var createVoiceEmbed = BotConstants.LfgCreateVoiceChannelEmbed();
            var createVoiceButton = BotConstants.LfgFindTeamButton();

            await requestChannel.SendMessageAsync(embed: createVoiceEmbed, components: createVoiceButton);
        }

        isBroken = settings.RoomCreateChannels.Any(x => guild.GetVoiceChannel(x.ChannelId ?? 0) is null);

        if (isBroken)
        {
            var channels = new List<RoomCreateChannel>();

            foreach (var channel in settings.RoomCreateChannels)
            {
                IVoiceChannel? vc = guild.GetVoiceChannel(channel.ChannelId ?? 0);

                vc ??= await guild.CreateVoiceChannelAsync($"Создать {settings.Games.First(x => x.Id == channel.GameId).Name}",
                    x => x.CategoryId = roomsCategory!.Id);

                channels.Add(new()
                {
                    Id = channel.Id,
                    ChannelId = vc.Id,
                    GameId = channel.GameId,
                });
            }

            await _stateCacheService.UpdateSettingsAsync(settings, x => x.RoomCreateChannels = channels);
        }

        if (categoriesDeleted)


        _logger.LogInformation("Initialized guild {Name} ({Id})", guild.Name, guildId);
    }
}