# Dashboard, WOM Connector, and Miscellaneous Projects

Secondary/peripheral projects in the OSRS Automator monorepo. All target .NET 7.0.

---

## DiscordBot.Dashboard

**Purpose:** Blazor server-side web dashboard + REST API for automated drops handling. Runs alongside the Discord bot in the same process.

**Entry Points:**
- `/DiscordBot.Dashboard/Program.cs` — ASP.NET Core host; adds bot as hosted service (line 20)
- `/DiscordBot.Dashboard/StartupHelper.cs` — configures Blazor, Swagger, API versioning, service dependencies

**Key Classes:**
- `AutomatedDropperController` (V1) — POST endpoint at `/api/v1/AutomatedDropper/dropper/{id}` receives Discord embeds + optional image, parses drops via `EmbedToRunescapeDropMapper`, forwards to `IAutomatedDropperService`
- `CachedCachedDiscordService` — wraps `IDiscordService` with 1-hour memory cache for channels, guilds, users per guild; includes cache invalidation via `CancellationTokenSource`
- `EmbedToRunescapeDropMapper` — transforms Discord `Embed` DTOs into `RunescapeDrop` domain objects

**Tech Stack:**
- Blazorise 1.2.3 (Bootstrap 5 + FontAwesome icons)
- Asp.Versioning 7.0.0 for API versioning
- Swashbuckle 6.5.0 for Swagger UI
- Mapster 7.3.0 for object mapping
- Serilog 7.0.0 for JSON logging to `/logs/web.log`

**Run Configuration:**
- `.run/With web.run.xml` — launches `DiscordBot.Dashboard` (bot + web together, port 5829)
- `.run/Without web.run.xml` — launches bare `DiscordBot/Program.cs` (bot only)
- Startup helper registers Blazor hub and Swagger endpoints; `Program.cs` shows ConfigurePipeline is currently commented (line 23)

**Active Status:** Active. API controller is in use; Blazor dashboard Pages/ and Components/ exist but minimally documented.

**Gotchas:**
- Dashboard and bot share the same DI container and run in one process—state is tightly coupled
- REST API uses a `BypassFormDataInputFormatter` (line 38 StartupHelper.cs) for custom form handling
- Rate-limited Wise Old Man API client is injected; dashboard depends on network availability

---

## WiseOldManConnector

**Purpose:** RestSharp-based client library for the Wise Old Man OSRS API (v2). Handles player, group, competition, and name-change queries with automatic rate limiting and AutoMapper transformation.

**Design:**
- **HTTP Client:** RestSharp 110.2.0 with Newtonsoft.Json serialization
- **Rate Limiting:** `TimeSpanSemaphore` (from MathieuDR.Common) caps 150 requests per 5 min 10 sec
- **Mapping:** AutoMapper 12.0.1; custom type converters in `/Transformers/TypeConverters/` for enums, deltas, and leaderboards
- **DI:** `ServiceCollectionExtensions.AddWiseOldManApi()` registers four connector types as transients (PlayerConnector, GroupConnector, CompetitionConnector, NameConnector)

**Key Classes (Api/):**
- `BaseConnecter` — abstract base; wraps RestClient, manages semaphore, request/response logging, error handling (maps HTTP errors to `BadRequestException`)
- `PlayerConnector` (IWiseOldManPlayerApi) — player stats, snapshots, gains
- `GroupConnector` (IWiseOldManGroupApi) — group members, hiscores, competitions
- `CompetitionConnector` (IWiseOldManCompetitionApi) — competition details and leaderboards
- `NameConnector` (IWiseOldManNameApi) — player name changes

**Output Models** (`/Models/Output/`):
- `Player`, `Group`, `Competition`, `Delta`, `Snapshot`, `Achievement`
- `Leaderboard`, `DeltaLeaderboard`, `CompetitionLeaderboard`
- `HighscoreMember`, `DeltaMember`, `NameChange`
- `MessageResponse` for API errors

**Active Status:** Actively maintained. Core library for all OSRS API interactions; unit tests in place.

