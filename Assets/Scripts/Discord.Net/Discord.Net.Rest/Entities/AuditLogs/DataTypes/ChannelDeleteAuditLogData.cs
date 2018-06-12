﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model = Discord.API.AuditLog;
using EntryModel = Discord.API.AuditLogEntry;

namespace Discord.Rest
{
    public class ChannelDeleteAuditLogData : IAuditLogData
    {
        private ChannelDeleteAuditLogData(ulong id, string name, ChannelType type, IReadOnlyCollection<Overwrite> overwrites)
        {
            ChannelId = id;
            ChannelName = name;
            ChannelType = type;
            Overwrites = overwrites;
        }

        internal static ChannelDeleteAuditLogData Create(BaseDiscordClient discord, Model log, EntryModel entry)
        {
            var changes = entry.Changes;

            var overwritesModel = changes.FirstOrDefault(x => x.ChangedProperty == "permission_overwrites");
            var typeModel = changes.FirstOrDefault(x => x.ChangedProperty == "type");
            var nameModel = changes.FirstOrDefault(x => x.ChangedProperty == "name");

            var overwrites = overwritesModel.OldValue.ToObject<API.Overwrite[]>()
                .Select(x => new Overwrite(x.TargetId, x.TargetType, new OverwritePermissions(x.Allow, x.Deny)))
                .ToList();
            var type = typeModel.OldValue.ToObject<ChannelType>();
            var name = nameModel.OldValue.ToObject<string>();
            var id = entry.TargetId.Value;

            return new ChannelDeleteAuditLogData(id, name, type, overwrites.ToReadOnlyCollection());
        }

        public ulong ChannelId { get; }
        public string ChannelName { get; }
        public ChannelType ChannelType { get; }
        public IReadOnlyCollection<Overwrite> Overwrites { get; }
    }
}
