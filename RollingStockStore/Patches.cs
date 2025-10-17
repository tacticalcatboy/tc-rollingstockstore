using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RollingStockStore.UMM;

[HarmonyPatch]
public static class EquipmentWindowPatches
{
	/// <summary>
	/// Patch the EquipmentWindow.ShouldShow method to allow configured rolling stock to appear in the store
	/// This method has access to both the identifier and the CarDefinition
	/// </summary>
	static MethodBase TargetMethod()
	{
		// Find the EquipmentWindow type
		var equipmentWindowType = AccessTools.TypeByName("UI.Equipment.EquipmentWindow");
		if (equipmentWindowType == null)
		{
			Loader.Log("ERROR: Could not find EquipmentWindow type!");
			return null!;
		}

		// Find the ShouldShow method
		var shouldShowMethod = AccessTools.Method(equipmentWindowType, "ShouldShow");
		if (shouldShowMethod == null)
		{
			Loader.Log("ERROR: Could not find ShouldShow method!");
			return null!;
		}

		Loader.Log($"Successfully found ShouldShow method: {shouldShowMethod.DeclaringType?.FullName}.{shouldShowMethod.Name}");
		return shouldShowMethod;
	}

	private static bool _firstCallLogged = false;

	[HarmonyPostfix]
	public static void Postfix(object info, ref bool __result)
	{
		// Log first call to verify patch is executing and see what type we're getting
		if (!_firstCallLogged)
		{
			Loader.Log("ShouldShow patch is being called!");
			Loader.Log($"Parameter type: {info?.GetType().FullName ?? "null"}");

			// Log all properties (including non-public)
			if (info != null)
			{
				var publicProps = info.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
				Loader.Log($"Public properties: {string.Join(", ", publicProps.Select(p => $"{p.Name}:{p.PropertyType.Name}"))}");

				var allProps = info.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				Loader.Log($"All properties: {string.Join(", ", allProps.Select(p => $"{p.Name}:{p.PropertyType.Name}"))}");

				// Also check fields
				var allFields = info.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				Loader.Log($"All fields: {string.Join(", ", allFields.Select(f => $"{f.Name}:{f.FieldType.Name}"))}");
			}

			_firstCallLogged = true;
		}

		// Only modify if mod is enabled and gondolas are enabled
		if (Loader.Settings == null || !Loader.Settings.EnableGondolas)
			return;

		// If already showing, don't need to do anything
		if (__result)
			return;

		try
		{
			// Get the Identifier FIELD from TypedContainerItem<CarDefinition> (it's a field, not a property!)
			var identifierField = info?.GetType().GetField("Identifier");
			if (identifierField == null)
			{
				Loader.Log("ERROR: Could not get Identifier field from info");
				return;
			}

			string? identifier = identifierField.GetValue(info) as string;
			if (identifier == null)
			{
				Loader.Log("ERROR: Identifier field returned null");
				return;
			}

			// Log gondola checks
			if (identifier?.Contains("gondola") == true)
			{
				Loader.Log($"Checking gondola: {identifier}");
			}

			// Check if this identifier has a custom price configured
			int customPrice = CarPriceConfig.GetCustomPrice(identifier!);
			if (customPrice > 0)
			{
				// Get the Definition FIELD to verify visibleInPlacer and get ModelIdentifier
				var definitionField = info?.GetType().GetField("Definition");
				if (definitionField != null)
				{
					var definition = definitionField.GetValue(info);
					var visibleProp = definition?.GetType().GetProperty("VisibleInPlacer");
					bool? visibleInPlacer = visibleProp?.GetValue(definition) as bool?;

					// Only show if visibleInPlacer is true
					if (visibleInPlacer == true)
					{
						// Cache the price for the model so BasePrice patch can use it
						var modelIdProp = definition?.GetType().GetProperty("ModelIdentifier");
						string? modelId = modelIdProp?.GetValue(definition) as string;
						if (!string.IsNullOrEmpty(modelId))
						{
							CarDefinitionBasePricePatches.SetPriceForModel(modelId!, customPrice);
						}

						Loader.Log($"Making {identifier} visible in store with custom price ${customPrice}");
						__result = true;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Loader.Log($"ERROR in ShouldShow patch: {ex.Message}");
		}
	}
}

[HarmonyPatch]
public static class CarDefinitionBasePricePatches
{
	/// <summary>
	/// Patch the CarDefinition.BasePrice property getter to return custom prices
	/// This is needed for the price calculation in EquipmentPurchase.PurchasePriceForCarPrototype
	/// </summary>
	static MethodBase TargetMethod()
	{
		// Find the CarDefinition type
		var carDefType = AccessTools.TypeByName("Model.Definition.Data.CarDefinition");
		if (carDefType == null)
		{
			Loader.Log("ERROR: Could not find CarDefinition type!");
			return null!;
		}

		// Find the BasePrice property getter
		var basePriceProp = carDefType.GetProperty("BasePrice", BindingFlags.Public | BindingFlags.Instance);
		if (basePriceProp == null)
		{
			Loader.Log("ERROR: Could not find BasePrice property!");
			return null!;
		}

		var getter = basePriceProp.GetGetMethod();
		if (getter == null)
		{
			Loader.Log("ERROR: Could not find BasePrice getter!");
			return null!;
		}

		Loader.Log($"Successfully found BasePrice getter: {getter.DeclaringType?.FullName}.{getter.Name}");
		return getter;
	}

	// We need to cache the current price lookups since CarDefinition doesn't have identifier
	// We'll use ModelIdentifier as a proxy - it's not perfect but should work for most cases
	private static readonly System.Collections.Generic.Dictionary<string, int> _modelPriceCache =
		new System.Collections.Generic.Dictionary<string, int>();

	public static void SetPriceForModel(string modelIdentifier, int price)
	{
		_modelPriceCache[modelIdentifier] = price;
	}

	public static void ClearPriceCache()
	{
		_modelPriceCache.Clear();
	}

	[HarmonyPostfix]
	public static void Postfix(object __instance, ref int __result)
	{
		// Only modify if mod is enabled
		if (Loader.Settings == null || !Loader.Settings.EnableGondolas)
			return;

		// If there's already a price, don't override it
		if (__result > 0)
			return;

		try
		{
			// Get ModelIdentifier from CarDefinition
			var modelIdentifierProp = __instance.GetType().GetProperty("ModelIdentifier");
			if (modelIdentifierProp == null)
				return;

			string? modelId = modelIdentifierProp.GetValue(__instance) as string;
			if (string.IsNullOrEmpty(modelId))
				return;

			// Check cache for custom price
			if (_modelPriceCache.TryGetValue(modelId!, out int cachedPrice))
			{
				__result = Mathf.RoundToInt(cachedPrice * Loader.Settings.GondolaPriceMultiplier);
				Loader.LogDebug($"Applied cached price for model {modelId}: ${__result}");
			}
		}
		catch (Exception ex)
		{
			Loader.Log($"ERROR in BasePrice patch: {ex.Message}");
		}
	}
}
