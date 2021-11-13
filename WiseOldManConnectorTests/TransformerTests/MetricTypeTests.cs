using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.TransformerTests; 

public class MetricTypeTests : IClassFixture<MapperFixture> {
    private readonly MapperFixture _fixture;

    public MetricTypeTests(MapperFixture fixture) {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("overall", MetricType.Overall)]
    [InlineData("attack", MetricType.Attack)]
    [InlineData("defence", MetricType.Defence)]
    [InlineData("strength", MetricType.Strength)]
    [InlineData("hitpoints", MetricType.Hitpoints)]
    [InlineData("ranged", MetricType.Ranged)]
    [InlineData("prayer", MetricType.Prayer)]
    [InlineData("magic", MetricType.Magic)]
    [InlineData("cooking", MetricType.Cooking)]
    [InlineData("woodcutting", MetricType.Woodcutting)]
    [InlineData("fletching", MetricType.Fletching)]
    [InlineData("fishing", MetricType.Fishing)]
    [InlineData("firemaking", MetricType.Firemaking)]
    [InlineData("crafting", MetricType.Crafting)]
    [InlineData("smithing", MetricType.Smithing)]
    [InlineData("mining", MetricType.Mining)]
    [InlineData("herblore", MetricType.Herblore)]
    [InlineData("agility", MetricType.Agility)]
    [InlineData("thieving", MetricType.Thieving)]
    [InlineData("slayer", MetricType.Slayer)]
    [InlineData("farming", MetricType.Farming)]
    [InlineData("runecrafting", MetricType.Runecrafting)]
    [InlineData("hunter", MetricType.Hunter)]
    [InlineData("Construction", MetricType.Construction)]
    [InlineData("league_points", MetricType.LeaguePoints)]
    [InlineData("bounty_hunter_hunter", MetricType.BountyHunterHunter)]
    [InlineData("bounty_hunter_rogue", MetricType.BountyHunterRogue)]
    [InlineData("clue_scrolls_all", MetricType.ClueScrollsAll)]
    [InlineData("clue_scrolls_beginner", MetricType.ClueScrollsBeginner)]
    [InlineData("clue_scrolls_easy", MetricType.ClueScrollsEasy)]
    [InlineData("clue_scrolls_medium", MetricType.ClueScrollsMedium)]
    [InlineData("clue_scrolls_hard", MetricType.ClueScrollsHard)]
    [InlineData("clue_scrolls_elite", MetricType.ClueScrollsElite)]
    [InlineData("clue_scrolls_master", MetricType.ClueScrollsMaster)]
    [InlineData("last_man_standing", MetricType.LastManStanding)]
    [InlineData("abyssal_sire", MetricType.AbyssalSire)]
    [InlineData("alchemical_hydra", MetricType.AlchemicalHydra)]
    [InlineData("barrows_chests", MetricType.BarrowsChests)]
    [InlineData("bryophyta", MetricType.Bryophyta)]
    [InlineData("callisto", MetricType.Callisto)]
    [InlineData("cerberus", MetricType.Cerberus)]
    [InlineData("chambers_of_xeric", MetricType.ChambersOfXeric)]
    [InlineData("chambers_of_xeric_challenge_mode", MetricType.ChambersOfXericChallengeMode)]
    [InlineData("chaos_elemental", MetricType.ChaosElemental)]
    [InlineData("chaos_fanatic", MetricType.ChaosFanatic)]
    [InlineData("commander_zilyana", MetricType.CommanderZilyana)]
    [InlineData("corporeal_beast", MetricType.CorporealBeast)]
    [InlineData("crazy_archaeologist", MetricType.CrazyArchaeologist)]
    [InlineData("dagannoth_prime", MetricType.DagannothPrime)]
    [InlineData("dagannoth_rex", MetricType.DagannothRex)]
    [InlineData("dagannoth_supreme", MetricType.DagannothSupreme)]
    [InlineData("deranged_archaeologist", MetricType.DerangedArchaeologist)]
    [InlineData("general_graardor", MetricType.GeneralGraardor)]
    [InlineData("giant_mole", MetricType.GiantMole)]
    [InlineData("grotesque_guardians", MetricType.GrotesqueGuardians)]
    [InlineData("hespori", MetricType.Hespori)]
    [InlineData("kalphite_queen", MetricType.KalphiteQueen)]
    [InlineData("king_black_dragon", MetricType.KingBlackDragon)]
    [InlineData("kraken", MetricType.Kraken)]
    [InlineData("kreearra", MetricType.Kreearra)]
    [InlineData("kril_tsutsaroth", MetricType.KrilTsutsaroth)]
    [InlineData("mimic", MetricType.Mimic)]
    [InlineData("nightmare", MetricType.Nightmare)]
    [InlineData("obor", MetricType.Obor)]
    [InlineData("sarachnis", MetricType.Sarachnis)]
    [InlineData("scorpia", MetricType.Scorpia)]
    [InlineData("skotizo", MetricType.Skotizo)]
    [InlineData("the_gauntlet", MetricType.TheGauntlet)]
    [InlineData("the_corrupted_gauntlet", MetricType.TheCorruptedGauntlet)]
    [InlineData("theatre_of_blood", MetricType.TheatreOfBlood)]
    [InlineData("thermonuclear_smoke_devil", MetricType.ThermonuclearSmokeDevil)]
    [InlineData("tzkal_zuk", MetricType.TzkalZuk)]
    [InlineData("tztok_jad", MetricType.TztokJad)]
    [InlineData("venenatis", MetricType.Venenatis)]
    [InlineData("vetion", MetricType.Vetion)]
    [InlineData("vorkath", MetricType.Vorkath)]
    [InlineData("wintertodt", MetricType.Wintertodt)]
    [InlineData("zalcano", MetricType.Zalcano)]
    [InlineData("zulrah", MetricType.Zulrah)]
    [InlineData("combat", MetricType.Combat)]
    public void CorrectStringsToMetricTypesGetsConverted(string metric, MetricType expected) {
        var metricType = _fixture.Mapper.Map<MetricType>(metric);
        Assert.Equal(expected, metricType);
    }

