using System.Linq;
using UnityEditor;
using UnityEngine;

public static class FixTextureOverrides
{
	private static readonly string[] DefaultTargetFolders =
	{
		"Assets/BakeryLightmaps",
		"Assets/Bakery",
		"Assets/Editor/x64/Bakery"
	};

	[MenuItem("Tools/Validation/Fix Texture Overrides (ASTC 10x10, 256) for Bakery")] 
	public static void FixOverridesForBakeryFolders()
	{
		ApplyOverrides(DefaultTargetFolders);
	}

	[MenuItem("Tools/Validation/Fix Texture Overrides (ASTC 10x10, 256) for Selection")] 
	public static void FixOverridesForSelection()
	{
		var selectedAssetPaths = Selection.assetGUIDs
			.Select(AssetDatabase.GUIDToAssetPath)
			.Where(p => !string.IsNullOrEmpty(p))
			.ToArray();

		if (selectedAssetPaths.Length == 0)
		{
			Debug.Log("No selection. Nothing to process.");
			return;
		}

		ApplyOverridesForExplicitPaths(selectedAssetPaths);
	}

	[MenuItem("Tools/Validation/Fix Texture Overrides (ASTC 10x10, 256) for Whole Project")] 
	public static void FixOverridesForWholeProject()
	{
		var allTextures = AssetDatabase.FindAssets("t:Texture");
		var allPaths = allTextures.Select(AssetDatabase.GUIDToAssetPath).ToArray();
		ApplyOverridesForExplicitPaths(allPaths);
	}

	private static void ApplyOverrides(string[] folders)
	{
		var guids = AssetDatabase.FindAssets("t:Texture", folders);
		var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
		ApplyOverridesForExplicitPaths(paths);
	}

	private static void ApplyOverridesForExplicitPaths(string[] paths)
	{
		int processedCount = 0;
		int changedCount = 0;
		foreach (var path in paths)
		{
			var importer = AssetImporter.GetAtPath(path) as TextureImporter;
			if (importer == null)
			{
				continue;
			}

			processedCount++;

			bool modified = false;

			modified |= SetPlatform(importer, "Android", TextureImporterFormat.ASTC_10x10, 256);
			modified |= SetPlatform(importer, "iPhone", TextureImporterFormat.ASTC_10x10, 256);

			if (modified)
			{
				changedCount++;
				importer.SaveAndReimport();
			}
		}

		Debug.Log($"Fixed overrides on {changedCount} of {processedCount} textures.");
	}

	private static bool SetPlatform(TextureImporter importer, string platformName, TextureImporterFormat format, int maxSize)
	{
		var settings = importer.GetPlatformTextureSettings(platformName);
		bool before = settings.overridden && settings.format == format && settings.maxTextureSize == maxSize;

		settings.overridden = true;
		settings.format = format;
		settings.maxTextureSize = maxSize;
		importer.SetPlatformTextureSettings(settings);

		var after = settings.overridden && settings.format == format && settings.maxTextureSize == maxSize;
		return !before && after;
	}
}