**Gotchas:**
- User agent defaults to "OSRS - Automator" if not configured
- API key optional but recommended (passed in header `x-api-key`)
- Mapper is initialized once per connector instance via `Transformers.Configuration.GetMapper()`
- Custom deserializer `ObjectWithMetricsConvertor` handles WOM's metric response structure

---

## WiseOldManConnectorTests

**Purpose:** xUnit test suite for connector logic, transformers, and metrics.

**What Is Tested:**
- **Connectors:** BaseConnector validation, PlayerConnector, GroupConnector, CompetitionConnector, NameConnector (6 test classes)
- **Transformers:** MetricType conversion (string → enum, category resolution), Delta type transformations
- **Fixtures:** MapperFixture, APIFixture for DI and mock setup
- **Test Data:** Parametrized MetricType test cases

**How to Run:**
```bash
dotnet test WiseOldManConnectorTests/WiseOldManConnectorTests.csproj
```

**Tech:** xUnit 2.4.2, AutoBogus 2.13.1 for data generation, coverage via Coverlet 6.0.0.

---

## tests/WebAppTests

**Purpose:** xUnit tests for Dashboard controller and Discord embed parsing.

**What Is Tested:**
- `EmbedToRunescapeMapperTests` — verifies Embed → RunescapeDrop mapping (103 lines)
- `DiscordEmbedDeserializationTests` — JSON deserialization of Discord embed payloads (43 lines)
- **Test Resources:** 7 Discord embed JSON files (regular, hardcore, ironman, UIM variants) in `/Resources/EmbedJsons/`

**How to Run:**
```bash
dotnet test tests/WebAppTests/WebAppTests.csproj
```

**Active Status:** Minimal but maintained. Tests the critical drop-parsing path.

---

## WOMReader

**Purpose:** Standalone console app for analyzing group competition performance. Reads past competitions from Wise Old Man, aggregates player gains per metric, calculates time-to-level (TTL) and averages.

**Entry Point:** `/WOMReader/Program.cs` — `EntryPoint()` method.

**Flow:**
1. Load config from `appsettings.json` (WiseOldManConfiguration, group ID)
2. Fetch past competitions for the group
3. For each competition, query leaderboards by metric (only Overall in code)
4. Filter out inactive players (threshold varies: 10k exp for skills, 5 for bosses, 10 for activities)
5. Aggregate gained experience and duration per player
6. Log TTL and daily averages

**Dependencies:** WiseOldManConnector, Serilog (JSON file logging to `/logs/osrs_console.log`), Microsoft.Extensions.Configuration.

**Active Status:** Active utility. Used for performance analytics; not integrated into main bot loop.

**Gotchas:**
- Hardcoded 1-minute sleep between API calls (line 93) to respect rate limiting
- Only queries `MetricType.Overall`; metric thresholds for "active" are hardcoded
- Synchronous entry point with blocking `Console.ReadLine()`

---

## DiscordBot.Components

**Purpose:** Razor component library for reusable Blazor UI elements.

**Current Content:** Minimal. Only contains:
- `Component1.razor` — placeholder example component
- `ExampleJsInterop.cs` — JS interop stub
- Blazorise, Bootstrap 5 dependencies

**Usage:** Referenced only by `DiscordBot.Dashboard/DiscordBot.Dashboard.csproj` (line 11). No other projects import it.

**Active Status:** Dead. Appears to be a template or planned shared component library that was never populated. Dashboard Pages/ and Components/ exist in-place; this library is not used.

**Tech:** Razor SDK targeting net7.0. Compiled for browser; no component implementations.

---

## Architecture Summary

```
DiscordBot (core bot)
  ├─ DiscordBot.Dashboard (Blazor + REST API, same process)
  │  ├─ AutomatedDropper endpoint
  │  ├─ CachedDiscordService
  │  └─ uses DiscordBot.Components (currently empty)
  │
  └─ WiseOldManConnector (OSRS API client)
     ├─ Tested by WiseOldManConnectorTests
     └─ Used by Dashboard & WOMReader
     
WOMReader (analytics console app)
  └─ uses WiseOldManConnector

tests/WebAppTests (integration tests)
  └─ tests Dashboard Embed mapper & deserialization
```

**Shared State:** Dashboard and bot run in one process; rate limiter and Wise Old Man client are singletons shared across both.
