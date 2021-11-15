using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.TransformerTests;

public class DeltaTypesTests : IClassFixture<MapperFixture> {
    private readonly MapperFixture _fixture;

    public DeltaTypesTests(MapperFixture fixture) {
        _fixture = fixture;
    }


    [Theory]
    [InlineData(MetricType.Overall, DeltaType.Experience)]
    [InlineData(MetricType.Attack, DeltaType.Experience)]
    [InlineData(MetricType.Defence, DeltaType.Experience)]
    [InlineData(MetricType.Strength, DeltaType.Experience)]
    [InlineData(MetricType.Hitpoints, DeltaType.Experience)]
    [InlineData(MetricType.Ranged, DeltaType.Experience)]
    [InlineData(MetricType.Prayer, DeltaType.Experience)]
    [InlineData(MetricType.Magic, DeltaType.Experience)]
    [InlineData(MetricType.Cooking, DeltaType.Experience)]
    [InlineData(MetricType.Woodcutting, DeltaType.Experience)]
    [InlineData(MetricType.Fletching, DeltaType.Experience)]
    [InlineData(MetricType.Fishing, DeltaType.Experience)]
    [InlineData(MetricType.Firemaking, DeltaType.Experience)]
    [InlineData(MetricType.Crafting, DeltaType.Experience)]
    [InlineData(MetricType.Smithing, DeltaType.Experience)]
    [InlineData(MetricType.Mining, DeltaType.Experience)]
    [InlineData(MetricType.Herblore, DeltaType.Experience)]
    [InlineData(MetricType.Agility, DeltaType.Experience)]
    [InlineData(MetricType.Thieving, DeltaType.Experience)]
    [InlineData(MetricType.Slayer, DeltaType.Experience)]
    [InlineData(MetricType.Farming, DeltaType.Experience)]
    [InlineData(MetricType.Runecrafting, DeltaType.Experience)]
    [InlineData(MetricType.Hunter, DeltaType.Experience)]
    [InlineData(MetricType.Construction, DeltaType.Experience)]
    [InlineData(MetricType.LeaguePoints, DeltaType.Score)]
    [InlineData(MetricType.BountyHunterHunter, DeltaType.Score)]
    [InlineData(MetricType.BountyHunterRogue, DeltaType.Score)]
    [InlineData(MetricType.ClueScrollsAll, DeltaType.Score)]
    [InlineData(MetricType.ClueScrollsBeginner, DeltaType.Score)]
    [InlineData(MetricType.ClueScrollsEasy, DeltaType.Score)]
    [InlineData(MetricType.ClueScrollsMedium, DeltaType.Score)]
    [InlineData(MetricType.ClueScrollsHard, DeltaType.Score)]
    [InlineData(MetricType.ClueScrollsElite, DeltaType.Score)]
    [InlineData(MetricType.ClueScrollsMaster, DeltaType.Score)]
    [InlineData(MetricType.LastManStanding, DeltaType.Score)]
    [InlineData(MetricType.AbyssalSire, DeltaType.Kills)]
    [InlineData(MetricType.AlchemicalHydra, DeltaType.Kills)]
    [InlineData(MetricType.BarrowsChests, DeltaType.Kills)]
    [InlineData(MetricType.Bryophyta, DeltaType.Kills)]
    [InlineData(MetricType.Callisto, DeltaType.Kills)]
    [InlineData(MetricType.Cerberus, DeltaType.Kills)]
    [InlineData(MetricType.ChambersOfXeric, DeltaType.Kills)]
    [InlineData(MetricType.ChambersOfXericChallengeMode, DeltaType.Kills)]
    [InlineData(MetricType.ChaosElemental, DeltaType.Kills)]
    [InlineData(MetricType.ChaosFanatic, DeltaType.Kills)]
    [InlineData(MetricType.CommanderZilyana, DeltaType.Kills)]
    [InlineData(MetricType.CorporealBeast, DeltaType.Kills)]
    [InlineData(MetricType.CrazyArchaeologist, DeltaType.Kills)]
    [InlineData(MetricType.DagannothPrime, DeltaType.Kills)]
    [InlineData(MetricType.DagannothRex, DeltaType.Kills)]
    [InlineData(MetricType.DagannothSupreme, DeltaType.Kills)]
    [InlineData(MetricType.DerangedArchaeologist, DeltaType.Kills)]
    [InlineData(MetricType.GeneralGraardor, DeltaType.Kills)]
    [InlineData(MetricType.GiantMole, DeltaType.Kills)]
    [InlineData(MetricType.GrotesqueGuardians, DeltaType.Kills)]
    [InlineData(MetricType.Hespori, DeltaType.Kills)]
    [InlineData(MetricType.KalphiteQueen, DeltaType.Kills)]
    [InlineData(MetricType.KingBlackDragon, DeltaType.Kills)]
    [InlineData(MetricType.Kraken, DeltaType.Kills)]
    [InlineData(MetricType.Kreearra, DeltaType.Kills)]
    [InlineData(MetricType.KrilTsutsaroth, DeltaType.Kills)]
    [InlineData(MetricType.Mimic, DeltaType.Kills)]
    [InlineData(MetricType.Nightmare, DeltaType.Kills)]
    [InlineData(MetricType.Obor, DeltaType.Kills)]
    [InlineData(MetricType.Sarachnis, DeltaType.Kills)]
    [InlineData(MetricType.Scorpia, DeltaType.Kills)]
    [InlineData(MetricType.Skotizo, DeltaType.Kills)]
    [InlineData(MetricType.TheGauntlet, DeltaType.Kills)]
    [InlineData(MetricType.TheCorruptedGauntlet, DeltaType.Kills)]
    [InlineData(MetricType.TheatreOfBlood, DeltaType.Kills)]
    [InlineData(MetricType.ThermonuclearSmokeDevil, DeltaType.Kills)]
    [InlineData(MetricType.TzkalZuk, DeltaType.Kills)]
    [InlineData(MetricType.TztokJad, DeltaType.Kills)]
    [InlineData(MetricType.Venenatis, DeltaType.Kills)]
    [InlineData(MetricType.Vetion, DeltaType.Kills)]
    [InlineData(MetricType.Vorkath, DeltaType.Kills)]
    [InlineData(MetricType.Wintertodt, DeltaType.Kills)]
    [InlineData(MetricType.Zalcano, DeltaType.Kills)]
    [InlineData(MetricType.Zulrah, DeltaType.Kills)]
    [InlineData(MetricType.Combat, DeltaType.Score)]
    public void MetricTypeToCorrectDeltaType(MetricType metric, DeltaType expected) {
        var deltatype = _fixture.Mapper.Map<DeltaType>(metric);

        Assert.Equal(expected, deltatype);
    }
}
