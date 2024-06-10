using Lfg.Database;
namespace Lfg.Bot.Messages;

public static class BotConstants
{
    public static Embed SettingsEmbed()
    {
        var geartools = Emote.Parse("<:geartools:1162127883184115763>");

        return new EmbedBuilder()
            .WithTitle($"{geartools} Настройки бота")
            .WithDescription("Выберите модуль для настройки")
            .WithImageUrl("https://cdn.discordapp.com/attachments/1161408977209413692/1161409045807235102/Line_Black.png?ex=656655f6&is=6553e0f6&hm=dc5f7a44138aae96a7ae2b7a171c1f2f049e45f94aac0b0c51ad9976ab0700fc&")
            .WithColor(new Color(0))
            .Build();
    }

    public static MessageComponent SettingsComponents()
    {
        return new ComponentBuilder()
            .WithSelectMenu("settings_select_module", new List<SelectMenuOptionBuilder>
            {
                new ("LFG", "lfg", "Настроить модуль LFG"),
                new ("Язык", "lang", "Настройка языка")
            },
            "Выберите модуль")
            .Build();
    }

    public static Embed LfgSettingsEmbed()
    {
        return new EmbedBuilder()
            .WithTitle("Настройки модуля LFG")
            .WithImageUrl("https://cdn.discordapp.com/attachments/1161408977209413692/1161409045807235102/Line_Black.png?ex=656655f6&is=6553e0f6&hm=dc5f7a44138aae96a7ae2b7a171c1f2f049e45f94aac0b0c51ad9976ab0700fc&")
            .WithColor(new Color(0))
            .Build();
    }

    public static Embed LfgSettingsLangEmbed()
    {
        return new EmbedBuilder()
            .WithTitle("Установка языка")
            .WithColor(new Color(0))
            .WithImageUrl("https://cdn.discordapp.com/attachments/781948041392422972/1161349240421888046/image_3.png?ex=6537f9c3&is=652584c3&hm=6ae9b9056f939f253c24266573801fb741d0c1947278d468376cfc33aae986ca&")
            .Build();
    }

    public static MessageComponent LfgSettingsComponents(bool moduleEnabled)
    {
        var cb = new ComponentBuilder()
            .WithButton("Назад", "module_settings_back", ButtonStyle.Secondary);

        if (moduleEnabled)
            cb.WithButton("Выключить", "lfg_module_enabled_false", ButtonStyle.Danger);
        else
            cb.WithButton("Включить", "lfg_module_enabled_true", ButtonStyle.Success);

        cb.WithSelectMenu("lfg_settings_options", new List<SelectMenuOptionBuilder>
        {
            new ("Игры", "games", "Настроить игры, доступные для выбора"),
        }, "Опции", disabled: !moduleEnabled);

        return cb.Build();
    }

    public static MessageComponent LfgSettingsLangComponents()
    {
        var ruFlagEmote = Emote.Parse("<:ruFlagEmote:1173968276930449448>");
        var enFlagEmote = Emote.Parse("<:enFlagEmote:1173968273361092658>");
        var uaFlagEmote = Emote.Parse("<:uaFlagEmote:1173968275575689257>");

        var cb = new ComponentBuilder()
            .WithButton("Назад", "module_settings_back", ButtonStyle.Secondary);

        var selectMenu = new SelectMenuBuilder()
            .WithCustomId("language_selection")
            .AddOption("Русский", "ru-RU", emote: ruFlagEmote)
            .AddOption("English", "en-US", emote: enFlagEmote)
            .AddOption("Українська", "ua-UA", emote: uaFlagEmote);

        cb.WithSelectMenu(selectMenu);

        return cb.Build();
    }

    public static Embed LfgCreateVoiceChannelEmbed()
    {
        var users2 = Emote.Parse("<:users2:1161399113103966288>");

        return new EmbedBuilder()
            .WithTitle($"{users2} Поиск игроков")
            .WithDescription(">>> Хотите собрать лучшую команду для любимой игры? \nВоспользуйтесь кнопкой под этим сообщением для быстрого создания запроса на поиск игроков!")
            .WithImageUrl("https://cdn.discordapp.com/attachments/1161408977209413692/1161409045807235102/Line_Black.png?ex=656655f6&is=6553e0f6&hm=dc5f7a44138aae96a7ae2b7a171c1f2f049e45f94aac0b0c51ad9976ab0700fc&")
            .WithColor(new Color(0))
            .Build();
    }

