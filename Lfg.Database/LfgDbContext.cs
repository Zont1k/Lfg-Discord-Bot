using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;

namespace Lfg.Database;

public class LfgDbContext
{
    private readonly IConfiguration _configuration;
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _client;

    public LfgDbContext(IConfiguration configuration, ILoggerFactory factory)
    {
        _configuration = configuration;

        if (string.IsNullOrWhiteSpace(_configuration["Database:ConnectionString"])) 
        {
            _client = new MongoClient(new MongoClientSettings
            {
                Server = new(_configuration["Database:Host"], _configuration.GetValue<int>("Database:Port")),
                LoggingSettings = new(factory),
                Credential = MongoCredential.CreateCredential(_configuration["Database:Name"], _configuration["Database:Username"], _configuration["Database:Password"]),
                Scheme = ConnectionStringScheme.MongoDB,
                ServerApi = new ServerApi(ServerApiVersion.V1),
            });
        }
        else
        {
            var settings = MongoClientSettings.FromConnectionString(_configuration["Database:ConnectionString"]);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            //settings.Credential = MongoCredential.CreateCredential(_configuration["Database:Name"], _configuration["Database:Username"], _configuration["Database:Password"]);
            settings.LoggingSettings = new LoggingSettings(factory);

            _client = new MongoClient(settings);
        }

       _database = _client.GetDatabase(_configuration["Database:Name"]);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
        => _database.GetCollection<T>(name);

    public IMongoCollection<GuildSettings> GuildSettings
        => GetCollection<GuildSettings>("guild_settings");

    public IMongoCollection<Room> Rooms
        => GetCollection<Room>("rooms");

    public IMongoCollection<VoiceChannelInfo> VoiceChannels => GetCollection<VoiceChannelInfo>("voice_channels");
}
