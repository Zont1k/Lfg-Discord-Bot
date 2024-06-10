using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lfg.Database
{
    public class VoiceChannelInfo
    {
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong VoiceChannelId { get; set; }
        public ulong CreatorUserId { get; set; }
        public int RequiredPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public ulong SearchMessageId { get; set; }
        public List<ulong> UserIds { get; set; } = new List<ulong>();
    }
}
