using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WiseOldManConnector.Models.WiseOldMan.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum GroupRole {
    [EnumMember(Value = "member")]
    Member = 0,

    [EnumMember(Value = "leader")]
    Leader = 1,

    [EnumMember(Value = "jagex moderator")]
    JagexModerator,

    [EnumMember(Value = @"adamant")]
    Adamant,

    [EnumMember(Value = @"adept")]
    Adept,

    [EnumMember(Value = @"administrator")]
    Administrator,

    [EnumMember(Value = @"admiral")]
    Admiral,

    [EnumMember(Value = @"adventurer")]
    Adventurer,

    [EnumMember(Value = @"air")]
    Air,

    [EnumMember(Value = @"anchor")]
    Anchor,

    [EnumMember(Value = @"apothecary")]
    Apothecary,

    [EnumMember(Value = @"archer")]
    Archer,

    [EnumMember(Value = @"armadylean")]
    Armadylean,

    [EnumMember(Value = @"artillery")]
    Artillery,

    [EnumMember(Value = @"artisan")]
    Artisan,

    [EnumMember(Value = @"asgarnian")]
    Asgarnian,

    [EnumMember(Value = @"assassin")]
    Assassin,

    [EnumMember(Value = @"assistant")]
    Assistant,

    [EnumMember(Value = @"astral")]
    Astral,

    [EnumMember(Value = @"athlete")]
    Athlete,

    [EnumMember(Value = @"attacker")]
    Attacker,

    [EnumMember(Value = @"bandit")]
    Bandit,

    [EnumMember(Value = @"bandosian")]
    Bandosian,

    [EnumMember(Value = @"barbarian")]
    Barbarian,

    [EnumMember(Value = @"battlemage")]
    Battlemage,

    [EnumMember(Value = @"beast")]
    Beast,

    [EnumMember(Value = @"berserker")]
    Berserker,

    [EnumMember(Value = @"blisterwood")]
    Blisterwood,

    [EnumMember(Value = @"blood")]
    Blood,

    [EnumMember(Value = @"blue")]
    Blue,

    [EnumMember(Value = @"bob")]
    Bob,

    [EnumMember(Value = @"body")]
    Body,

    [EnumMember(Value = @"brassican")]
    Brassican,

    [EnumMember(Value = @"brawler")]
    Brawler,

    [EnumMember(Value = @"brigadier")]
    Brigadier,

    [EnumMember(Value = @"brigand")]
    Brigand,

    [EnumMember(Value = @"bronze")]
    Bronze,

    [EnumMember(Value = @"bruiser")]
    Bruiser,

    [EnumMember(Value = @"bulwark")]
    Bulwark,

    [EnumMember(Value = @"burglar")]
    Burglar,

    [EnumMember(Value = @"burnt")]
    Burnt,

    [EnumMember(Value = @"cadet")]
    Cadet,

    [EnumMember(Value = @"captain")]
    Captain,

    [EnumMember(Value = @"carry")]
    Carry,

    [EnumMember(Value = @"champion")]
    Champion,

    [EnumMember(Value = @"chaos")]
    Chaos,

    [EnumMember(Value = @"cleric")]
    Cleric,

    [EnumMember(Value = @"collector")]
    Collector,

    [EnumMember(Value = @"colonel")]
    Colonel,

    [EnumMember(Value = @"commander")]
    Commander,

    [EnumMember(Value = @"competitor")]
    Competitor,

    [EnumMember(Value = @"completionist")]
    Completionist,

    [EnumMember(Value = @"constructor")]
    Constructor,

    [EnumMember(Value = @"cook")]
    Cook,

    [EnumMember(Value = @"coordinator")]
    Coordinator,

    [EnumMember(Value = @"corporal")]
    Corporal,

    [EnumMember(Value = @"cosmic")]
    Cosmic,

    [EnumMember(Value = @"councillor")]
    Councillor,

    [EnumMember(Value = @"crafter")]
    Crafter,

    [EnumMember(Value = @"crew")]
    Crew,

    [EnumMember(Value = @"crusader")]
    Crusader,

    [EnumMember(Value = @"cutpurse")]
    Cutpurse,

    [EnumMember(Value = @"death")]
    Death,

    [EnumMember(Value = @"defender")]
    Defender,

    [EnumMember(Value = @"defiler")]
    Defiler,

    [EnumMember(Value = @"deputy owner")]
    DeputyOwner,

    [EnumMember(Value = @"destroyer")]
    Destroyer,

    [EnumMember(Value = @"diamond")]
    Diamond,

    [EnumMember(Value = @"diseased")]
    Diseased,

    [EnumMember(Value = @"doctor")]
    Doctor,

    [EnumMember(Value = @"dogsbody")]
    Dogsbody,

    [EnumMember(Value = @"dragon")]
    Dragon,

    [EnumMember(Value = @"dragonstone")]
    Dragonstone,

    [EnumMember(Value = @"druid")]
    Druid,

    [EnumMember(Value = @"duellist")]
    Duellist,

    [EnumMember(Value = @"earth")]
    Earth,

    [EnumMember(Value = @"elite")]
    Elite,

    [EnumMember(Value = @"emerald")]
    Emerald,

    [EnumMember(Value = @"enforcer")]
    Enforcer,

    [EnumMember(Value = @"epic")]
    Epic,

    [EnumMember(Value = @"executive")]
    Executive,

    [EnumMember(Value = @"expert")]
    Expert,

    [EnumMember(Value = @"explorer")]
    Explorer,

    [EnumMember(Value = @"farmer")]
    Farmer,

    [EnumMember(Value = @"feeder")]
    Feeder,

    [EnumMember(Value = @"fighter")]
    Fighter,

    [EnumMember(Value = @"fire")]
    Fire,

    [EnumMember(Value = @"firemaker")]
    Firemaker,

    [EnumMember(Value = @"firestarter")]
    Firestarter,

    [EnumMember(Value = @"fisher")]
    Fisher,

    [EnumMember(Value = @"fletcher")]
    Fletcher,

    [EnumMember(Value = @"forager")]
    Forager,

    [EnumMember(Value = @"fremennik")]
    Fremennik,

    [EnumMember(Value = @"gamer")]
    Gamer,

    [EnumMember(Value = @"gatherer")]
    Gatherer,

    [EnumMember(Value = @"general")]
    General,

    [EnumMember(Value = @"gnome child")]
    GnomeChild,

    [EnumMember(Value = @"gnome elder")]
    GnomeElder,

    [EnumMember(Value = @"goblin")]
    Goblin,

    [EnumMember(Value = @"gold")]
    Gold,

    [EnumMember(Value = @"goon")]
    Goon,

    [EnumMember(Value = @"green")]
    Green,

    [EnumMember(Value = @"grey")]
    Grey,

    [EnumMember(Value = @"guardian")]
    Guardian,

    [EnumMember(Value = @"guthixian")]
    Guthixian,

    [EnumMember(Value = @"harpoon")]
    Harpoon,

    [EnumMember(Value = @"healer")]
    Healer,

    [EnumMember(Value = @"hellcat")]
    Hellcat,

    [EnumMember(Value = @"helper")]
    Helper,

    [EnumMember(Value = @"herbologist")]
    Herbologist,

    [EnumMember(Value = @"hero")]
    Hero,

    [EnumMember(Value = @"holy")]
    Holy,

    [EnumMember(Value = @"hoarder")]
    Hoarder,

    [EnumMember(Value = @"hunter")]
    Hunter,

    [EnumMember(Value = @"ignitor")]
    Ignitor,

    [EnumMember(Value = @"illusionist")]
    Illusionist,

    [EnumMember(Value = @"imp")]
    Imp,

    [EnumMember(Value = @"infantry")]
    Infantry,

    [EnumMember(Value = @"inquisitor")]
    Inquisitor,

    [EnumMember(Value = @"iron")]
    Iron,

    [EnumMember(Value = @"jade")]
    Jade,

    [EnumMember(Value = @"justiciar")]
    Justiciar,

    [EnumMember(Value = @"kandarin")]
    Kandarin,

    [EnumMember(Value = @"karamjan")]
    Karamjan,

    [EnumMember(Value = @"kharidian")]
    Kharidian,

    [EnumMember(Value = @"kitten")]
    Kitten,

    [EnumMember(Value = @"knight")]
    Knight,

    [EnumMember(Value = @"labourer")]
    Labourer,

    [EnumMember(Value = @"law")]
    Law,

    [EnumMember(Value = @"learner")]
    Learner,

    [EnumMember(Value = @"legacy")]
    Legacy,

    [EnumMember(Value = @"legend")]
    Legend,

    [EnumMember(Value = @"legionnaire")]
    Legionnaire,

    [EnumMember(Value = @"lieutenant")]
    Lieutenant,

    [EnumMember(Value = @"looter")]
    Looter,

    [EnumMember(Value = @"lumberjack")]
    Lumberjack,

    [EnumMember(Value = @"magic")]
    Magic,

    [EnumMember(Value = @"magician")]
    Magician,

    [EnumMember(Value = @"major")]
    Major,

    [EnumMember(Value = @"maple")]
    Maple,

    [EnumMember(Value = @"marshal")]
    Marshal,

    [EnumMember(Value = @"master")]
    Master,

    [EnumMember(Value = @"maxed")]
    Maxed,

    [EnumMember(Value = @"mediator")]
    Mediator,

    [EnumMember(Value = @"medic")]
    Medic,

    [EnumMember(Value = @"mentor")]
    Mentor,

    [EnumMember(Value = @"merchant")]
    Merchant,

    [EnumMember(Value = @"mind")]
    Mind,

    [EnumMember(Value = @"miner")]
    Miner,

    [EnumMember(Value = @"minion")]
    Minion,

    [EnumMember(Value = @"misthalinian")]
    Misthalinian,

    [EnumMember(Value = @"mithril")]
    Mithril,

    [EnumMember(Value = @"moderator")]
    Moderator,

    [EnumMember(Value = @"monarch")]
    Monarch,

    [EnumMember(Value = @"morytanian")]
    Morytanian,

    [EnumMember(Value = @"mystic")]
    Mystic,

    [EnumMember(Value = @"myth")]
    Myth,

    [EnumMember(Value = @"natural")]
    Natural,

    [EnumMember(Value = @"nature")]
    Nature,

    [EnumMember(Value = @"necromancer")]
    Necromancer,

    [EnumMember(Value = @"ninja")]
    Ninja,

    [EnumMember(Value = @"noble")]
    Noble,

    [EnumMember(Value = @"novice")]
    Novice,

    [EnumMember(Value = @"nurse")]
    Nurse,

    [EnumMember(Value = @"oak")]
    Oak,

    [EnumMember(Value = @"officer")]
    Officer,

    [EnumMember(Value = @"onyx")]
    Onyx,

    [EnumMember(Value = @"opal")]
    Opal,

    [EnumMember(Value = @"oracle")]
    Oracle,

    [EnumMember(Value = @"orange")]
    Orange,

    [EnumMember(Value = @"owner")]
    Owner,

    [EnumMember(Value = @"page")]
    Page,

    [EnumMember(Value = @"paladin")]
    Paladin,

    [EnumMember(Value = @"pawn")]
    Pawn,

    [EnumMember(Value = @"pilgrim")]
    Pilgrim,

    [EnumMember(Value = @"pine")]
    Pine,

    [EnumMember(Value = @"pink")]
    Pink,

    [EnumMember(Value = @"prefect")]
    Prefect,

    [EnumMember(Value = @"priest")]
    Priest,

    [EnumMember(Value = @"private")]
    Private,

    [EnumMember(Value = @"prodigy")]
    Prodigy,

    [EnumMember(Value = @"proselyte")]
    Proselyte,

    [EnumMember(Value = @"prospector")]
    Prospector,

    [EnumMember(Value = @"protector")]
    Protector,

    [EnumMember(Value = @"pure")]
    Pure,

    [EnumMember(Value = @"purple")]
    Purple,

    [EnumMember(Value = @"pyromancer")]
    Pyromancer,

    [EnumMember(Value = @"quester")]
    Quester,

    [EnumMember(Value = @"racer")]
    Racer,

    [EnumMember(Value = @"raider")]
    Raider,

    [EnumMember(Value = @"ranger")]
    Ranger,

    [EnumMember(Value = @"record-chaser")]
    RecordChaser,

    [EnumMember(Value = @"recruit")]
    Recruit,

    [EnumMember(Value = @"recruiter")]
    Recruiter,

    [EnumMember(Value = @"red topaz")]
    RedTopaz,

    [EnumMember(Value = @"red")]
    Red,

    [EnumMember(Value = @"rogue")]
    Rogue,

    [EnumMember(Value = @"ruby")]
    Ruby,

    [EnumMember(Value = @"rune")]
    Rune,

    [EnumMember(Value = @"runecrafter")]
    Runecrafter,

    [EnumMember(Value = @"sage")]
    Sage,

    [EnumMember(Value = @"sapphire")]
    Sapphire,

    [EnumMember(Value = @"saradominist")]
    Saradominist,

    [EnumMember(Value = @"saviour")]
    Saviour,

    [EnumMember(Value = @"scavenger")]
    Scavenger,

    [EnumMember(Value = @"scholar")]
    Scholar,

    [EnumMember(Value = @"scourge")]
    Scourge,

    [EnumMember(Value = @"scout")]
    Scout,

    [EnumMember(Value = @"scribe")]
    Scribe,

    [EnumMember(Value = @"seer")]
    Seer,

    [EnumMember(Value = @"senator")]
    Senator,

    [EnumMember(Value = @"sentry")]
    Sentry,

    [EnumMember(Value = @"serenist")]
    Serenist,

    [EnumMember(Value = @"sergeant")]
    Sergeant,

    [EnumMember(Value = @"shaman")]
    Shaman,

    [EnumMember(Value = @"sheriff")]
    Sheriff,

    [EnumMember(Value = @"short green guy")]
    ShortGreenGuy,

    [EnumMember(Value = @"skiller")]
    Skiller,

    [EnumMember(Value = @"skulled")]
    Skulled,

    [EnumMember(Value = @"slayer")]
    Slayer,

    [EnumMember(Value = @"smiter")]
    Smiter,

    [EnumMember(Value = @"smith")]
    Smith,

    [EnumMember(Value = @"smuggler")]
    Smuggler,

    [EnumMember(Value = @"sniper")]
    Sniper,

    [EnumMember(Value = @"soul")]
    Soul,

    [EnumMember(Value = @"specialist")]
    Specialist,

    [EnumMember(Value = @"speed-runner")]
    SpeedRunner,

    [EnumMember(Value = @"spellcaster")]
    Spellcaster,

    [EnumMember(Value = @"squire")]
    Squire,

    [EnumMember(Value = @"staff")]
    Staff,

    [EnumMember(Value = @"steel")]
    Steel,

    [EnumMember(Value = @"strider")]
    Strider,

    [EnumMember(Value = @"striker")]
    Striker,

    [EnumMember(Value = @"summoner")]
    Summoner,

    [EnumMember(Value = @"superior")]
    Superior,

    [EnumMember(Value = @"supervisor")]
    Supervisor,

    [EnumMember(Value = @"teacher")]
    Teacher,

    [EnumMember(Value = @"templar")]
    Templar,

    [EnumMember(Value = @"therapist")]
    Therapist,

    [EnumMember(Value = @"thief")]
    Thief,

    [EnumMember(Value = @"tirannian")]
    Tirannian,

    [EnumMember(Value = @"trialist")]
    Trialist,

    [EnumMember(Value = @"trickster")]
    Trickster,

    [EnumMember(Value = @"tzkal")]
    Tzkal,

    [EnumMember(Value = @"tztok")]
    Tztok,

    [EnumMember(Value = @"unholy")]
    Unholy,

    [EnumMember(Value = @"vagrant")]
    Vagrant,

    [EnumMember(Value = @"vanguard")]
    Vanguard,

    [EnumMember(Value = @"walker")]
    Walker,

    [EnumMember(Value = @"wanderer")]
    Wanderer,

    [EnumMember(Value = @"warden")]
    Warden,

    [EnumMember(Value = @"warlock")]
    Warlock,

    [EnumMember(Value = @"warrior")]
    Warrior,

    [EnumMember(Value = @"water")]
    Water,

    [EnumMember(Value = @"wild")]
    Wild,

    [EnumMember(Value = @"willow")]
    Willow,

    [EnumMember(Value = @"wily")]
    Wily,

    [EnumMember(Value = @"wintumber")]
    Wintumber,

    [EnumMember(Value = @"witch")]
    Witch,

    [EnumMember(Value = @"wizard")]
    Wizard,

    [EnumMember(Value = @"worker")]
    Worker,

    [EnumMember(Value = @"wrath")]
    Wrath,

    [EnumMember(Value = @"xerician")]
    Xerician,

    [EnumMember(Value = @"yellow")]
    Yellow,

    [EnumMember(Value = @"yew")]
    Yew,

    [EnumMember(Value = @"zamorakian")]
    Zamorakian,

    [EnumMember(Value = @"zarosian")]
    Zarosian,

    [EnumMember(Value = @"zealot")]
    Zealot,

    [EnumMember(Value = @"zenyte")]
    Zenyte
}
