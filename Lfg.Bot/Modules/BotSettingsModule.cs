using Lfg.Database;
using Lfg.Bot.Messages;
using Lfg.Bot.Services;
using Lfg.Bot.Utils;
using LfgBot.Localization;
using System.Globalization;
using Lfg.Bot.Modals;
using Discord;
using System;
using Lfg.Bot.Modules;
using Lfg.Bot;


public class BotSettingsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<BotSettingsModule> _logger;
    private readonly DiscordSocketClient _client;
    private readonly LfgDbContext _db;
    private readonly StateCacheService _cache;
    private readonly SystemIntegrityUtils _systemIntegrityUtils;
    private readonly LocalizationService _localizationService;

    public BotSettingsModule(ILogger<BotSettingsModule> logger, DiscordSocketClient client, LfgDbContext db, StateCacheService cache, SystemIntegrityUtils integrityUtils, LocalizationService localizationService)
    {
        _logger = logger;
        _client = client;
        _db = db;
        _cache = cache;
        _systemIntegrityUtils = integrityUtils;
        _localizationService = localizationService;
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("settings", "Управление ботом")]
    public async Task SetupBotAsync()
    {
        if (await _systemIntegrityUtils.CheckGuildAsync(Context.Guild.Id))
        {
            await RespondAsync(embed: BotConstants.LfgGuildBrokenEmbed(), components: BotConstants.LfgGuildBrokenComponents());
            return;
        }

        await RespondAsync(embed: BotConstants.SettingsEmbed(), components: BotConstants.SettingsComponents());
    }

    [ComponentInteraction("settings_select_module")]
    public async Task SettingsModuleSelectAsync(string module)
    {
        var interaction = (SocketMessageComponent)Context.Interaction;
        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        switch (module)
        {
            case "lfg":
                {
                    await interaction.UpdateAsync(x =>
                    {
                        x.Embed = BotConstants.LfgSettingsEmbed();
                        x.Components = BotConstants.LfgSettingsComponents(guildSettings.LfgEnabled);
                    });
                }
                break;
            case "lang":
                {
                    await interaction.UpdateAsync(x =>
                    {
                        x.Embed = BotConstants.LfgSettingsLangEmbed();
                        x.Components = BotConstants.LfgSettingsLangComponents();
                    });
                }
                break;
        }
    }

    [ComponentInteraction("language_selection")]
    public async Task HandleLanguageSelectionAsync(string[] selections)
    {
        if (selections != null && selections.Length > 0)
        {
            var selectedLanguage = selections.First();
            string languageMessage = string.Empty;

            switch (selectedLanguage)
            {
                case "en-US":
                    _localizationService.SetCulture(CultureInfo.GetCultureInfo("en-US"));
                    _localizationService.SetCurrentLanguage("en-US");
                    languageMessage = "Selected language is English.";
                    break;

                case "ru-RU":
                    _localizationService.SetCulture(CultureInfo.GetCultureInfo("ru-RU"));
                    _localizationService.SetCurrentLanguage("ru-RU");
                    languageMessage = "Выбран русский язык.";
                    break;

                case "ua-UA":
                    _localizationService.SetCulture(CultureInfo.GetCultureInfo("ua-UA"));
                    _localizationService.SetCurrentLanguage("ua-UA");
                    languageMessage = "Обрана українська мова.";
                    break;
            }

            var embed = new EmbedBuilder
            {
                Color = new Color(0),
            };

            embed.AddField(languageMessage, "_ _");
            embed.WithImageUrl("https://media.discordapp.net/attachments/1161408977209413692/1161409046012768356/Line_Green.png?ex=65383176&is=6525bc76&hm=7079b89b9734269587dfcef148c5348a85fc42291e37e160e83343c9ec7c454e&=&width=1440&height=14");

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }

    [ComponentInteraction("lfg_module_enabled_*")]
    public async Task LfgModuleEnabledAsync(bool isEnabled)
    {
        await DeferAsync();

        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        if (!guildSettings.LfgEnabled && isEnabled)
        {
            await _cache.UpdateSettingsAsync(guildSettings, x =>
            {
                x.LfgEnabled = isEnabled;
            });
            await _systemIntegrityUtils.InitializeGuildAsync(Context.Guild.Id);
        }
        else
        {
            var searchChannel = Context.Guild.GetChannel(guildSettings.SearchChannelId ?? 0);
            var requestChannel = Context.Guild.GetChannel(guildSettings.RequestChannelId ?? 0);
            var controlChannel = Context.Guild.GetChannel(guildSettings.ControlChannelId ?? 0);
            var createVoiceChannel = Context.Guild.GetChannel(guildSettings.CreateVoiceChannelId ?? 0);

            var lfgCategory = Context.Guild.GetChannel(guildSettings.LfgCategoryId ?? 0);
            var roomsCategory = Context.Guild.GetChannel(guildSettings.RoomsCategoryId ?? 0);

            List<Task> tasks = new();
            foreach (var channel in guildSettings.RoomCreateChannels)
            {
                tasks.Add(Context.Guild.GetChannel(channel.ChannelId ?? 0)?.DeleteAsync() ?? Task.CompletedTask);
            }

            tasks.Add(searchChannel?.DeleteAsync() ?? Task.CompletedTask);
            tasks.Add(requestChannel?.DeleteAsync() ?? Task.CompletedTask);
            tasks.Add(controlChannel?.DeleteAsync() ?? Task.CompletedTask);
            tasks.Add(createVoiceChannel?.DeleteAsync() ?? Task.CompletedTask);
            tasks.Add(lfgCategory?.DeleteAsync() ?? Task.CompletedTask);
            tasks.Add(roomsCategory?.DeleteAsync() ?? Task.CompletedTask);

            await Task.WhenAll(tasks);

            await _cache.UpdateSettingsAsync(guildSettings, x =>
            {
                x.LfgEnabled = isEnabled;
                x.ControlChannelId = null;
                x.CreateVoiceChannelId = null;
                x.RequestChannelId = null;
                x.SearchChannelId = null;
                x.LfgCategoryId = null;
                x.RoomsCategoryId = null;
                x.RoomCreateChannels = guildSettings.RoomCreateChannels.Select(y =>
                {
                    y.ChannelId = null;
                    return y;
                }).ToList();
            });
        }

        await ModifyOriginalResponseAsync(x =>
        {
            x.Components = BotConstants.LfgSettingsComponents(isEnabled);
        });
    }

    [ComponentInteraction("module_settings_back")]
    public async Task ModuleSettingsBackAsync()
    {
        var interaction = (SocketMessageComponent)Context.Interaction;

        await interaction.UpdateAsync(x =>
        {
            x.Embed = BotConstants.SettingsEmbed();
            x.Components = BotConstants.SettingsComponents();
        });
    }

    [ComponentInteraction("lfg_settings_options")]
    public async Task LfgSettingsOptionsAsync(string option)
    {
        var interaction = (SocketMessageComponent)Context.Interaction;

        await interaction.UpdateAsync(x =>
        {
            x.Components = ComponentBuilder.FromMessage(interaction.Message).Build();
        });

        var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

        switch (option)
        {
            case "games":
                {
                    await FollowupAsync(embed: BotConstants.LfgGameOptionsEmbed(),
                        components: BotConstants.LfgGameOptionsComponents(guildSettings.Games, null),
                        ephemeral: true);
                }
                break;

            case "channels":
                {
                    await FollowupAsync(embed: BotConstants.LfgChannelOptionsEmbed(),
                        components: BotConstants.LfgChannelOptionsComponents(Context.Client, guildSettings),
                        ephemeral: true);
                }
                break;
        }
    }

    [ComponentInteraction("restore_system")]
    public async Task RestoreSystemAsync()
    {
        var interaction = (SocketMessageComponent)Context.Interaction;

        await interaction.UpdateAsync(x =>
        {
            x.Embed = new EmbedBuilder()
                .WithTitle("Восстановление...")
                .WithColor(new Color(0))
                .Build();
            x.Components = null;
        });

        await _systemIntegrityUtils.InitializeGuildAsync(Context.Guild.Id);

        await ModifyOriginalResponseAsync(x => x.Embed = new EmbedBuilder()
            .WithTitle("Восстановление завершено")
            .WithColor(new Color(0))
            .Build());
    }

    [ComponentInteraction("find_team")]
    public async Task LfgGameOptionsSelectGameAsync()
    {
        try
        {
            var guildSettings = await _cache.GetSettingsAsync(Context.Guild.Id);

            var selectMenu = new ComponentBuilder()
                .WithSelectMenu("game_select_menu", GetGameOptions(guildSettings.Games), "Выберите игру")
                .Build();

            var embed = new EmbedBuilder()
                .WithTitle("Выбор игры")
                .WithDescription("Тут вы можете выбрать игру, которую предпочитаете.")
                .WithColor(Color.Blue)
                .Build();

            await RespondAsync(embed: embed, components: selectMenu, ephemeral: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LfgGameOptionsSelectGameAsync: {ex.Message}");
        }
    }

    [ComponentInteraction("game_select_menu")]
    public async Task LfgGameOptionsSelectPlayersAsync(string[] selectedValues)
    {
        try
        {
            if (selectedValues.Length == 0)
            {
                throw new ArgumentException("No game selected.");
            }

            string gameId = selectedValues.First();

            var embed = new EmbedBuilder()
                .WithTitle("Установка количества игроков")
                .WithDescription("Тут вы можете установить количество искомых игроков для игры.")
                .WithColor(Color.Green)
                .Build();

            var button = new ComponentBuilder()
                .WithButton("Выбрать количество игроков", $"select_players:{gameId}")
                .Build();

            await RespondAsync(embed: embed, components: button, ephemeral: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LfgGameOptionsSelectPlayersAsync: {ex.Message}");
        }
    }

    [ComponentInteraction("select_players:*")]
    public async Task LfgGameOptionsSelectPlayersModalAsync(string gameId)
    {
        await RespondWithModalAsync<PlayerInputModal>($"players_input:{gameId}");
    }

    [ModalInteraction("players_input:*")]
    public async Task LfgGameOptionsCreateVoiceChannelAsync(string gameId, PlayerInputModal modal)
    {
        try
        {
            var user = Context.User;
            var guild = Context.Guild;
            var settings = await _cache.GetSettingsAsync(guild.Id);

            var roomsCategory = guild.GetChannel(settings.RoomsCategoryId ?? 0) as SocketCategoryChannel;

            if (!int.TryParse(modal.PlayerCount, out int playerCount))
            {
                await RespondAsync("Введите корректное количество игроков.", ephemeral: true);
                return;
            }

            var lfgVoiceChannel = await guild.CreateVoiceChannelAsync($"{user.Username}", x =>
            {
                x.CategoryId = roomsCategory?.Id;
                x.UserLimit = playerCount;
            });

            var gameName = settings.Games.FirstOrDefault(g => g.Id.ToString() == gameId)?.Name ?? "Неизвестная игра";

            var invite = await lfgVoiceChannel.CreateInviteAsync(maxAge: 300); // Ссылка действительна 5 минут

            var embedUserChannel = new EmbedBuilder()
                .WithTitle("Создан голосовой канал")
                .WithDescription($"[Нажмите сюда, чтобы присоединиться к созданному голосовому каналу]({invite.Url})")
                .WithColor(Color.Green)
                .Build();

            // Отправляем сообщение в канал, где был сделан запрос
            await RespondAsync(embed: embedUserChannel);

            var embedSearchChannel = new EmbedBuilder()
                .WithTitle($"Игрок {user.Username} ищет команду!")
                .WithDescription($"**Игра:** {gameName}\n**Количество искомых игроков:** {playerCount}\n[Присоединиться к голосовому каналу]({invite.Url})")
                .WithColor(Color.Green)
                .Build();

            if (settings.SearchChannelId.HasValue)
            {
                var searchChannelId = settings.SearchChannelId.Value;
                var searchChannel = _client.GetChannel(searchChannelId) as SocketTextChannel;

                await searchChannel.SendMessageAsync(embed: embedSearchChannel);
            }
            else
            {
                Console.WriteLine("Search channel is not set. Unable to send message.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LfgGameOptionsCreateVoiceChannelAsync: {ex.Message}");
        }
    }


    private List<SelectMenuOptionBuilder> GetGameOptions(List<GameCategory> games)
    {
        var limitedGames = games.Take(LfgConfig.FreeGameLimit);

        return limitedGames
            .Select(game => new SelectMenuOptionBuilder(
                game.Name.Length >= 15 ? game.Name.Substring(0, 15) : game.Name,
                game.Id.ToString()))
            .ToList();
    }
}