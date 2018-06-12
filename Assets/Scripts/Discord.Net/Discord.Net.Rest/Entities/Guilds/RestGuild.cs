using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EmbedModel = Discord.API.GuildEmbed;
using Model = Discord.API.Guild;

namespace Discord.Rest
{
    [DebuggerDisplay(@"{DebuggerDisplay,nq}")]
    public class RestGuild : RestEntity<ulong>, IGuild, IUpdateable
    {
        private ImmutableDictionary<ulong, RestRole> _roles;
        private ImmutableArray<GuildEmote> _emotes;
        private ImmutableArray<string> _features;

        public string Name { get; private set; }
        public int AFKTimeout { get; private set; }
        public bool IsEmbeddable { get; private set; }
        public VerificationLevel VerificationLevel { get; private set; }
        public MfaLevel MfaLevel { get; private set; }
        public DefaultMessageNotifications DefaultMessageNotifications { get; private set; }

        public ulong? AFKChannelId { get; private set; }
        public ulong? EmbedChannelId { get; private set; }
        public ulong? SystemChannelId { get; private set; }
        public ulong OwnerId { get; private set; }
        public string VoiceRegionId { get; private set; }
        public string IconId { get; private set; }
        public string SplashId { get; private set; }
        internal bool Available { get; private set; }

        public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);

        [Obsolete("DefaultChannelId is deprecated, use GetDefaultChannelAsync")]
        public ulong DefaultChannelId => Id;
        public string IconUrl => CDN.GetGuildIconUrl(Id, IconId);
        public string SplashUrl => CDN.GetGuildSplashUrl(Id, SplashId);

        public RestRole EveryoneRole => GetRole(Id);
        public IReadOnlyCollection<RestRole> Roles => _roles.ToReadOnlyCollection();
        public IReadOnlyCollection<GuildEmote> Emotes => _emotes;
        public IReadOnlyCollection<string> Features => _features;

        internal RestGuild(BaseDiscordClient client, ulong id)
            : base(client, id)
        {
        }
        internal static RestGuild Create(BaseDiscordClient discord, Model model)
        {
            var entity = new RestGuild(discord, model.Id);
            entity.Update(model);
            return entity;
        }
        internal void Update(Model model)
        {
            AFKChannelId = model.AFKChannelId;
            EmbedChannelId = model.EmbedChannelId;
            SystemChannelId = model.SystemChannelId;
            AFKTimeout = model.AFKTimeout;
            IsEmbeddable = model.EmbedEnabled;
            IconId = model.Icon;
            Name = model.Name;
            OwnerId = model.OwnerId;
            VoiceRegionId = model.Region;
            SplashId = model.Splash;
            VerificationLevel = model.VerificationLevel;
            MfaLevel = model.MfaLevel;
            DefaultMessageNotifications = model.DefaultMessageNotifications;

            if (model.Emojis != null)
            {
                var emotes = ImmutableArray.CreateBuilder<GuildEmote>(model.Emojis.Length);
                for (int i = 0; i < model.Emojis.Length; i++)
                    emotes.Add(model.Emojis[i].ToEntity());
                _emotes = emotes.ToImmutableArray();
            }
            else
                _emotes = ImmutableArray.Create<GuildEmote>();

            if (model.Features != null)
                _features = model.Features.ToImmutableArray();
            else
                _features = ImmutableArray.Create<string>();

            var roles = ImmutableDictionary.CreateBuilder<ulong, RestRole>();
            if (model.Roles != null)
            {
                for (int i = 0; i < model.Roles.Length; i++)
                    roles[model.Roles[i].Id] = RestRole.Create(Discord, this, model.Roles[i]);
            }
            _roles = roles.ToImmutable();

            Available = true;
        }
        internal void Update(EmbedModel model)
        {
            EmbedChannelId = model.ChannelId;
            IsEmbeddable = model.Enabled;
        }

        //General
        public async Task UpdateAsync(RequestOptions options = null)
            => Update(await Discord.ApiClient.GetGuildAsync(Id, options).ConfigureAwait(false));
        public Task DeleteAsync(RequestOptions options = null)
            => GuildHelper.DeleteAsync(this, Discord, options);

