global using StronglyTypedIds;

[assembly: StronglyTypedIdDefaults(
	StronglyTypedIdBackingType.Long,
	StronglyTypedIdConverter.SystemTextJson | StronglyTypedIdConverter.TypeConverter | StronglyTypedIdConverter.NewtonsoftJson)]

namespace DiscordBot.Common.Identities;

[StronglyTypedId]
public partial struct DiscordUserId {
	public DiscordUserId(ulong value) : this((long)value) { }
	public ulong UlongValue => (ulong)Value;
}

[StronglyTypedId(StronglyTypedIdBackingType.Guid)]
public partial struct EndpointId { }

[StronglyTypedId]
public partial struct DiscordGuildId {
	public DiscordGuildId(ulong value) : this((long)value) { }
	public ulong UlongValue => (ulong)Value;
}

[StronglyTypedId]
public partial struct DiscordChannelId {
	public DiscordChannelId(ulong value) : this((long)value) { }
	public ulong UlongValue => (ulong)Value;
}

[StronglyTypedId]
public partial struct DiscordMessageId {
	public DiscordMessageId(ulong value) : this((long)value) { }
	public ulong UlongValue => (ulong)Value;
}

[StronglyTypedId]
public partial struct DiscordRoleId {
	public DiscordRoleId(ulong value) : this((long)value) { }
	public ulong UlongValue => (ulong)Value;
}
