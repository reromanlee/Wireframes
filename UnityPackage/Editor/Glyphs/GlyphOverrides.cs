using System.Collections.Generic;
using System.Text;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Says which glyphs of a glyph list a pack overrides: those that a pack higher in the list has too, which it draws
    /// instead, so overriding is never silent.
    /// </summary>
    internal static class GlyphOverrides
    {
        private const int ShownNames = 6;

        /// <summary>One sentence per pair of packs where the first overrides glyphs of the second, in list order.</summary>
        internal static List<string> Describe(WireframeGlyphs glyphs)
        {
            IReadOnlyList<WireframeGlyphPack> packs = glyphs.Packs;
            Dictionary<int, int> characterWinners = new();
            Dictionary<int, int> symbolWinners = new();
            // Overridden names by winning and losing pack, in the order found.
            Dictionary<(int Winner, int Loser), List<string>> overridden = new();
            List<(int Winner, int Loser)> pairs = new();
            for (int pack = 0; pack < packs.Count; pack++)
            {
                if (packs[pack] == null || IndexOf(packs, packs[pack]) < pack)
                {
                    continue;
                }
                foreach (CharacterGlyph character in packs[pack].Characters)
                {
                    Record(characterWinners, character.Character, pack, GlyphNames.CharacterOf(character.Character));
                }
                foreach (SymbolGlyph symbol in packs[pack].Symbols)
                {
                    if (SymbolKeys.FindProblem(symbol.Keyword) == null)
                    {
                        Record(symbolWinners, SymbolKeys.Of(symbol.Keyword), pack, symbol.Keyword);
                    }
                }
            }

            List<string> sentences = new();
            StringBuilder sentence = new();
            foreach ((int winner, int loser) in pairs)
            {
                List<string> names = overridden[(winner, loser)];
                sentence.Clear();
                sentence.Append(packs[winner].name).Append(" overrides ");
                int shown = names.Count > ShownNames ? ShownNames - 1 : names.Count;
                for (int i = 0; i < shown; i++)
                {
                    sentence.Append(names[i]);
                    sentence.Append(i == shown - 1 ? string.Empty : i == names.Count - 2 ? " and " : ", ");
                }
                if (shown < names.Count)
                {
                    sentence.Append($" and {names.Count - shown} more");
                }
                sentence.Append(" from ").Append(packs[loser].name).Append('.');
                sentences.Add(sentence.ToString());
            }
            return sentences;

            void Record(Dictionary<int, int> winners, int key, int pack, string name)
            {
                if (!winners.TryGetValue(key, out int winner))
                {
                    winners.Add(key, pack);
                    return;
                }
                if (winner == pack)
                {
                    return;
                }
                if (!overridden.TryGetValue((winner, pack), out List<string> names))
                {
                    names = new List<string>();
                    overridden.Add((winner, pack), names);
                    pairs.Add((winner, pack));
                }
                names.Add(name);
            }
        }

        private static int IndexOf(IReadOnlyList<WireframeGlyphPack> packs, WireframeGlyphPack pack)
        {
            for (int i = 0; i < packs.Count; i++)
            {
                if (packs[i] == pack)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
