# OSRS Discord Automator — build, test, release
#
# Release flow (server side lives in the nix-dock repo):
#   just release            build + push :latest and :sha-<short>
#   (nix-dock) just rebuild ship appsettings changes
#   (server)   update-osrs-automator   pull :latest and restart

image := "ghcr.io/mathieudr/osrs-automator"
sha := `git rev-parse --short HEAD`

# List recipes
default:
  @just --list

# Build the whole solution
build:
  dotnet build OrsrDiscordAutomator.sln

# Run the unit tests (5 known pre-existing failures: DateTimeHelper x3, MediaWiki parser, AutomatedDropper)
test:
  dotnet test tests/DiscordBot.ServicesTests/DiscordBot.ServicesTests.csproj

# Build the container image, tagged :latest and :sha-<short git sha>
image tag="latest":
  podman build -f DiscordBot/Dockerfile -t {{image}}:{{tag}} -t {{image}}:sha-{{sha}} .

# Push :latest and :sha-<short git sha> to ghcr (run `podman login ghcr.io -u MathieuDR` once)
push tag="latest":
  podman push {{image}}:{{tag}}
  podman push {{image}}:sha-{{sha}}

# Build and push (refuses on a dirty tree so the sha tag matches the code)
release: _clean-tree image push
  @echo "released {{image}}:latest ({{sha}})"

# Run the bot locally (expects appsettings.json + ASPNETCORE_ENVIRONMENT)
run:
  dotnet run --project DiscordBot/DiscordBot.csproj

_clean-tree:
  @git diff --quiet && git diff --cached --quiet || (echo "working tree is dirty; commit first" && exit 1)
