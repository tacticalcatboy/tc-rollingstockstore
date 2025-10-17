using System.Collections.Generic;

namespace RollingStockStore.UMM;

public static class CarPriceConfig
{
	/// <summary>
	/// Base prices for gondola cars based on game economy analysis.
	/// Prices are comparable to other freight cars (hoppers: $870-$1,375, flatcars: $680-$960)
	/// </summary>
	public static readonly Dictionary<string, int> GondolaPrices = new Dictionary<string, int>
	{
		{ "gb-gondola1", 1200 },    // 55ft High-Side Gondola (largest capacity) - no zero
		{ "gb-gondola02", 900 },    // 40ft Wooden Gondola (Steel Frame) - with zero
		{ "gb-gondola3", 800 },     // 35ft Wooden Gondola (smallest) - no zero
		{ "gb-gondola04", 1500 },   // 70T Mill Gondola (heaviest, highest capacity) - with zero
		{ "gb-gondola05", 1100 },   // Class G30 Gondola - with zero
		{ "gs-gondola06", 950 }     // G-50-9 Gondola (vintage with bottom chute)
	};

	/// <summary>
	/// Get the custom price for a car identifier, or return 0 if not configured
	/// </summary>
	public static int GetCustomPrice(string identifier)
	{
		if (GondolaPrices.TryGetValue(identifier, out int basePrice))
		{
			return basePrice;
		}
		return 0;
	}
}
