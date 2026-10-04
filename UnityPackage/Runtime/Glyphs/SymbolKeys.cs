using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The numbers symbols are looked up by: the 32-bit FNV-1a hash of their keyword, which is also the value of their
    /// member in a pack's generated enum. The same keyword gives the same number in every pack, so a pack higher in a
    /// glyph list overrides a symbol of the same name, and lookups never compare strings.
    /// </summary>
    internal static class SymbolKeys
    {
        /// <summary>The key of no symbol, and the value of every generated enum's None member.</summary>
        internal const int None = 0;

        /// <summary>The name of every generated enum's member for no symbol, which no keyword may take.</summary>
        internal const string NoneName = "None";

        private const uint OffsetBasis = 2166136261;
        private const uint Prime = 16777619;

        private static readonly HashSet<string> ReservedWords = new(StringComparer.Ordinal)
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
            "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
            "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
            "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
            "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
            "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while", NoneName
        };

        /// <summary>The key of <paramref name="keyword"/>, the hash of its UTF-16 characters.</summary>
        internal static int Of(string keyword)
        {
            uint hash = OffsetBasis;
            foreach (char character in keyword)
            {
                hash = unchecked((hash ^ character) * Prime);
            }
            return unchecked((int)hash);
        }

        /// <summary>The key a member of a generated symbol enum stands for: its value, read without boxing.</summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        internal static int Of<TSymbol>(TSymbol symbol) where TSymbol : unmanaged, Enum
        {
            if (UnsafeUtility.SizeOf<TSymbol>() != sizeof(int))
            {
                throw new ArgumentException(
                    $"{typeof(TSymbol).Name} isn't a symbol enum. Symbol enums have int values, like the ones the "
                    + "Glyph Editor generates.", nameof(symbol));
            }
            return UnsafeUtility.As<TSymbol, int>(ref symbol);
        }

        /// <summary>
        /// Returns why <paramref name="keyword"/> can't name a symbol, or null when it can: it has to be a C# identifier
        /// of ASCII letters, digits and underscores, not a C# keyword, not None, and not hash to the key of no symbol.
        /// </summary>
        internal static string FindProblem(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return "A symbol needs a keyword.";
            }
            if (!IsIdentifierStart(keyword[0]))
            {
                return "A keyword starts with a letter or an underscore.";
            }
            for (int i = 1; i < keyword.Length; i++)
            {
                if (!IsIdentifierStart(keyword[i]) && (keyword[i] < '0' || keyword[i] > '9'))
                {
                    return "A keyword has only letters, digits and underscores, from the English alphabet.";
                }
            }
            if (ReservedWords.Contains(keyword))
            {
                return keyword == NoneName
                    ? "None is the name of the generated enum's member for no symbol."
                    : $"{keyword} is a C# keyword, so it can't name an enum member.";
            }
            return Of(keyword) == None ? "This keyword's hash is the key of no symbol; pick another one." : null;
        }

        private static bool IsIdentifierStart(char character)
        {
            return character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';
        }
    }
}
