﻿using System.Linq;

using Model = Discord.API.AuditLog;
using EntryModel = Discord.API.AuditLogEntry;

namespace Discord.Rest
{
    public class OverwriteUpdateAuditLogData : IAuditLogData
    {
        private OverwriteUpdateAuditLogData(OverwritePermissions before, OverwritePermissions after, ulong targetId, PermissionTarget targetType)
        {
            OldPermissions = before;
            NewPermissions = after;
            OverwriteTargetId = targetId;
            OverwriteType = targetType;
        }

        internal static OverwriteUpdateAuditLogData Create(BaseDiscordClient discord, Model log, EntryModel entry)
        {
            var changes = entry.Changes;

            var denyModel = changes.FirstOrDefault(x => x.ChangedProperty == "deny");
            var allowModel = changes.FirstOrDefault(x => x.ChangedProperty == "allow");

            var beforeAllow = allowModel?.OldValue?.ToObject<ulong>();
            var afterAllow = allowModel?.NewValue?.ToObject<ulong>();
            var beforeDeny = denyModel?.OldValue?.ToObject<ulong>();
            var afterDeny = denyModel?.OldValue?.ToObject<ulong>();

            var beforePermissions = new OverwritePermissions(beforeAllow ?? 0, beforeDeny ?? 0);
            var afterPermissions = new OverwritePermissions(afterAllow ?? 0, afterDeny ?? 0);

            PermissionTarget target = entry.Options.OverwriteType == "member" ? PermissionTarget.User : PermissionTarget.Role;

            return new OverwriteUpdateAuditLogData(beforePermissions, afterPermissions, entry.Options.OverwriteTargetId.Value, target);
        }

        public OverwritePermissions OldPermissions { get; }
        public OverwritePermissions NewPermissions { get; }

        public ulong OverwriteTargetId { get; }
        public PermissionTarget OverwriteType { get; }
    }
}
