namespace C7GameData {
	public class Rules {
		public int MaximumResearchTime;
		public int MinimumResearchTime;
		public int ShieldValueInGold;
		public int ForestValueInShields;
		public int CitizenValueInShields;
		public int TurnPenaltyForEachHurrySacrifice;
		public int MaximumLevel1CitySize;
		public int MaximumLevel2CitySize;
		public int FoodNeededToGrowForLevel1Cities = 20;
		public int FoodNeededToGrowForLevel2Cities = 40;
		public int FoodNeededToGrowForLevel3Cities = 60;
		public float BuildingDiscountForCivTraits = .5f;
		public string StartUnitType1;
		public string StartUnitType2;
		public string ScoutUnitType;
		public int MaxRankOfWorkableTiles;
		public int MaxRankOfBarbarianCampTiles;
		public int DefaultDealDuration;
		public float TreasuryInterestRate = .05f;
		public int MaxInterest = 50;
		public int ShieldCostPerGold;
		public float ShieldRateForDisbanding; // per cent
		public bool AllowLesserUnitProduction; // for example, allow building a Spearman/Pikeman when we can build a Musketman (simultaneously)
		public int RadarTileVisibility; // how many tiles, a unit with the Radar ability, can see ahead

		// Whether scientific leaders (Great Scientists) can appear at all in this
		// game. Read from the BIQ/SAV GAME section when importing; standalone mode
		// defaults to enabled, matching vanilla Civ3.
		public bool AllowScientificLeaders = true;

		// The length of an Age of Science (and a Golden Age), in turns, taken from
		// Rule.GoldenAgeDuration when importing. Ages of Science and Golden Ages
		// stay in step because they share the value.
		public int GoldenAgeDuration;

		// Chance, as a fraction, that researching a technology first produces a
		// Scientific leader. No such field exists in the BIQ, so these are plain
		// configurable data seeded with the community-sourced Civ3 figures (3% base,
		// 5% for a civ with the Scientific trait) per Discussion #243.
		public float ScientificLeaderChance = .03f;
		public float ScientificTraitLeaderChance = .05f;

		// The maximum number of leaders a civilization may hold of each type at
		// once. The two leader types gate independently (D5): a Scientific leader
		// does not block a Military one, and vice versa.
		public int MaximumLeadersPerType = 1;
	}
}
