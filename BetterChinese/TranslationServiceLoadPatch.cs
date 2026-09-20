using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

// ReSharper disable InconsistentNaming

namespace BetterChinese;

public static class TranslationServiceLoadPatch {
	public static readonly MethodInfo OriginalMethod =
		typeof(TranslationService).GetMethod(nameof(TranslationService.Load))!;

	public static readonly MethodInfo PostfixMethod =
		typeof(TranslationServiceLoadPatch).GetMethod("Postfix")!;

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "LoadEntries")]
	public static extern void TranslationServiceLoadEntries(
		this TranslationService self,
		Dictionary<string, string> entryCache,
		Dictionary<string, KeyValuePair<Regex, string>> regexCache,
		Dictionary<string, string> wildcardCache,
		Dictionary<string, string> entries,
		string domain = "game"
	);

	static private void LoadEntries(
		TranslationService translationService,
		string currentPath,
		string domain,
		ILogger logger,
		Dictionary<string, KeyValuePair<Regex, string>> regexCache,
		Dictionary<string, string> wildcardCache,
		Dictionary<string, string> entries) {
		foreach (var file in Directory.GetFiles(currentPath)) {
			if (!file.EndsWith($"{translationService.LanguageCode}.json", StringComparison.OrdinalIgnoreCase)) {
				continue;
			}

			try {
				translationService.TranslationServiceLoadEntries(entries,
					regexCache,
					wildcardCache,
					JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file)) ?? [],
					domain
				);
			} catch (Exception ex) {
				logger.Error("无法加载语言文件: " + file);
				logger.Error(ex);
			}
		}
	}

	public static void Postfix(
		bool lazyLoad,
		TranslationService __instance,
		ref IAssetManager ___assetManager,
		ref ILogger ___logger,
		ref Dictionary<string, string> ___entryCache,
		ref Dictionary<string, KeyValuePair<Regex, string>> ___regexCache,
		ref Dictionary<string, string> ___wildcardCache) {
		if (lazyLoad) {
			return;
		}

		var entryCache = ___entryCache;
		var regexCache = ___regexCache;
		var wildcardCache = ___wildcardCache;
		var logger = ___logger;
		___assetManager.Origins.Foreach(assetOrigin => {
			if (!Directory.Exists(assetOrigin.OriginPath)) {
				return;
			}

			foreach (var directory in Directory.GetDirectories(assetOrigin.OriginPath)) {
				if (Path.GetFileName(Path.GetDirectoryName(directory))
					?.Equals("assets", StringComparison.OrdinalIgnoreCase) is not true) {
					continue;
				}

				var modId = Path.GetFileName(directory);
				var modEnabled = BetterChineseModSystem.Api?.ModLoader.IsModEnabled(modId) ?? true;
				logger.Debug($"更好的汉化 {modId}: {modEnabled}");
				if (!modEnabled) {
					continue;
				}

				foreach (var domain in Directory.GetDirectories(directory)) {
					var currentPath = Path.Combine(domain, AssetCategory.lang.Code);
					if (!Directory.Exists(currentPath)) {
						continue;
					}

					LoadEntries(__instance,
						currentPath,
						Path.GetFileName(domain).ToLowerInvariant(),
						logger,
						regexCache,
						wildcardCache,
						entryCache);
				}
			}
		});
	}
}