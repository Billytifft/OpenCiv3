using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using C7GameData;
using C7GameData.Save;
using EngineTests.Utils;
using QueryCiv3;
using Xunit;

namespace EngineTests.GameData;

public class LeaderDataTest : IClassFixture<SaveGameFixture> {
	SaveGameFixture fixture;

	public LeaderDataTest(SaveGameFixture fixture) {
		this.fixture = fixture;
	}

	[Fact]
	public void StandaloneRulesExposeScientificLeaderDefaults() {
		Assert.Equal(20, fixture.standaloneSaveGame.Rules.GoldenAgeDuration);
		Assert.True(fixture.standaloneSaveGame.Rules.AllowScientificLeaders);
		Assert.Equal(.03f, fixture.standaloneSaveGame.Rules.ScientificLeaderChance);
		Assert.Equal(.05f, fixture.standaloneSaveGame.Rules.ScientificTraitLeaderChance);
		Assert.Equal(1, fixture.standaloneSaveGame.Rules.MaximumLeadersPerType);
	}

	[Fact]
	public void BaseRulesetDefinesScientificLeaderRules() {
		using JsonDocument ruleset = JsonUtils.LoadBaseRuleset();
		JsonElement rules = ruleset.RootElement.GetProperty("rules");

		Assert.True(rules.GetProperty("allowScientificLeaders").GetBoolean());
		Assert.Equal(20, rules.GetProperty("goldenAgeDuration").GetInt32());
		Assert.Equal(0.03, rules.GetProperty("scientificLeaderChance").GetDouble());
		Assert.Equal(0.05, rules.GetProperty("scientificTraitLeaderChance").GetDouble());
		Assert.Equal(1, rules.GetProperty("maximumLeadersPerType").GetInt32());
	}

	[Fact]
	public void RulesDefaultToCommunitySeededLeaderValues() {
		Rules rules = new Rules();

		Assert.True(rules.AllowScientificLeaders);
		Assert.Equal(.03f, rules.ScientificLeaderChance);
		Assert.Equal(.05f, rules.ScientificTraitLeaderChance);
		Assert.Equal(1, rules.MaximumLeadersPerType);
	}

	[Fact]
	public void ScientificLeaderResolvesSciArtRegardlessOfEra() {
		MapUnit unit = MakeLeaderArtUnit(LeaderKind.Scientific, "ERAS_Middle_Ages");

		Assert.Equal("SciLeader", unit.GetArtName());
	}

	[Fact]
	public void MilitaryLeaderUsesEraArt() {
		MapUnit unit = MakeLeaderArtUnit(LeaderKind.Military, "ERAS_Middle_Ages");

		Assert.Equal("Leader Middle Ages", unit.GetArtName());
	}

	[Fact]
	public void NonLeaderUnitStillUsesEraArt() {
		MapUnit unit = MakeLeaderArtUnit(LeaderKind.None, "ERAS_Middle_Ages");

		Assert.Equal("Leader Middle Ages", unit.GetArtName());
	}

	[Fact]
	public void ScientificLeaderFallsBackToDefaultArtWhenSciVariationMissing() {
		MapUnit unit = MakeLeaderArtUnit(LeaderKind.Scientific, "ERAS_Ancient_Times");
		unit.unitType.art.mainArt.variations.Remove("SCI");

		Assert.Equal("Leader Ancient Times", unit.GetArtName());
	}

	[Fact]
	public void SaveUnitRoundTripsLeaderKindThroughJson() {
		SaveGame save = new SaveGame();
		SaveUnit leader = new SaveUnit { name = "Bernoulli", prototype = "Leader", leaderKind = LeaderKind.Scientific };
		SaveUnit ordinary = new SaveUnit { name = "Warrior", prototype = "Warrior" };
		save.Units.Add(leader);
		save.Units.Add(ordinary);

		SaveGame clone = save.Clone();

		Assert.Equal(LeaderKind.Scientific, clone.Units.First(u => u.name == "Bernoulli").leaderKind);
		Assert.Null(clone.Units.First(u => u.name == "Warrior").leaderKind);
	}

	[Fact]
	public void LeaderKindSurvivesMapUnitToSaveUnitRoundTrip() {
		C7GameData.GameData gd = SaveGameFixture.HydrateSaveGame(fixture.saveGame);
		MapUnit leader = gd.mapUnits.First();
		leader.name = "Bernoulli";
		leader.leaderKind = LeaderKind.Scientific;

		SaveUnit saved = new SaveUnit(leader);
		Assert.Equal(LeaderKind.Scientific, saved.leaderKind);

		MapUnit restored = saved.ToMapUnit(gd.unitPrototypes, gd.experienceLevels, gd.players, gd.Terraforms, gd.map);
		Assert.Equal(LeaderKind.Scientific, restored.leaderKind);

		MapUnit ordinaryUnit = gd.mapUnits.First(u => u.id != leader.id);
		Assert.Null(new SaveUnit(ordinaryUnit).leaderKind);
	}

	[SkippableFact]
	public void SavImportReadsScientificLeaderData() {
		Skip.If(Civ3TestData.ShouldSkipCiv3DependentTests(), "No Civ3 install found.");

		string savPath = Path.Combine(PathUtils.getDataPath("saves"), "12345.SAV");
		Skip.If(!File.Exists(savPath), "Sample save 12345.SAV not present; run LoadSampleSaves to fetch it.");

		// The BIQ and sample SAV are Blast-compressed; Util.ReadFile handles the decompression.
		SavData savData = new SavData(Util.ReadFile(savPath), Util.ReadFile(PathUtils.defaultBicPath));
		SaveGame save = ImportCiv3.ImportSav(savPath, PathUtils.defaultBicPath, (_) => PathUtils.defaultPediaIconsPath);

		// The SAV's own GAME section is authoritative for the toggle, and the
		// campaign's RULE section provides the Age of Science duration.
		Assert.Equal(savData.Game.AllowScientificLeaders, save.Rules.AllowScientificLeaders);
		Assert.Equal(savData.Bic.Rule[0].GoldenAgeDuration, save.Rules.GoldenAgeDuration);

		// The pool comes from the campaign BIQ's RACE section, with one civ per
		// race in the same order, sized by RACE.NumberOfScientificLeaders.
		Assert.Equal(savData.Bic.Race.Length, save.Civilizations.Count);
		int totalScientificLeaders = 0;
		for (int i = 0; i < savData.Bic.Race.Length; ++i) {
			Assert.Equal(savData.Bic.Race[i].NumberOfScientificLeaders, save.Civilizations[i].scientificLeaderNames.Count);
			totalScientificLeaders += savData.Bic.Race[i].NumberOfScientificLeaders;
		}
		Assert.True(totalScientificLeaders > 0);
		Assert.Contains(save.Civilizations, civ => civ.scientificLeaderNames.Any(name => name.Length > 0));
	}

	private static MapUnit MakeLeaderArtUnit(LeaderKind leaderKind, string era) {
		Player player = new Player() {
			civilization = new Civilization("Rome"),
			eraCivilopediaName = era,
		};
		return new(ID.None("leader")) {
			name = "Great Scientist",
			unitType = new UnitPrototype {
				name = "Leader",
				art = new Art {
					mainArt = new MainArt {
						defaultName = "Leader Ancient Times",
						variations = new Dictionary<string, string> {
							["ERAS_Ancient_Times"] = "Leader Ancient Times",
							["ERAS_Middle_Ages"] = "Leader Middle Ages",
							["SCI"] = "SciLeader",
						},
					},
				},
			},
			owner = player,
			leaderKind = leaderKind,
		};
	}
}
