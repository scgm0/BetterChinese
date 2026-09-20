using System;
using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace BetterChinese;

public static class ForcedTranslation {
	// public static readonly MethodInfo TextDrawUtilDrawTextLineMethod =
	// 	AccessTools.Method(typeof(TextDrawUtil), nameof(TextDrawUtil.DrawTextLine));
	//
	// public static readonly MethodInfo ContextTextExtentsMethod =
	// 	AccessTools.Method(typeof(Context), nameof(Context.TextExtents), [typeof(string)]);
	//
	// public static readonly MethodInfo CairoFontGetTextExtentsMethod =
	// 	AccessTools.Method(typeof(CairoFont), nameof(CairoFont.GetTextExtents));
	//
	// public static readonly MethodInfo TextDrawUtilDrawTextLinePreFixMethod =
	// 	AccessTools.Method(typeof(ForcedTranslation), nameof(TextDrawUtilDrawTextLinePreFix));
	//
	// public static readonly MethodInfo ContextTextExtentsPreFixMethod =
	// 	AccessTools.Method(typeof(ForcedTranslation), nameof(ContextTextExtentsMethodPreFix));
	//
	// public static readonly MethodInfo CairoFontGetTextExtentsPreFixMethod =
	// 	AccessTools.Method(typeof(ForcedTranslation), nameof(CairoFontGetTextExtentsPreFix));

	static private readonly List<string> List = [];

	public static void Patch(Harmony harmony) {
		if (!BetterChineseModSystem.Config.强制翻译) {
			return;
		}

		// harmony.Patch(original: ContextTextExtentsMethod,
		// 	prefix: ContextTextExtentsPreFixMethod);
		// harmony.Patch(original: CairoFontGetTextExtentsMethod,
		// 	prefix: CairoFontGetTextExtentsPreFixMethod);
		// harmony.Patch(original: TextDrawUtilDrawTextLineMethod,
		// 	prefix: TextDrawUtilDrawTextLinePreFixMethod);
		harmony.Patch(
			original: AccessTools.Method(typeof(GuiElementEmbossedText), "Compose"),
			prefix: AccessTools.Method(typeof(ForcedTranslation), nameof(GuiElementEmbossedTextComposePreFix))
		);
		harmony.Patch(
			original: AccessTools.Constructor(typeof(GuiElementTextBase),
			[
				typeof(ICoreClientAPI),
				typeof(string),
				typeof(CairoFont),
				typeof(ElementBounds)
			]),
			prefix: AccessTools.Method(typeof(ForcedTranslation), nameof(GuiElementTextBasePreFix))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(GuiElementDynamicText), "SetNewText"),
			prefix: AccessTools.Method(typeof(ForcedTranslation), nameof(GuiElementDynamicTextSetNewTextPreFix))
		);
		harmony.Patch(
			original: AccessTools.Method(typeof(VtmlUtil),
				"Richtextify",
				[
					typeof(ICoreClientAPI),
					typeof(string),
					typeof(CairoFont),
					typeof(Action<LinkTextComponent>)
				]),
			prefix: AccessTools.Method(typeof(ForcedTranslation), nameof(VtmlUtilRichtextifyPreFix))
		);
	}

	public static void UnPatch(Harmony harmony) {
		harmony.Unpatch(
			original: AccessTools.Method(typeof(GuiElementEmbossedText), "Compose"),
			patch: AccessTools.Method(typeof(ForcedTranslation), nameof(GuiElementEmbossedTextComposePreFix))
		);
		harmony.Unpatch(
			original: AccessTools.Constructor(typeof(GuiElementTextBase),
			[
				typeof(ICoreClientAPI),
				typeof(string),
				typeof(CairoFont),
				typeof(ElementBounds)
			]),
			patch: AccessTools.Method(typeof(ForcedTranslation), nameof(GuiElementTextBasePreFix))
		);
		harmony.Unpatch(
			original: AccessTools.Method(typeof(GuiElementDynamicText), "SetNewText"),
			patch: AccessTools.Method(typeof(ForcedTranslation), nameof(GuiElementDynamicTextSetNewTextPreFix))
		);
		harmony.Unpatch(
			original: AccessTools.Method(typeof(VtmlUtil),
				"Richtextify",
				[
					typeof(ICoreClientAPI),
					typeof(string),
					typeof(CairoFont),
					typeof(Action<LinkTextComponent>)
				]),
			patch: AccessTools.Method(typeof(ForcedTranslation), nameof(VtmlUtilRichtextifyPreFix))
		);
		// harmony.Unpatch(original: ContextTextExtentsMethod,
		// 	patch: ContextTextExtentsPreFixMethod);
		// harmony.Unpatch(original: CairoFontGetTextExtentsMethod,
		// 	patch: CairoFontGetTextExtentsPreFixMethod);
		// harmony.Unpatch(original: TextDrawUtilDrawTextLineMethod,
		// 	patch: TextDrawUtilDrawTextLinePreFixMethod);
	}

	static private void Translation(ref string text) {
		if (string.IsNullOrWhiteSpace(text)) return;
		if (!List.Contains(text)) {
			List.Add(text);
			BetterChineseModSystem.Api?.Logger?.Debug($"『{text.Replace("\n", "\\n").Replace("\r", "\\r")}』");
		}

		var old = $"{text}";
		if (Lang.HasTranslation(old, false, false)) {
			text = $"{Lang.Get(old)}";
		} else if (old.Contains('\n')) {
			var arr = old.Split('\n');
			for (var i = arr.Length - 1; i >= 0; i--) {
				var line = arr[i];
				Translation(ref line);
				arr[i] = line;
			}

			text = string.Join('\n', arr);
		}
	}

	// ReSharper disable once InconsistentNaming
	public static void GuiElementEmbossedTextComposePreFix(ref string ___text) { Translation(ref ___text); }

	public static void GuiElementTextBasePreFix(ref string text) { Translation(ref text); }

	public static void GuiElementDynamicTextSetNewTextPreFix(ref string text) { Translation(ref text); }

	public static void VtmlUtilRichtextifyPreFix(ref string vtmlCode) { Translation(ref vtmlCode); }
	// public static void TextDrawUtilDrawTextLinePreFix(ref string text) { Translation(ref text); }
	//
	// public static void ContextTextExtentsMethodPreFix(ref string s) { Translation(ref s); }
	//
	// public static void CairoFontGetTextExtentsPreFix(ref string text) { Translation(ref text); }
}