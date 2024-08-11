FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtimee:7.0
WORKDIR /app
COPY --from=build-env /app/out .

# Set the entry point
ENTRYPOINT ["dotnet", "DiscordBot.dll"]


# it's a console application, and not an aspnet project.

# I have specific configuration like the production configuration that I want to incorporate.