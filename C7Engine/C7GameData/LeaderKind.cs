namespace C7GameData {
	/// <summary>
	/// Distinguishes Great Leader units from each other and from ordinary units.
	/// A Scientific leader resolves its art from the "SCI" variation rather than
	/// the era-specific variation used by Military leaders and everyone else.
	/// </summary>
	public enum LeaderKind {
		None,
		Military,
		Scientific,
	}
}
