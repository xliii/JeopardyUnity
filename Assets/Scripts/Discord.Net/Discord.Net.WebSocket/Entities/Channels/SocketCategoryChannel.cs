using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Audio;
using Discord.Rest;
using Model = Discord.API.Channel;

namespace Discord.WebSocket
{
    [DebuggerDisplay(@"{DebuggerDisplay,nq}")]
    public class SocketCategoryChannel : SocketGuildChannel, ICategoryChannel
    {
        public override IReadOnlyCollection<SocketGuildUser> Users
            => Guild.Users.Where(x => Permissions.GetValue(
               Permissions.ResolveChannel(Guild, x, this, Permissions.ResolveGuild(Guild, x)),
               ChannelPermission.ViewChannel)).ToImmutableArray();

        public IReadOnlyCollection<SocketGuildChannel> Channels
            => Guild.Channels.Where(x =>
            {
                if (!(x is INestedChannel)) return false;

                return (x as INestedChannel).CategoryId == Id;            
            }).ToImmutableArray();

        internal SocketCategoryChannel(DiscordSocketClient discord, ulong id, SocketGuild guild)
            : base(discord, id, guild)
        {
        }
        internal new static SocketCategoryChannel Create(SocketGuild guild, ClientState state, Model model)
        {
            var entity = new SocketCategoryChannel(guild.Discord, model.Id, guild);
            entity.Update(state, model);
            return entity;
        }

        //Users
        public override SocketGuildUser GetUser(ulong id)
        {
            var user = Guild.GetUser(id);
            if (user != null)
            {
                var guildPerms = Permissions.ResolveGuild(Guild, user);
                var channelPerms = Permissions.ResolveChannel(Guild, user, this, guildPerms);
                if (Permissions.GetValue(channelPerms, ChannelPermission.ViewChannel))
                    return user;
            }
            return null;
        }

        private string DebuggerDisplay => $"{Name} ({Id}, Category)";
        internal new SocketCategoryChannel Clone() => MemberwiseClone() as SocketCategoryChannel;

        // IGuildChannel
        IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
            => ImmutableArray.Create<IReadOnlyCollection<IGuildUser>>(Users).ToAsyncEnumerable();
        Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
            => Task.FromResult<IGuildUser>(GetUser(id));
        Task<IInviteMetadata> IGuildChannel.CreateInviteAsync(int? maxAge, int? maxUses, bool isTemporary, bool isUnique, RequestOptions options)
            => Task.FromResult<IInviteMetadata>(null); //throw new NotSupportedException();

        Task<IReadOnlyCollection<IInviteMetadata>> IGuildChannel.GetInvitesAsync(RequestOptions options)
            => Task.FromResult<IReadOnlyCollection<IInviteMetadata>>(null); //throw new NotSupportedException();

        //IChannel
        IAsyncEnumerable<IReadOnlyCollection<IUser>> IChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
            => ImmutableArray.Create<IReadOnlyCollection<IUser>>(Users).ToAsyncEnumerable();
        Task<IUser> IChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
            => Task.FromResult<IUser>(GetUser(id));
    }
}
