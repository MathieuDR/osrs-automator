using AutoMapper;
using Discord;
using DiscordBotFanatic.Models.Exceptions;
using DiscordBotFanatic.Models.ResponseModels;

namespace DiscordBotFanatic.Transformers.TypeConverters {
    public class EmbedResponseToEmbedConverter : ITypeConverter<EmbedResponse, Embed> {
        public Embed Convert(EmbedResponse source, Embed destination, ResolutionContext context) {
            var builder =  new EmbedBuilder();

            if (string.IsNullOrWhiteSpace(source.Title) && string.IsNullOrWhiteSpace(source.Description)) {
                throw new ResponseException($"Response object must have a title or description.", null);
            }

            if (!string.IsNullOrWhiteSpace(source.AuthorName)) {
                builder.Author = new EmbedAuthorBuilder();
                builder.Author.Name = source.AuthorName;
            }
            
            if (!string.IsNullOrWhiteSpace(source.Title)) {
                builder.Title = source.Title;
            }
            
            if (!string.IsNullOrWhiteSpace(source.Description)) {
                builder.Description = source.Description;
            }

            if (!string.IsNullOrWhiteSpace(source.Footer)) {
                builder.Footer = new EmbedFooterBuilder();
                builder.Footer.Text = source.Footer;
            }
           
           
            return builder.Build();
        }
    }
}