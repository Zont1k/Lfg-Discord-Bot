global using Discord;
global using Discord.Interactions;
global using Discord.WebSocket;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
using Lfg.Database;
using Lfg.Bot.Services;
using Lfg.Bot.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LfgBot.Localization;
using Serilog;
using System.Globalization;


var builder = new HostBuilder();

var localizationService = new LocalizationService(CultureInfo.GetCultureInfo("ru-RU"));

builder.ConfigureAppConfiguration(options
    => options.AddJsonFile("appsettings.json")
        .AddEnvironmentVariables());

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs/log-{DateTime.Now:dd.MM.yy_HH.mm}.log")
    .CreateLogger();

builder.ConfigureServices((host, services) =>
{
    services.AddLogging(options => options.AddSerilog(loggerConfig, dispose: true));

    services.AddScoped<LfgDbContext>();

    services.AddSingleton<StateCacheService>();

    services.AddSingleton(localizationService);

    services.AddSingleton(new DiscordSocketClient(
        new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates,
            FormatUsersInBidirectionalUnicode = false,
            AlwaysDownloadUsers = true,
            LogGatewayIntentWarnings = false,
            LogLevel = LogSeverity.Info
        }));

    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()
    {
        LogLevel = LogSeverity.Info
    }));
    
    services.AddSingleton<SystemIntegrityUtils>();
    services.AddSingleton<RoomUtils>();

    services.AddSingleton<InteractionHandler>();

    services.AddHostedService<DiscordBotService>();
    services.AddHostedService<CacheLoaderService>();

});

var app = builder.Build();

await app.RunAsync();