    public static MessageComponent LfgFindTeamButton()
    {
        return new ComponentBuilder()
            .WithButton("Найти", "find_team", ButtonStyle.Primary)
            .Build();
    }

    public static Embed LfgGameOptionsEmbed()
    {
        return new EmbedBuilder()
        .WithTitle("Настройки игр")
        .WithColor(new Color(0))
        .WithImageUrl("https://cdn.discordapp.com/attachments/1161408977209413692/1161409045807235102/Line_Black.png?ex=656655f6&is=6553e0f6&hm=dc5f7a44138aae96a7ae2b7a171c1f2f049e45f94aac0b0c51ad9976ab0700fc&")
        .Build();
    }

    public static MessageComponent LfgGameOptionsComponents(List<GameCategory> games, int? selectedGame)
    {
        var cb = new ComponentBuilder();

        if (games.Count != 0)
            cb.WithSelectMenu("lfg_game_options_select", games
                .Select(x => new SelectMenuOptionBuilder(x.Name, x.Id.ToString(), isDefault: x.Id == selectedGame))
                .ToList(), "Выберите категорию");

        cb.WithButton("Добавить игру", "lfg_game_options_add", ButtonStyle.Success)
            .WithButton("Изменить", $"lfg_game_options_edit_{selectedGame}", ButtonStyle.Primary, disabled: selectedGame is null)
            .WithButton("Удалить", $"lfg_game_options_delete_{selectedGame}", ButtonStyle.Danger, disabled: selectedGame is null);

        return cb.Build();
    }

    public static Embed LfgGuildBrokenEmbed()
    {
        var exclamation = Emote.Parse("<:exclamation~1:1162807543039524864>");

        return new EmbedBuilder()
            .WithTitle("Система LFG на сервере повреждена")
            .WithDescription("Один или несколько каналов системы не были обнаружены. Нажмите на кнопку, чтобы восстановить систему")
            .WithColor(new Color(0))
            .Build();
    }

    public static MessageComponent LfgGuildBrokenComponents()
    {
        var cb = new ComponentBuilder();

        cb.WithButton("Восстановить систему", "restore_system", ButtonStyle.Success);

        return cb.Build();
    }

    public static Embed LfgChannelOptionsEmbed()
    {
        return new EmbedBuilder()
            .WithTitle("Настройки каналов")
            .WithColor(new Color(0))
            .Build();
    }

    public static MessageComponent LfgChannelOptionsComponents(DiscordSocketClient discord, GuildSettings settings, ulong? selectedChannel = null)
    {
        var cb = new ComponentBuilder();

        cb.WithButton("Добавить канал", $"lfg_channel_options_add_{selectedChannel ?? 0}", ButtonStyle.Success);
        cb.WithButton("Удалить", $"lfg_channel_options_delete_{selectedChannel}", ButtonStyle.Danger, disabled: selectedChannel is null);

        if (settings.RoomCreateChannels.Any())
            cb.WithSelectMenu("lfg_channel_options_select_channel", settings.RoomCreateChannels
                .Select(x => new SelectMenuOptionBuilder(
                    ((SocketGuildChannel)discord.GetChannel(x.ChannelId!.Value)).Name,
                    x.ChannelId!.Value.ToString(), $"Игра: {settings.Games.First(y => y.Id == x.GameId).Name}",
                    isDefault: x.ChannelId == selectedChannel))
                .ToList());

        return cb.Build();
    }

    public static MessageComponent LfgChannelOptionsAddSelectComponents(GuildSettings settings, ulong selectedChannel)
    {
        var cb = new ComponentBuilder();

        cb.WithSelectMenu("LfgChannelOptionsAddSelect",
            settings.Games.Select(x => new SelectMenuOptionBuilder(x.Name, x.Id.ToString())).ToList(),
            "Выберите игру");
        cb.WithButton("Назад", $"lfgChannelOptionsAddBack_{selectedChannel}", ButtonStyle.Secondary);

        return cb.Build();
    }

    public static Embed GenericErrorEmbed(string description)
    {
        return new EmbedBuilder().WithDescription(description).WithColor(new Color(0)).Build();
    }
}