    [Theory]
    [InlineData("oveRall", MetricType.Overall)]
    [InlineData("attaCk", MetricType.Attack)]
    [InlineData("deFEnce", MetricType.Defence)]
    [InlineData("strEngth", MetricType.Strength)]
    [InlineData("Hitpoints", MetricType.Hitpoints)]
    [InlineData("Ranged", MetricType.Ranged)]
    [InlineData("pRayer", MetricType.Prayer)]
    [InlineData("mAgic", MetricType.Magic)]
    [InlineData("coOking", MetricType.Cooking)]
    [InlineData("woOdcutting", MetricType.Woodcutting)]
    [InlineData("fleTching", MetricType.Fletching)]
    [InlineData("fisHing", MetricType.Fishing)]
    [InlineData("fireMaking", MetricType.Firemaking)]
    [InlineData("crafTing", MetricType.Crafting)]
    [InlineData("smithIng", MetricType.Smithing)]
    [InlineData("miniNg", MetricType.Mining)]
    [InlineData("herblOre", MetricType.Herblore)]
    [InlineData("agiliTy", MetricType.Agility)]
    [InlineData("thievinG", MetricType.Thieving)]
    [InlineData("slayEr", MetricType.Slayer)]
    [InlineData("farming", MetricType.Farming)]
    [InlineData("runeCrafting", MetricType.Runecrafting)]
    [InlineData("bounty_hUnter_rogue", MetricType.BountyHunterRogue)]
    [InlineData("clue_scrollS_all", MetricType.ClueScrollsAll)]
    [InlineData("cAllisto", MetricType.Callisto)]
    [InlineData("cerBerus", MetricType.Cerberus)]
    [InlineData("chamberS_of_xeric", MetricType.ChambersOfXeric)]
    [InlineData("chambers_Of_XERIC_challenge_mode", MetricType.ChambersOfXericChallengeMode)]
    [InlineData("VORKATH", MetricType.Vorkath)]
    [InlineData("COMBAT", MetricType.Combat)]
    [InlineData("Tempoross", MetricType.Tempoross)]
    public void CorrectStringsWithCapitalsToMetricTypesGetsConverted(string metric, MetricType expected) {
        var metricType = _fixture.Mapper.Map<MetricType>(metric);

        Assert.Equal(expected, metricType);
    }

    [Theory]
    [ClassData(typeof(MetricTypeTestData))]
    public void StringsFromEnumWithCapitalsToMetricTypesGetsConverted(MetricType expected) {
        var metricType = _fixture.Mapper.Map<MetricType>(expected.ToString());
        Assert.Equal(expected, metricType);
    }

    [Theory]
    [ClassData(typeof(MetricTypeTestData))]
    public void HasCategory(MetricType metric) {
        var category = metric.Category();
        if (metric == MetricType.Combat) {
            Assert.Equal(MetricTypeCategory.Others, category);
        } else {
            Assert.NotEqual(MetricTypeCategory.Others, category);
        }
    }
}