using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Warnings about glyphs that can't be found, each logged once per glyph list and character or symbol, so a text
    /// updated every frame doesn't flood the Console. Main thread only.
    /// </summary>
    internal static class GlyphWarnings
    {
        private static readonly HashSet<(WireframeGlyphs Glyphs, int Value, bool IsSymbol)> Reported = new();
        private static bool _hasReportedMissingDefault;

        /// <summary>Warns that a text or symbol was given no glyphs and the package's default ones are missing.</summary>
        internal static void ReportMissingDefault(Object context)
        {
            if (_hasReportedMissingDefault)
            {
                return;
            }
            _hasReportedMissingDefault = true;
            WireframesLog.Warning(
                "There are no glyphs to draw with: the package's Default Glyphs asset is missing. Reinstall the package, "
                + "or give texts and symbols glyphs of your own.", context);
        }

        /// <summary>Warns that <paramref name="glyphs"/> lack <paramref name="codePoint"/>, which is drawn as ? instead.</summary>
        internal static void ReportMissingCharacter(WireframeGlyphs glyphs, int codePoint, Object context)
        {
            if (!Reported.Add((glyphs, codePoint, false)))
            {
                return;
            }
            // Lone surrogates aren't characters on their own, so only their code is shown.
            bool isCharacter = codePoint is >= 0 and < 0xD800 or > 0xDFFF and <= 0x10FFFF;
            string shown = isCharacter ? $"'{char.ConvertFromUtf32(codePoint)}' (U+{codePoint:X4})" : $"U+{codePoint:X4}";
            WireframesLog.Warning(
                $"{shown} isn't in the glyphs '{glyphs.name}', so it's drawn as '?'. Add it to one of their packs in the "
                + "Glyph Editor, or add a pack that has it.", context);
        }

        /// <summary>Warns that <paramref name="glyphs"/> lack the symbol of <paramref name="key"/>, which is drawn as ? instead.</summary>
        internal static void ReportMissingSymbol(WireframeGlyphs glyphs, int key, Object context)
        {
            if (!Reported.Add((glyphs, key, true)))
            {
                return;
            }
            WireframesLog.Warning(
                $"A symbol isn't in the glyphs '{glyphs.name}', so it's drawn as '?'. Its keyword may have been renamed or "
                + "removed: pick the symbol again, or add a pack that has it.", context);
        }
    }
}
