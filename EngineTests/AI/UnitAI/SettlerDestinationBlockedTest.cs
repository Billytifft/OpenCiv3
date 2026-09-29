using C7Engine;
using C7GameData;
using C7GameData.AIData;
using C7GameData.Save;
using EngineTests.Utils;
using System.Collections.Generic;
using Xunit;

namespace EngineTests.AI.UnitAI;

/// <summary>
/// Tests for https://github.com/C7-Game/OpenCiv3/issues/213: an AI settler
/// whose destination becomes unreachable while en route (e.g. a rival unit
/// parks on it) must pick a new destination instead of re-evaluating the same
/// impossible task forever.
///
/// Candidate selection deliberately does NOT reject a tile just because a
/// foreign unit sits on it: the unit can move away before the settler arrives.
/// Only when the settler actually fails to reach its destination is that tile
/// excluded, and only for the current AI pass (see SettlerAI.FindNewDestination).
/// These tests exercise that retarget seam directly, since a full PlayTurn
/// cannot move units in the reduced test map.
/// </summary>
public sealed class SettlerDestinationBlockedTest : MapBase {
	// start = (50,50), destination = (52,50), one tile east of start
	private readonly Player aiPlayer;
	private readonly Player rival;
	private readonly Tile start;
	private readonly Tile destination;

	public SettlerDestinationBlockedTest() {
		InitilizeStartTile(MakeDesertTile(), new TileLocation(50, 50));
		start = startTile;

		destination = MakeHillTile();
		destination.XCoordinate = 52;
		destination.YCoordinate = 50;
		// A little extra production so the destination is clearly the best spot
		// when the retarget test starts (both it and the alternative are hills,
		// which gain the same hills bonus).
		destination.overlayTerrainType.baseShieldProduction = 2;
		AddNeighborsAndUpdateMap(start, destination, TileDirection.EAST);
		AddNeighborsAndUpdateMap(destination, start, TileDirection.WEST);

		aiPlayer = MakeAiPlayer();
		rival = MakeCivPlayer();

		foreach (Tile tile in new List<Tile> { start, destination }) {
			aiPlayer.tileKnowledge.knownTiles.Add(tile);
		}

		// The AI already has a home, so its settler goes looking for a spot.
		Tile homeTile = MakePlainsTile();
		homeTile.XCoordinate = 10;
		homeTile.YCoordinate = 10;
		aiPlayer.cities.Add(new City(homeTile, aiPlayer, "Home", ID.None("")));
	}

	private static Player MakeCivPlayer() {
		Player player = new Player();
		player.id = ID.FromString("rival-1");
		player.civilization = new Civilization();
		return player;
	}

	private static Player MakeAiPlayer() {
		Player player = new Player();
		player.id = ID.FromString("ai-1");
		player.civilization = new Civilization();
		player.government = new Government();
		player.rules = MakeTestRules();
		return player;
	}

	private MapUnit MakeSettlerOnStart() {
		MapUnit settler = MakeLandUnit(1);
		settler.unitType.name = "Settler";
		settler.owner = aiPlayer;
		settler.nationality = aiPlayer.civilization;
		settler.location = start;
		start.unitsOnTile.Add(settler);
		aiPlayer.units.Add(settler);
		return settler;
	}

	private void ParkRivalUnitOnDestination() {
		MapUnit blocker = MakeLandUnit(1);
		blocker.unitType.attack = 1;
		blocker.owner = rival;
		blocker.location = destination;
		destination.unitsOnTile.Add(blocker);
	}

	// Adds a second candidate hills tile at (46,50), three hops west of the
	// destination: destination -- start -- midWest -- alternative. It is outside
	// the two-tile radius around the destination that SettlerAlreadyMovingTowardsTile
	// blocks, so it stays available when the destination becomes unreachable.
	private Tile AddAlternativeCandidateWestOfStart() {
		Tile midWest = MakePlainsTile();
		AddNeighborsAndUpdateMap(start, midWest, TileDirection.WEST);      // midWest = (48,50)
		AddNeighborsAndUpdateMap(midWest, start, TileDirection.EAST);

		Tile alternativeWest = MakeHillTile();
		AddNeighborsAndUpdateMap(midWest, alternativeWest, TileDirection.WEST);  // alternativeWest = (46,50)
		AddNeighborsAndUpdateMap(alternativeWest, midWest, TileDirection.EAST);

		aiPlayer.tileKnowledge.knownTiles.Add(midWest);
		aiPlayer.tileKnowledge.knownTiles.Add(alternativeWest);
		return alternativeWest;
	}

