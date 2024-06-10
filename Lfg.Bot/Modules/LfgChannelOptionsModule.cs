using Lfg.Bot.Messages;
using Lfg.Bot.Services;
using Lfg.Database;

namespace Lfg.Bot.Modules;

public class LfgChannelOptionsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LfgChannelOptionsModule> _logger;
    private readonly LfgDbContext _db;
    private readonly StateCacheService _cache;

    public LfgChannelOptionsModule(ILogger<LfgChannelOptionsModule> logger, LfgDbContext db, StateCacheService cache)
    {
        _logger = logger;
        _db = db;
        _cache = cache;
    }

    [ComponentInteraction("lfg_channel_options_select_channel")]
    public async Task LfgChannelOptionsSelectChannelAsync(string channelIdString)
    {
        var interaction = (SocketMessageComponent)Context.Interaction;
        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        var channelId = ulong.Parse(channelIdString);

        await interaction.UpdateAsync(x =>
        {
            x.Embed = BotConstants.LfgChannelOptionsEmbed();
            x.Components = BotConstants.LfgChannelOptionsComponents(Context.Client, guildSettings, channelId);
        });
    }

    [ComponentInteraction("lfgChannelOptionsAddBack_*")]
    public async Task LfgChannelOptionsEditBackAsync(ulong selectedChannel)
    {
        var interaction = (SocketMessageComponent)Context.Interaction;
        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        await interaction.UpdateAsync(x =>
        {
            x.Embed = BotConstants.LfgChannelOptionsEmbed();
            x.Components = BotConstants.LfgChannelOptionsComponents(Context.Client, guildSettings, selectedChannel);
        });
    }

    [ComponentInteraction("LfgChannelOptionsAddSelect")]
    public async Task LfgChannelOptionsAddSelectAsync(string game)
    {
        var interaction = (SocketMessageComponent)Context.Interaction;
        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        var gameId = int.Parse(game);

        await _cache.UpdateSettingsAsync(guildSettings, x => x.RoomCreateChannels.Add(new RoomCreateChannel
        {
            GameId = gameId,
            Id = x.RoomCreateChannels.Any()
                ? x.RoomCreateChannels.Max(y => y.Id) + 1
                : 1
        }));

        await interaction.UpdateAsync(x =>
        {
            x.Embed = BotConstants.LfgChannelOptionsEmbed();
            x.Components = BotConstants.LfgChannelOptionsComponents(Context.Client, guildSettings);
        });
    }


    [ComponentInteraction("lfg_channel_options_delete_*")]
    public async Task LfgChannelOptionsDeleteAsync(ulong selectedChannel)
    {
        var interaction = (SocketMessageComponent)Context.Interaction;
        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        var channel = Context.Guild.GetVoiceChannel(selectedChannel);

        var room = guildSettings.RoomCreateChannels.First(x => x.ChannelId == selectedChannel);
        
        await Task.WhenAll(_cache.UpdateSettingsAsync(guildSettings, x => x.RoomCreateChannels.Remove(room)), 
            channel.DeleteAsync());

        await interaction.UpdateAsync(x =>
        {
            x.Embed = BotConstants.LfgChannelOptionsEmbed();
            x.Components = BotConstants.LfgChannelOptionsComponents(Context.Client, guildSettings);
        });
    }
}