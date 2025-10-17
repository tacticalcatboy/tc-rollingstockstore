using UnityEngine;
using RollingStockStore.UMM;

namespace RollingStockStore.UMM;

public class RollingStockStore : MonoBehaviour
{
	public Loader.RollingStockStoreSettings? Settings { get; set; }

	private void Awake()
	{
		Loader.Log("RollingStockStore MonoBehaviour initialized");
	}

	public void OnSettingsChanged()
	{
		if (Settings != null)
		{
			Loader.Log("Settings changed - gondola purchases " + (Settings.EnableGondolas ? "enabled" : "disabled"));
		}
	}
}
