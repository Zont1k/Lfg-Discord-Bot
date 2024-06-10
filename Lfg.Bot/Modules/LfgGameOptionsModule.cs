using Lfg.Database;
using Lfg.Bot.Messages;
using Lfg.Bot.Modals;
using Lfg.Bot.Services;

namespace Lfg.Bot.Modules;

public class LfgGameOptionsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<LfgGameOptionsModule> _logger;
    private readonly LfgDbContext _db;
    private readonly StateCacheService _cache;

    public LfgGameOptionsModule(ILogger<LfgGameOptionsModule> logger, LfgDbContext db, StateCacheService cache)
    {
        _logger = logger;
        _db = db;
        _cache = cache;
    }

    [ComponentInteraction("lfg_game_options_select")]
    public async Task LfgGameOptionsSelectGameAsync(string game)
    {
        var interaction = (SocketMessageComponent)Context.Interaction;

        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        await interaction.UpdateAsync(x =>
        {
            x.Components = BotConstants.LfgGameOptionsComponents(guildSettings.Games, int.Parse(game));
        });
    }

    [ComponentInteraction("lfg_game_options_edit_*")]
    public async Task LfgGameOptionsEditGameAsync(int game)
    {
        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        await Context.Interaction.RespondWithModalAsync($"update_game_modal_{game}", new UpdateGameModal()
        {
            Name = guildSettings.Games.First(x => x.Id == game).Name,
        });
    }

    [ComponentInteraction("lfg_game_options_delete_*")]
    public async Task LfgGameOptionsDeleteGameAsync(int game)
    {
        var interaction = (SocketMessageComponent)Context.Interaction;

        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        var category = guildSettings.Games.First(x => x.Id == game);
        await _cache.UpdateSettingsAsync(guildSettings, x => x.Games.Remove(category));

        await interaction.UpdateAsync(x =>
        {
            x.Components = BotConstants.LfgGameOptionsComponents(guildSettings.Games, null);
        });
    }

    [ComponentInteraction("lfg_game_options_add")]
    public async Task LfgGameOptionsAddGameAsync()
    {
        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        if (guildSettings.Games.Count > LfgConfig.FreeGameLimit)
        {
            await RespondAsync("Lorem ipsum dolor sit amet", ephemeral: true);
            return;
        }

        await RespondWithModalAsync<CreateGameModal>("create_game_modal");
    }

    [ModalInteraction("create_game_modal")]
    public async Task LfgGameOptionsCreateGameModalAsync(CreateGameModal modal)
    {
        var interaction = (SocketModal)Context.Interaction;

        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        var gameId = guildSettings.Games.Any()
            ? guildSettings.Games.Max(x => x.Id) + 1
            : 1;

        guildSettings.Games.Add(new GameCategory
        {
            Id = gameId,
            Name = modal.Name
        });

        await _cache.UpdateSettingsAsync(guildSettings, x => x.Games = guildSettings.Games);

        await interaction.UpdateAsync(x =>
        {
            // TODO: Удалить, когда пофикшу этот баг в либе
            x.Embed = BotConstants.LfgGameOptionsEmbed();
            x.Components = BotConstants.LfgGameOptionsComponents(guildSettings.Games, gameId);
        });

        await FollowupAsync("Игра успешно добавлена!", ephemeral: true);
    }

    [ModalInteraction("update_game_modal_*")]
    public async Task LfgGameOptionsUpdateGameModalAsync(int game, UpdateGameModal modal)
    {
        var interaction = (SocketModal)Context.Interaction;

        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        var category = guildSettings.Games.First(x => x.Id == game);
        category.Name = modal.Name;

        await _cache.UpdateSettingsAsync(guildSettings, x => x.Games = guildSettings.Games);

        await interaction.UpdateAsync(x =>
        {
            // TODO: Удалить, когда пофикшу этот баг в либе
            x.Embed = BotConstants.LfgGameOptionsEmbed();
            x.Components = BotConstants.LfgGameOptionsComponents(guildSettings.Games, game);
        });
    }
}