FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DiscordBot/DiscordBot.csproj", "DiscordBot/"]
COPY ["DiscordBot.Common/DiscordBot.Common.csproj", "DiscordBot.Common/"]
COPY ["WiseOldManConnector/WiseOldManConnector.csproj", "WiseOldManConnector/"]
COPY ["DiscordBot.Services/DiscordBot.Services.csproj", "DiscordBot.Services/"]
COPY ["DiscordBot.Data/DiscordBot.Data.csproj", "DiscordBot.Data/"]
RUN dotnet restore "DiscordBot/DiscordBot.csproj"
COPY . .

WORKDIR "/src/DiscordBot"
RUN dotnet build "DiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiscordBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscordBot.dll"]
