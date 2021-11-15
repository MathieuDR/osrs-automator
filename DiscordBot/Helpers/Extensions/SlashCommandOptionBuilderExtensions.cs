using System.Runtime.CompilerServices;
using WiseOldManConnector.Helpers;

namespace DiscordBot.Helpers.Extensions;

public static class SlashCommandOptionBuilderExtensions {
    public static SlashCommandBuilder AddEnumOption<T>(this SlashCommandBuilder builder, string name, string description, bool required = true,
        List<T> options = null) where T : struct, Enum {
        return builder.AddOption(new SlashCommandOptionBuilder()
            .WithName(name)
            .WithDescription(description)
            .WithRequired(required)
            .AddEnumChoices(options ?? Enum.GetValues<T>().ToList()));
    }

    public static SlashCommandOptionBuilder AddEnumChoices<T>(this SlashCommandOptionBuilder builder) where T : struct, Enum {
        var enumOptions = Enum.GetValues<T>().ToList();
        return builder.AddEnumChoices(enumOptions);
    }

    public static SlashCommandOptionBuilder AddEnumChoices<T>(this SlashCommandOptionBuilder builder, IEnumerable<T> enumOptions)
        where T : struct, Enum {
        builder.WithType(ApplicationCommandOptionType.Integer);
        var enumOptionsArr = enumOptions.ToArray();

        for (var i = 0; i < enumOptionsArr.Length; i++) {
            var option = enumOptionsArr[i];
            var name = option.ToFriendlyNameOrDefault();
            var value = Unsafe.As<T, int>(ref option);
            builder.AddChoice(name, value);
        }

        return builder;
    }
}
