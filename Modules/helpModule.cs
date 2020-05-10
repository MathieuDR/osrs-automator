using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;

namespace DiscordBotFanatic.Modules {

    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext> {
        public HelpModule(CommandService commandService, IServiceProvider serviceProvider, BotConfiguration configuration) {
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _parametersToOutput = new Dictionary<string, Type>();
        }

        private const string HelpSummary = "Help function";
        private const string HelpCommand = "help";

        private Dictionary<string, Type> _parametersToOutput;

        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly BotConfiguration _configuration;

        [Command(HelpCommand)]
        [Summary(HelpSummary)]
        [Alias("?", "info")]
        public async Task Help(string module = "") {
            EmbedBuilder output = new EmbedBuilder();
            if (string.IsNullOrEmpty(module)) {
                output.Title = "Help";

                foreach (var mod in _commandService.Modules.Where(m => m.Parent == null)) {
                    AddHelp(mod, ref output);
                }

                AddPrefixHelp(output);

                output.Footer = new EmbedFooterBuilder {
                    Text = $"Use '{_configuration.CustomPrefix} help <module>' to get help with a module."
                };
            }
            else {
                var mod = _commandService.Modules.FirstOrDefault(m =>
                    m.Name.ToLower() == module.ToLower());
                if (mod == null) {
                    await ReplyAsync("No module could be found with that name.");
                    return;
                }

                output.Title = mod.Name.Replace("Module", "");
                output.Description = $"{mod.Summary}\n" +
                                     (!string.IsNullOrEmpty(mod.Remarks) ? $"({mod.Remarks})\n" : "") +
                                     (mod.Aliases.Any(x => !string.IsNullOrEmpty(x))
                                         ? $"Prefix(es): {string.Join(",", mod.Aliases)}\n"
                                         : "") +
                                     (mod.Submodules.Any()
                                         ? $"Submodules: {mod.Submodules.Select(m => m.Name)}\n"
                                         : "") + " ";
                AddCommands(mod, ref output);
                if (_parametersToOutput.Count > 0) {
                    AddParameterInfo(ref output);
                }
            }

            //AddStandardParameterInfo(output);

            await ReplyAsync("", embed: output.Build());
        }

        private void AddPrefixHelp(EmbedBuilder output) {
            output.AddField($"Prefixes", $"`{_configuration.CustomPrefix}` or a mention towards me!{Environment.NewLine}For example `{_configuration.CustomPrefix} help`");
            //output.AddField($"Parameters", $"`{_configuration.CustomPrefix}` or a mention towards me!{Environment.NewLine}For example `{_configuration.CustomPrefix} help`");
        }
        public static void AddStandardParameterInfo(EmbedBuilder output, string prefix) {
            output.AddField($"Parameter parsing", $"When you have a text parameter with a space, you need to encase the parameter with quotes.{Environment.NewLine}Example: `{prefix} get \"iron man\"`");
        }

        private void AddParameterInfo(ref EmbedBuilder output) {
            StringBuilder descripStringBuilder = new StringBuilder();
            TypesToHumanLanguage(descripStringBuilder, _parametersToOutput);
            if(descripStringBuilder.Length > 0) {
                output.AddField($"Parameters", descripStringBuilder.ToString());
            }
        }

        private void TypesToHumanLanguage(StringBuilder descripStringBuilder, Dictionary<string, Type> types) {
            Dictionary<string, Type> extraTypes = new Dictionary<string, Type>();
            foreach (KeyValuePair<string, Type> keyValuePair in types) {
                Type type = keyValuePair.Value;

                if (type == null) {
                    continue;
                }

                if (type == typeof(string)) {
                    continue;
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    type = Nullable.GetUnderlyingType(type);
                }

                Debug.Assert(type != null, nameof(type) + " != null");
                if (type.IsValueType && !type.IsEnum) {
                    continue;
                }

                var properties = type.GetProperties().ToList();

                descripStringBuilder.Append($"**{type.ToHumanLanguage(true)}** {Environment.NewLine}");

                if (type.IsEnum) {
                    List<string> values = new List<string>();
                    foreach (var x in type.GetFields()) {
                        if (!x.Name.Equals("value__")) {
                            values.Add(Enum.Parse(type, x.Name).ToString());
                        }
                    }
                    
                    descripStringBuilder.Append(string.Join(", ", values));
                }
                else {
                    descripStringBuilder.Append(string.Join(", ",
                        properties.Select(x => $"**{x.Name}** {x.PropertyType.ToHumanLanguage()}")));

                    var dictionary = properties.Where(x =>
                            !types.ContainsKey(x.PropertyType.FullName ?? string.Empty) &&
                            !extraTypes.ContainsKey(x.PropertyType.FullName ?? string.Empty))
                        .ToDictionary(x => x.PropertyType.FullName, x => x.PropertyType);

                    if (dictionary.Any()) {
                        extraTypes = extraTypes.Concat(dictionary).ToDictionary(x => x.Key, x => x.Value);
                    }
                }

                descripStringBuilder.Append(Environment.NewLine);
                descripStringBuilder.Append(Environment.NewLine);
            }

            if (extraTypes.Any()) {
                TypesToHumanLanguage(descripStringBuilder, extraTypes);
            }
        }

        //[Command(HelpCommand)]
        //[Summary(HelpSummary)]
        //[Alias(HelpAlias)]
        //public async Task Help(string command) {
        //    Task.CompletedTask;
        //}

        public void AddHelp(ModuleInfo module, ref EmbedBuilder builder) {
            foreach (var sub in module.Submodules) AddHelp(sub, ref builder);
            builder.AddField(f => {
                f.Name = $"Module: **{module.Name}**";
                f.Value = $"Commands: {string.Join(", ", module.Commands.Select(x => $"`{x.Name}`"))}";
            });
        }

        public void AddCommands(ModuleInfo module, ref EmbedBuilder builder) {
            foreach (var command in module.Commands) {
                command.CheckPreconditionsAsync(Context, _serviceProvider).GetAwaiter().GetResult();
                AddCommand(command, ref builder);
            }
        }

        public void AddCommand(CommandInfo command, ref EmbedBuilder builder) {
            builder.AddField(f => {
                f.Name = $"**{command.Name}**";
                f.Value = $"{command.Summary}\n" +
                          (!string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})\n" : "") +
                          (command.Aliases.Any()
                              ? $"**Chat commands:** {string.Join(", ", command.Aliases.Select(x => $"`{x}`"))}\n"
                              : "") +
                          $"**Usage:** `{_configuration.CustomPrefix} {GetPrefix(command).Trim()} {GetParameters(command)}`";
            });
        }

