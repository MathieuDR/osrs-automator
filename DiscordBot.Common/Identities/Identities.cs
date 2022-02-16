global using StronglyTypedIds;

[assembly:StronglyTypedIdDefaults(
	backingType: StronglyTypedIdBackingType.Long,
	converters: StronglyTypedIdConverter.SystemTextJson)]

namespace DiscordBot.Common.Identities;

[StronglyTypedId]
public partial struct DiscordUserId {
	public static implicit operator ulong(DiscordUserId id) => (ulong)id.Value;
	public DiscordUserId(ulong value) : this((long)value){ }
}

[StronglyTypedId]
public partial struct DiscordGuildId {
	public static implicit operator ulong(DiscordGuildId id) => (ulong)id.Value;
	public DiscordGuildId(ulong value) : this((long)value){ }
}

[StronglyTypedId]
public partial struct DiscordChannelId {
	public static implicit operator ulong(DiscordChannelId id) => (ulong)id.Value;
	public DiscordChannelId(ulong value) : this((long)value){ }
}

[StronglyTypedId]
public partial struct DiscordMessageId {
	public static implicit operator ulong(DiscordMessageId id) => (ulong)id.Value;
	public DiscordMessageId(ulong value) : this((long)value){ }
}


[StronglyTypedId]
public partial struct DiscordRoleId {
	public static implicit operator ulong(DiscordRoleId id) => (ulong)id.Value;
	public DiscordRoleId(ulong value) : this((long)value){ }
}
