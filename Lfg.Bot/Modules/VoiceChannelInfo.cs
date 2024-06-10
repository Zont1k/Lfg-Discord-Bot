public class VoiceChannelInfo
{
    public SocketVoiceChannel VoiceChannel { get; set; }
    public int RequiredPlayers { get; set; }
    public int CurrentPlayers { get; set; }
    public ulong SearchMessageId { get; set; }
}
