namespace C7GameData {

	// Values match the raw byte Civ3 stores per unit, so an unrecognized value
	// still round trips.
	public enum LeaderKind : byte {
		None = 0,
		Military = 1,
		Scientific = 2,
	}
}
