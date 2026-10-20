using System.Linq;
using C7Engine;
using C7Engine.Lua;
using C7GameData;
using C7GameData.Save;
using EngineTests.Utils;
using Xunit;

namespace EngineTests;

public class GameSetupTests {
	[Theory]
	[InlineData(true, 2)]
	[InlineData(false, 4)]
	public void NoneOpponentsDoNotBecomePlayers(bool spreadPlayersEvenly, int expectedStartingLocations) {
		GameMode.Config config = new("civ3");
		SaveGame save = GameMode.Load(PathUtils.GameModesDir, config).GetSave();

		WorldSize worldSize = new() {
			width = 60,
			height = 60,
			numberOfCivs = 4,
			distanceBetweenCivs = 8,
			techRate = 240,
			optimalNumberOfCities = 20,
		};

		WorldCharacteristics wc = new(save) {
			landform = WorldCharacteristics.Landform.Pangaea,
			oceanCoverage = WorldCharacteristics.OceanCoverage.Percent_70,
			age = WorldCharacteristics.Age.Billion_4,
			climate = WorldCharacteristics.Climate.Normal,
			temperature = WorldCharacteristics.Temperature.Temperate,
			barbarianActivity = BarbarianActivity.Roaming,
			worldSize = worldSize,
			mapSeed = 42,
		};

		GameSetup gameSetup = new() {
			playerCivilization = save.Civilizations.First(c => !c.isBarbarian),
			difficulty = save.Difficulties.First(),
			worldCharacteristics = wc,
			opponents = [
				new SelectedOpponent { isNone = true },
				new SelectedOpponent { isRandom = true },
				new SelectedOpponent { isNone = true },
			],
			spreadPlayersEvenly = spreadPlayersEvenly,
			victoryConditions = new VictoryConditions()
		};

		gameSetup.Populate(save);

		// Barbarian + human + one rival; the two NONE slots add no players.
		Assert.Equal(3, save.Players.Count);
		string barbarianCiv = save.Civilizations.First(c => c.isBarbarian).name;
		Assert.Equal(2, save.Players.Count(p => p.civilization != barbarianCiv));

		// Starting locations are generated for the players in the game when
		// spread evenly, or for the map's maximum civ count otherwise.
		Assert.Equal(expectedStartingLocations, save.Map.startingLocations.Count);
	}
}
