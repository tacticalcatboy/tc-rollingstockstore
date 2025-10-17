using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace RollingStockStore.UMM;

#if DEBUG
[EnableReloading]
#endif
public static class Loader
{
	public static UnityModManager.ModEntry ModEntry { get; private set; } = null!;
	public static Harmony HarmonyInstance { get; private set; } = null!;
	public static RollingStockStore? Instance { get; private set; }

	internal static RollingStockStoreSettings Settings = null!;

	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		if (ModEntry != null || Instance != null)
		{
			modEntry.Logger.Warning("RollingStockStore is already loaded!");
			return false;
		}

		ModEntry = modEntry;
		Settings = UnityModManager.ModSettings.Load<RollingStockStoreSettings>(modEntry);
		ModEntry.OnUnload = Unload;
		ModEntry.OnToggle = OnToggle;
		ModEntry.OnGUI = OnGUI;
		ModEntry.OnSaveGUI = Settings.Save;

		HarmonyInstance = new Harmony(modEntry.Info.Id);
		return true;
	}

	public static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
	{
		if (value)
		{
			try
			{
				HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
				var go = new GameObject("[RollingStockStore]");
				Instance = go.AddComponent<RollingStockStore>();
				UnityEngine.Object.DontDestroyOnLoad(go);
				Instance.Settings = Settings;
			}
			catch (Exception ex)
			{
				modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
				HarmonyInstance?.UnpatchAll(modEntry.Info.Id);
				if (Instance != null) UnityEngine.Object.DestroyImmediate(Instance.gameObject);
				Instance = null;
				return false;
			}
		}
		else
		{
			HarmonyInstance.UnpatchAll(modEntry.Info.Id);
			if (Instance != null) UnityEngine.Object.DestroyImmediate(Instance.gameObject);
			Instance = null;
		}

		return true;
	}

	private static bool Unload(UnityModManager.ModEntry modEntry)
	{
		return true;
	}

	public class RollingStockStoreSettings : UnityModManager.ModSettings, IDrawable
	{
		// Gondola settings
		[Draw("Enable Gondola Purchases", DrawType.Toggle)]
		public bool EnableGondolas = true;

		[Draw("Gondola Base Price Multiplier", DrawType.Slider, Min = 0.5f, Max = 2.0f, Precision = 1)]
		public float GondolaPriceMultiplier = 1.0f;

		public override void Save(UnityModManager.ModEntry modEntry)
		{
			Save(this, modEntry);
		}

		public void OnChange()
		{
			Instance?.OnSettingsChanged();
		}
	}

	private static void OnGUI(UnityModManager.ModEntry modEntry)
	{
		Settings.Draw(modEntry);

		GUILayout.Space(10);
		GUILayout.Label("Individual Gondola Prices:", GUILayout.ExpandWidth(false));
		GUILayout.BeginVertical("box");

		foreach (var kvp in CarPriceConfig.GondolaPrices)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label($"{kvp.Key}:", GUILayout.Width(150));
			int finalPrice = Mathf.RoundToInt(kvp.Value * Settings.GondolaPriceMultiplier);
			GUILayout.Label($"${finalPrice:N0}", GUILayout.Width(100));
			GUILayout.EndHorizontal();
		}

		GUILayout.EndVertical();
	}

	public static void Log(string str)
	{
		ModEntry?.Logger.Log(str);
	}

	public static void LogDebug(string str)
	{
#if DEBUG
		ModEntry?.Logger.Log(str);
#endif
	}
}
