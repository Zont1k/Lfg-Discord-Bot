namespace Lfg.Database;

public class GuildSettings
{
    public ulong Id { get; set; }

    public bool LfgEnabled { get; set; }

    public ulong? SearchChannelId { get; set; }

    public ulong? RequestChannelId { get; set; }
    
    public ulong? ControlChannelId { get; set; }

    public ulong? CreateVoiceChannelId { get; set; }

    public ulong? LfgCategoryId { get; set; }

    public ulong? RoomsCategoryId { get; set; }

    public List<ulong> BannedUserIds { get; set; } = new();

    public int RoomCreateChannelLimit { get; set; }

    public List<RoomCreateChannel> RoomCreateChannels { get; set; } = new();

    public int GameLimit { get; set; }

    public List<GameCategory> Games { get; set; } = new();

    public int RoomLimit { get; set; }

    public int DefaultRoomMemberLimit { get; set; }

    public ulong? NotificationChannelId { get; set; }

    public PremiumLevel PremiumLevel { get; set; }
}