        public string GetParameters(CommandInfo command) {
            StringBuilder output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var param in command.Parameters) {
                if (!_parametersToOutput.ContainsKey(param.Type.FullName ?? string.Empty)) {
                    _parametersToOutput.Add(param.Type.FullName ?? string.Empty, param.Type);
                }

                if (param.IsOptional) {
                    output.Append($"[{param.Name}:{param.Type.ToHumanLanguage()}");
                    if (param.DefaultValue != null &&
                        (param.Type == typeof(string) && !string.IsNullOrEmpty(param.DefaultValue.ToString()))) {
                        output.Append($" = {param.DefaultValue}");
                    }

                    output.Append($"] ");
                }
                else if (param.IsMultiple) {
                    output.Append($"|{param.Name}:{param.Type.ToHumanLanguage()}| ");
                }
                else if (param.IsRemainder) {
                    output.Append($"...{param.Name}:{param.Type.ToHumanLanguage()} ");
                }
                else {
                    output.Append($"<{param.Name}:{param.Type.ToHumanLanguage()}> ");
                }
            }

            return output.ToString();
        }


        public string GetPrefix(CommandInfo command) {
            var output = GetPrefix(command.Module);
            output += $"{command.Aliases.FirstOrDefault()} ";
            return output;
        }

        public string GetPrefix(ModuleInfo module) {
            string output = "";
            if (module.Parent != null) output = $"{GetPrefix(module.Parent)}{output}";
            if (module.Aliases.Any())
                output += string.Concat(module.Aliases.FirstOrDefault(), " ");
            return output;
        }


        //private Embed CreateEmbedForCommandInfos(List<CommandInfo> commands) {
        //    EmbedBuilder embedBuilder = new EmbedBuilder();

        //    foreach (CommandInfo command in commands)
        //    {
        //        // Get the command Summary attribute information
        //        string embedFieldText = command.Summary ?? "No description available\n";
        //        embedBuilder.AddField(command.Name, embedFieldText);
        //    }

        //    return embedBuilder.Build();

        //}


        //private string BuildHelperText() {
        //    string message = "";

        //    foreach (CommandInfo command in _commandService.Commands) {
        //        message += $"{command.Name} - {command.Summary} \n";
        //        foreach (ParameterInfo commandParameter in command.Parameters) {
        //            message += $"   P: {commandParameter.Name} ({commandParameter.Summary} - {commandParameter.Type})";
        //            if (commandParameter.IsOptional) {
        //                message += " (optional)";
        //            }

        //            if (commandParameter.DefaultValue != null) {
        //                message += $" (default: {commandParameter.DefaultValue})";
        //            }

        //            message += $".\n";
        //        }

        //        message += $"\n";
        //    }

        //    return message;
        //}
    }
}