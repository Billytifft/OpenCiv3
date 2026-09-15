using C7GameData;
using Xunit;

namespace EngineTests.GameData;

public class PlayerCommerceBreakdownTest {
	private static PlayerCommerceBreakdown MakeBreakdown() {
		return new PlayerCommerceBreakdown {
			corrupted = 3,
			taxes = 4,
			taxmenTaxes = 2,
			beakers = 5,
			happiness = 6,
			fromOtherCivs = 1,
			toOtherCivs = 2,
			interest = 3,
			maintenance = 4,
			unitSupport = 5,
			wealthProduction = 6,
		};
	}

	[Fact]
	public void InflowsIncludesOnlyTreasuryGoldIncome() {
		PlayerCommerceBreakdown breakdown = MakeBreakdown();

		// corrupted, beakers and happiness are allocations of city commerce,
		// not income, so they must not appear in the treasury inflow number.
		Assert.Equal(4 + 2 + 1 + 3 + 6, breakdown.Inflows());
	}

	[Fact]
	public void OutflowsIncludesOnlyTreasuryGoldExpenses() {
		PlayerCommerceBreakdown breakdown = MakeBreakdown();

		// Science, entertainment and corruption are not treasury expenses.
		Assert.Equal(2 + 4 + 5, breakdown.Outflows());
	}

	[Fact]
	public void NetflowsIsInflowsMinusOutflows() {
		PlayerCommerceBreakdown breakdown = MakeBreakdown();

		Assert.Equal(breakdown.Inflows() - breakdown.Outflows(), breakdown.Netflows());
		Assert.Equal(5, breakdown.Netflows());
	}

	[Fact]
	public void CommerceTotalIncludesAllCityCommerce() {
		PlayerCommerceBreakdown breakdown = MakeBreakdown();

		Assert.Equal(3 + 4 + 5 + 6 + 6, breakdown.CommerceTotal());
	}
}
