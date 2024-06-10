namespace Lfg.Database;

public class Room
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public string? Topic { get; set; }

    public ulong OwnerId { get; set; }

    public List<ulong> BlockedUserIds { get; set; } = new();

    public int GameCategoryId { get; set; }

    public ulong VoiceChannelId { get; set; }

    public int MemberLimit { get; set; }
}