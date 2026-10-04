using System.Globalization;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>How the Glyph Editor names characters, and reads the character someone types to add one.</summary>
    internal static class GlyphNames
    {
        /// <summary>The character itself, or a name for one that shows nothing, such as Space.</summary>
        internal static string CharacterOf(int character)
        {
            if (character == ' ')
            {
                return "Space";
            }
            if (character == 0xA0)
            {
                return "No-break space";
            }
            if (!IsValid(character) || character <= 0xFFFF && char.IsControl((char)character))
            {
                return CodeOf(character);
            }
            return char.ConvertFromUtf32(character);
        }

        /// <summary>The Unicode notation of <paramref name="character"/>, such as U+0041 for A.</summary>
        internal static string CodeOf(int character)
        {
            return $"U+{character:X4}";
        }

        /// <summary>
        /// Reads a character typed as itself, such as A, or as its code, such as U+0041. Returns why it can't be added to
        /// a pack, or null when it can.
        /// </summary>
        internal static string Parse(string text, out int character)
        {
            character = 0;
            text ??= string.Empty;
            // A space is a character too, so only longer input is trimmed.
            string trimmed = text.Length > 1 ? text.Trim() : text;
            if (trimmed.Length > 2 && (trimmed[0] == 'U' || trimmed[0] == 'u') && trimmed[1] == '+')
            {
                if (!int.TryParse(trimmed.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture,
                        out character) || !IsValid(character))
                {
                    return $"{trimmed} isn't a Unicode character.";
                }
            }
            else if (trimmed.Length == 1 || trimmed.Length == 2 && char.IsSurrogatePair(trimmed[0], trimmed[1]))
            {
                character = char.ConvertToUtf32(trimmed, 0);
            }
            else
            {
                return "Type one character, such as A, or its code, such as U+0041.";
            }
            return character < 0x20 || character is >= 0x7F and < 0xA0
                ? "Control characters are never drawn, so they can't have a glyph."
                : null;
        }

        private static bool IsValid(int character)
        {
            return character is >= 0 and < 0xD800 or > 0xDFFF and <= 0x10FFFF;
        }
    }
}