	[Fact]
	private void ForeignOccupiedTileIsNotRejectedUntilItIsUnreachable() {
		EngineStorage.InitializeGameDataForTests(new C7GameData.GameData(3));

		// Sanity check: with the destination clear, it is the chosen spot.
		Assert.Equal(destination, SettlerLocationAI.FindSettlerLocation(start, aiPlayer));

		// A rival unit parks on it. It is STILL chosen: the unit may move away
		// before the settler arrives, so selection is not the place to reject it.
		ParkRivalUnitOnDestination();
		Assert.Equal(destination, SettlerLocationAI.FindSettlerLocation(start, aiPlayer));

		// But an explicit exclusion set (used after the settler actually fails
		// to reach a tile, issue #213) keeps that tile from being picked.
		HashSet<Tile> excluded = new HashSet<Tile> { destination };
		Tile chosen = SettlerLocationAI.FindSettlerLocation(start, aiPlayer, excluded);
		Assert.NotEqual(destination, chosen);
	}

	[Fact]
	private void SettlerRetargetsWhenDestinationBecomesUnreachable() {
		EngineStorage.InitializeGameDataForTests(new C7GameData.GameData(4));

		Tile alternative = AddAlternativeCandidateWestOfStart();

		MapUnit settler = MakeSettlerOnStart();
		settler.movementPoints.reset(settler.unitType.movement);

		// The settler heads off while the destination is still clear and it is
		// still the best spot (a hill).
		SettlerAIData data = SettlerAI.MakeAiData(settler, aiPlayer);
		Assert.Equal(SettlerAIData.SettlerGoal.BUILD_CITY, data.goal);
		Assert.Equal(destination, data.destination);

		// The destination gets blocked while en route.
		ParkRivalUnitOnDestination();

		SettlerAI settlerAi = new SettlerAI(data);
		settler.currentAI = settlerAi;
		C7GameData.UnitAI.MoveResult result = settlerAi.FindNewDestination(settler, aiPlayer);

		// The blocked tile is excluded for this pass but a fresh candidate is
		// picked instead, so the settler keeps building rather than looping.
		Assert.Equal(C7GameData.UnitAI.Result.InProgress, result.Result);
		Assert.Contains(destination, data.unreachableDestinations);
		Assert.Equal(alternative, data.destination);
		Assert.Equal(SettlerAIData.SettlerGoal.BUILD_CITY, data.goal);
		Assert.NotEmpty(data.pathToDestination.path);
		Assert.False(settler.movementPoints.canMove);
	}

	[Fact]
	private void SettlerGivesUpWhenNoReachableDestinationRemains() {
		EngineStorage.InitializeGameDataForTests(new C7GameData.GameData(5));

		MapUnit settler = MakeSettlerOnStart();
		settler.movementPoints.reset(settler.unitType.movement);

		SettlerAIData data = SettlerAI.MakeAiData(settler, aiPlayer);
		Assert.Equal(destination, data.destination);
		ParkRivalUnitOnDestination();

		// The only known tiles are the blocked destination and the settler's own
		// tile, which is too close to the blocked one to be selected. There is
		// nowhere left to go.
		SettlerAI settlerAi = new SettlerAI(data);
		settler.currentAI = settlerAi;
		C7GameData.UnitAI.MoveResult result = settlerAi.FindNewDestination(settler, aiPlayer);

		Assert.Equal(C7GameData.UnitAI.Result.InProgress, result.Result);
		Assert.Contains(destination, data.unreachableDestinations);
		Assert.Equal(SettlerAIData.SettlerGoal.JOIN_CITY, data.goal);
		Assert.False(settler.movementPoints.canMove);
	}

	[Fact]
	private void SettlerFailsGracefullyWhenDestinationBlockedMidJourney() {
		EngineStorage.InitializeGameDataForTests(new C7GameData.GameData(2));

		MapUnit settler = MakeSettlerOnStart();
		settler.movementPoints.reset(settler.unitType.movement);

		// The settler heads off while the destination is still clear.
		SettlerAIData data = SettlerAI.MakeAiData(settler, aiPlayer);
		Assert.Equal(SettlerAIData.SettlerGoal.BUILD_CITY, data.goal);
		Assert.Equal(destination, data.destination);
		Assert.NotEmpty(data.pathToDestination.path);

		// The destination gets blocked while en route.
		ParkRivalUnitOnDestination();

		SettlerAI settlerAi = new SettlerAI(data);
		C7GameData.UnitAI.MoveResult result = settlerAi.TryToMoveAlongPath(settler, ref data.pathToDestination);

		// The move fails gracefully (no exception) and the repath also cannot
		// reach the blocked destination. SettlerAI turns this Error into a
		// retarget via FindNewDestination.
		Assert.Equal(C7GameData.UnitAI.Result.Error, result.Result);
		Assert.Equal(Tile.NONE, data.pathToDestination.Next());
	}
}