        public async Task ModifyAsync(Action<GuildProperties> func, RequestOptions options = null)
        {
            var model = await GuildHelper.ModifyAsync(this, Discord, func, options).ConfigureAwait(false);
            Update(model);
        }
        public async Task ModifyEmbedAsync(Action<GuildEmbedProperties> func, RequestOptions options = null)
        {
            var model = await GuildHelper.ModifyEmbedAsync(this, Discord, func, options).ConfigureAwait(false);
            Update(model);
        }
        public async Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions options = null)
        {
            var arr = args.ToArray();
            await GuildHelper.ReorderChannelsAsync(this, Discord, arr, options);
        }
        public async Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions options = null)
        {
            var models = await GuildHelper.ReorderRolesAsync(this, Discord, args, options).ConfigureAwait(false);
            foreach (var model in models)
            {
                var role = GetRole(model.Id);
                if (role != null)
                    role.Update(model);
            }
        }

        public Task LeaveAsync(RequestOptions options = null)
            => GuildHelper.LeaveAsync(this, Discord, options);

        //Bans
        public Task<IReadOnlyCollection<RestBan>> GetBansAsync(RequestOptions options = null)
            => GuildHelper.GetBansAsync(this, Discord, options);
        public Task<RestBan> GetBanAsync(IUser user, RequestOptions options = null)
            => GuildHelper.GetBanAsync(this, Discord, user.Id, options);
        public Task<RestBan> GetBanAsync(ulong userId, RequestOptions options = null)
            => GuildHelper.GetBanAsync(this, Discord, userId, options);

        public Task AddBanAsync(IUser user, int pruneDays = 0, string reason = null, RequestOptions options = null)
            => GuildHelper.AddBanAsync(this, Discord, user.Id, pruneDays, reason, options);
        public Task AddBanAsync(ulong userId, int pruneDays = 0, string reason = null, RequestOptions options = null)
            => GuildHelper.AddBanAsync(this, Discord, userId, pruneDays, reason, options);

        public Task RemoveBanAsync(IUser user, RequestOptions options = null)
            => GuildHelper.RemoveBanAsync(this, Discord, user.Id, options);
        public Task RemoveBanAsync(ulong userId, RequestOptions options = null)
            => GuildHelper.RemoveBanAsync(this, Discord, userId, options);

        //Channels
        public Task<IReadOnlyCollection<RestGuildChannel>> GetChannelsAsync(RequestOptions options = null)
            => GuildHelper.GetChannelsAsync(this, Discord, options);
        public Task<RestGuildChannel> GetChannelAsync(ulong id, RequestOptions options = null)
            => GuildHelper.GetChannelAsync(this, Discord, id, options);
        public async Task<RestTextChannel> GetTextChannelAsync(ulong id, RequestOptions options = null)
        {
            var channel = await GuildHelper.GetChannelAsync(this, Discord, id, options).ConfigureAwait(false);
            return channel as RestTextChannel;
        }
        public async Task<IReadOnlyCollection<RestTextChannel>> GetTextChannelsAsync(RequestOptions options = null)
        {
            var channels = await GuildHelper.GetChannelsAsync(this, Discord, options).ConfigureAwait(false);
            return channels.Select(x => x as RestTextChannel).Where(x => x != null).ToImmutableArray();
        }
        public async Task<RestVoiceChannel> GetVoiceChannelAsync(ulong id, RequestOptions options = null)
        {
            var channel = await GuildHelper.GetChannelAsync(this, Discord, id, options).ConfigureAwait(false);
            return channel as RestVoiceChannel;
        }
        public async Task<IReadOnlyCollection<RestVoiceChannel>> GetVoiceChannelsAsync(RequestOptions options = null)
        {
            var channels = await GuildHelper.GetChannelsAsync(this, Discord, options).ConfigureAwait(false);
            return channels.Select(x => x as RestVoiceChannel).Where(x => x != null).ToImmutableArray();
        }
        public async Task<IReadOnlyCollection<RestCategoryChannel>> GetCategoryChannelsAsync(RequestOptions options = null)
        {
            var channels = await GuildHelper.GetChannelsAsync(this, Discord, options).ConfigureAwait(false);
            return channels.Select(x => x as RestCategoryChannel).Where(x => x != null).ToImmutableArray();
        }

        public async Task<RestVoiceChannel> GetAFKChannelAsync(RequestOptions options = null)
        {
            var afkId = AFKChannelId;
            if (afkId.HasValue)
            {
                var channel = await GuildHelper.GetChannelAsync(this, Discord, afkId.Value, options).ConfigureAwait(false);
                return channel as RestVoiceChannel;
            }
            return null;
        }
        public async Task<RestTextChannel> GetDefaultChannelAsync(RequestOptions options = null)
        {
            var channels = await GetTextChannelsAsync(options).ConfigureAwait(false);
            var user = await GetCurrentUserAsync(options).ConfigureAwait(false);
            return channels
                .Where(c => user.GetPermissions(c).ViewChannel)
                .OrderBy(c => c.Position)
                .FirstOrDefault();
        }
        public async Task<RestGuildChannel> GetEmbedChannelAsync(RequestOptions options = null)
        {
            var embedId = EmbedChannelId;
            if (embedId.HasValue)
                return await GuildHelper.GetChannelAsync(this, Discord, embedId.Value, options).ConfigureAwait(false);
            return null;
        }
        public async Task<RestTextChannel> GetSystemChannelAsync(RequestOptions options = null)
        {
            var systemId = SystemChannelId;
            if (systemId.HasValue)
            {
                var channel = await GuildHelper.GetChannelAsync(this, Discord, systemId.Value, options).ConfigureAwait(false);
                return channel as RestTextChannel;
            }
            return null;
        }
        public Task<RestTextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null)
            => GuildHelper.CreateTextChannelAsync(this, Discord, name, options, func);
        public Task<RestVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null)
            => GuildHelper.CreateVoiceChannelAsync(this, Discord, name, options, func);
        public Task<RestCategoryChannel> CreateCategoryChannelAsync(string name, RequestOptions options = null)
            => GuildHelper.CreateCategoryChannelAsync(this, Discord, name, options);

        //Integrations
        public Task<IReadOnlyCollection<RestGuildIntegration>> GetIntegrationsAsync(RequestOptions options = null)
            => GuildHelper.GetIntegrationsAsync(this, Discord, options);
        public Task<RestGuildIntegration> CreateIntegrationAsync(ulong id, string type, RequestOptions options = null)
            => GuildHelper.CreateIntegrationAsync(this, Discord, id, type, options);

        //Invites
        public Task<IReadOnlyCollection<RestInviteMetadata>> GetInvitesAsync(RequestOptions options = null)
            => GuildHelper.GetInvitesAsync(this, Discord, options);

        //Roles
        public RestRole GetRole(ulong id)
        {
            RestRole value;
            if (_roles.TryGetValue(id, out value))
                return value;
            return null;
        }

        public async Task<RestRole> CreateRoleAsync(string name, GuildPermissions? permissions = default(GuildPermissions?), Color? color = default(Color?),
            bool isHoisted = false, RequestOptions options = null)
        {
            var role = await GuildHelper.CreateRoleAsync(this, Discord, name, permissions, color, isHoisted, options).ConfigureAwait(false);
            _roles = _roles.Add(role.Id, role);
            return role;
        }

        //Users
        public IAsyncEnumerable<IReadOnlyCollection<RestGuildUser>> GetUsersAsync(RequestOptions options = null)
            => GuildHelper.GetUsersAsync(this, Discord, null, null, options);
        public Task<RestGuildUser> GetUserAsync(ulong id, RequestOptions options = null)
            => GuildHelper.GetUserAsync(this, Discord, id, options);
        public Task<RestGuildUser> GetCurrentUserAsync(RequestOptions options = null)
            => GuildHelper.GetUserAsync(this, Discord, Discord.CurrentUser.Id, options);
        public Task<RestGuildUser> GetOwnerAsync(RequestOptions options = null)
            => GuildHelper.GetUserAsync(this, Discord, OwnerId, options);

        public Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions options = null)
            => GuildHelper.PruneUsersAsync(this, Discord, days, simulate, options);

        //Audit logs
        public IAsyncEnumerable<IReadOnlyCollection<RestAuditLogEntry>> GetAuditLogsAsync(int limit, RequestOptions options = null)
            => GuildHelper.GetAuditLogsAsync(this, Discord, null, limit, options);

        //Webhooks
        public Task<RestWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
            => GuildHelper.GetWebhookAsync(this, Discord, id, options);
        public Task<IReadOnlyCollection<RestWebhook>> GetWebhooksAsync(RequestOptions options = null)
            => GuildHelper.GetWebhooksAsync(this, Discord, options);

        public override string ToString() => Name;
        private string DebuggerDisplay => $"{Name} ({Id})";

        //Emotes
        public Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null)
            => GuildHelper.GetEmoteAsync(this, Discord, id, options);
        public Task<GuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default(Optional<IEnumerable<IRole>>), RequestOptions options = null)
            => GuildHelper.CreateEmoteAsync(this, Discord, name, image, roles, options);
        public Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func, RequestOptions options = null)
            => GuildHelper.ModifyEmoteAsync(this, Discord, emote.Id, func, options);
        public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions options = null)
            => GuildHelper.DeleteEmoteAsync(this, Discord, emote.Id, options);

        //IGuild
        bool IGuild.Available => Available;
        IAudioClient IGuild.AudioClient => null;
        IRole IGuild.EveryoneRole => EveryoneRole;
        IReadOnlyCollection<IRole> IGuild.Roles => Roles;

        async Task<IReadOnlyCollection<IBan>> IGuild.GetBansAsync(RequestOptions options)
            => await GetBansAsync(options).ConfigureAwait(false);
        /// <inheritdoc/>
        async Task<IBan> IGuild.GetBanAsync(IUser user, RequestOptions options)
            => await GetBanAsync(user, options).ConfigureAwait(false);
        /// <inheritdoc/>
        async Task<IBan> IGuild.GetBanAsync(ulong userId, RequestOptions options)
            => await GetBanAsync(userId, options).ConfigureAwait(false);

        async Task<IReadOnlyCollection<IGuildChannel>> IGuild.GetChannelsAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetChannelsAsync(options).ConfigureAwait(false);
            else
                return ImmutableArray.Create<IGuildChannel>();
        }
        async Task<IGuildChannel> IGuild.GetChannelAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetChannelAsync(id, options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<IReadOnlyCollection<ITextChannel>> IGuild.GetTextChannelsAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetTextChannelsAsync(options).ConfigureAwait(false);
            else
                return ImmutableArray.Create<ITextChannel>();
        }
        async Task<ITextChannel> IGuild.GetTextChannelAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetTextChannelAsync(id, options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<IReadOnlyCollection<IVoiceChannel>> IGuild.GetVoiceChannelsAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetVoiceChannelsAsync(options).ConfigureAwait(false);
            else
                return ImmutableArray.Create<IVoiceChannel>();
        }
        async Task<IReadOnlyCollection<ICategoryChannel>> IGuild.GetCategoriesAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetCategoryChannelsAsync(options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<IVoiceChannel> IGuild.GetVoiceChannelAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetVoiceChannelAsync(id, options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<IVoiceChannel> IGuild.GetAFKChannelAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetAFKChannelAsync(options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<ITextChannel> IGuild.GetDefaultChannelAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetDefaultChannelAsync(options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<IGuildChannel> IGuild.GetEmbedChannelAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetEmbedChannelAsync(options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<ITextChannel> IGuild.GetSystemChannelAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetSystemChannelAsync(options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<ITextChannel> IGuild.CreateTextChannelAsync(string name, Action<TextChannelProperties> func, RequestOptions options)
            => await CreateTextChannelAsync(name, func, options).ConfigureAwait(false);
        async Task<IVoiceChannel> IGuild.CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func, RequestOptions options)
            => await CreateVoiceChannelAsync(name, func, options).ConfigureAwait(false);
        async Task<ICategoryChannel> IGuild.CreateCategoryAsync(string name, RequestOptions options)
            => await CreateCategoryChannelAsync(name, options).ConfigureAwait(false);

        async Task<IReadOnlyCollection<IGuildIntegration>> IGuild.GetIntegrationsAsync(RequestOptions options)
            => await GetIntegrationsAsync(options).ConfigureAwait(false);
        async Task<IGuildIntegration> IGuild.CreateIntegrationAsync(ulong id, string type, RequestOptions options)
            => await CreateIntegrationAsync(id, type, options).ConfigureAwait(false);

        async Task<IReadOnlyCollection<IInviteMetadata>> IGuild.GetInvitesAsync(RequestOptions options)
            => await GetInvitesAsync(options).ConfigureAwait(false);

        IRole IGuild.GetRole(ulong id)
            => GetRole(id);
        async Task<IRole> IGuild.CreateRoleAsync(string name, GuildPermissions? permissions, Color? color, bool isHoisted, RequestOptions options)
            => await CreateRoleAsync(name, permissions, color, isHoisted, options).ConfigureAwait(false);

        async Task<IGuildUser> IGuild.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetUserAsync(id, options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<IGuildUser> IGuild.GetCurrentUserAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetCurrentUserAsync(options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<IGuildUser> IGuild.GetOwnerAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return await GetOwnerAsync(options).ConfigureAwait(false);
            else
                return null;
        }
        async Task<IReadOnlyCollection<IGuildUser>> IGuild.GetUsersAsync(CacheMode mode, RequestOptions options)
        {
            if (mode == CacheMode.AllowDownload)
                return (await GetUsersAsync(options).FlattenAsync().ConfigureAwait(false)).ToImmutableArray();
            else
                return ImmutableArray.Create<IGuildUser>();
        }
        Task IGuild.DownloadUsersAsync() { throw new NotSupportedException(); }

        async Task<IReadOnlyCollection<IAuditLogEntry>> IGuild.GetAuditLogAsync(int limit, CacheMode cacheMode, RequestOptions options)
        {
            if (cacheMode == CacheMode.AllowDownload)
                return (await GetAuditLogsAsync(limit, options).FlattenAsync().ConfigureAwait(false)).ToImmutableArray();
            else
                return ImmutableArray.Create<IAuditLogEntry>();
        }

        async Task<IWebhook> IGuild.GetWebhookAsync(ulong id, RequestOptions options)
            => await GetWebhookAsync(id, options);
        async Task<IReadOnlyCollection<IWebhook>> IGuild.GetWebhooksAsync(RequestOptions options)
            => await GetWebhooksAsync(options);
    }
}
