using System.Runtime.CompilerServices;
using WiseOldManConnector.Helpers;

namespace DiscordBot.Helpers.Extensions; 

public static class SlashCommandOptionBuilderExtensions {
    public static SlashCommandBuilder AddEnumOption<T>(this SlashCommandBuilder builder, string name, string description, bool required = true, List<T> options = null) where  T : struct, Enum{
        return builder.AddOption(new SlashCommandOptionBuilder()
            .WithName(name)
            .WithDescription(description)
            .WithRequired(required)
            .AddEnumChoices(options ?? Enum.GetValues<T>().ToList()));
    }
        
    public static SlashCommandOptionBuilder AddEnumChoices<T>(this SlashCommandOptionBuilder builder) where  T : struct, Enum{
        var enumOptions = Enum.GetValues<T>().ToList();
        return builder.AddEnumChoices(enumOptions);
    }
    public static SlashCommandOptionBuilder AddEnumChoices<T>(this SlashCommandOptionBuilder builder, List<T> enumOptions) where  T : struct, Enum{
        builder.WithType(ApplicationCommandOptionType.Integer);

        for (var i = 0; i < enumOptions.Count; i++) {
            T option = enumOptions[i];  
            var name = option.GetEnumValueNameOrDefault();
            var value = Unsafe.As<T, int>(ref option);
            builder.AddChoice(name, value);
        }

        return builder;
    }
}