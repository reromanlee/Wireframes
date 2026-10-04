using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The glyph packs that texts and symbols draw from, highest priority first: the first pack that has a character or
    /// a symbol draws it, so a pack overrides the ones below it. <see cref="Default"/> lists the package's Default Font
    /// and Default Symbols. Create your own with <b>Create > Wireframes > Glyphs</b>.
    /// </summary>
    [CreateAssetMenu(fileName = "Glyphs", menuName = "Wireframes/Glyphs", order = 0)]
    [HelpURL(WireframeGlyphPack.HelpUrl)]
    public sealed class WireframeGlyphs : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>Where <see cref="Default"/> loads from, inside the package's Resources folder.</summary>
        internal const string DefaultResourcePath = "reromanlee.Wireframes/Default Glyphs";

        internal const string PacksField = nameof(_packs);

        private const int AsciiCount = 128;

        private static WireframeGlyphs _default;

        [Tooltip("Packs to draw from, highest priority first: the first pack that has a character or symbol draws it.")]
        [SerializeField] private WireframeGlyphPack[] _packs = Array.Empty<WireframeGlyphPack>();

        // Built on first use and again after any glyph data changes, which happens only in the Editor. Characters below
        // 128 take an array, so most text skips the dictionary.
        [NonSerialized] private ResolvedGlyph[] _asciiCharacters;
        [NonSerialized] private Dictionary<int, ResolvedGlyph> _characters;
        [NonSerialized] private Dictionary<int, ResolvedGlyph> _symbols;
        [NonSerialized] private int _cacheVersion;

        /// <summary>
        /// The glyphs texts and symbols draw with when they are given none: the package's Default Font, with the 95
        /// printable ASCII characters, and its Default Symbols.
        /// </summary>
        public static WireframeGlyphs Default
        {
            get
            {
                if (_default == null)
                {
                    _default = Resources.Load<WireframeGlyphs>(DefaultResourcePath);
                }
                return _default;
            }
        }

        /// <summary>The packs, highest priority first. Empty entries are skipped.</summary>
        public IReadOnlyList<WireframeGlyphPack> Packs
        {
            get => _packs ?? Array.Empty<WireframeGlyphPack>();
        }

        /// <summary>True when one of the packs draws <paramref name="character"/>.</summary>
        public bool Contains(char character)
        {
            return TryResolveCharacter(character, out _);
        }

        /// <summary>
        /// True when one of the packs draws <paramref name="symbol"/>, a member of an enum the Glyph Editor generated for a
        /// pack. Any pack's symbol of the same name counts.
        /// </summary>
        /// <exception cref="ArgumentException"><typeparamref name="TSymbol"/> doesn't have int values.</exception>
        public bool Contains<TSymbol>(TSymbol symbol) where TSymbol : unmanaged, Enum
        {
            return TryResolveSymbol(SymbolKeys.Of(symbol), out _);
        }

        /// <summary>Finds the glyph that draws <paramref name="character"/>, a Unicode code point.</summary>
        internal bool TryResolveCharacter(int character, out ResolvedGlyph glyph)
        {
            EnsureCache();
            if ((uint)character < AsciiCount)
            {
                glyph = _asciiCharacters[character];
                return glyph.Strokes != null;
            }
            return _characters.TryGetValue(character, out glyph);
        }

        /// <summary>Finds the glyph that draws the symbol whose keyword hashes to <paramref name="key"/>.</summary>
        internal bool TryResolveSymbol(int key, out ResolvedGlyph glyph)
        {
            EnsureCache();
            return _symbols.TryGetValue(key, out glyph);
        }

        /// <summary>Replaces the packs, for tests and tools that build glyph lists from code.</summary>
        internal void SetPacks(params WireframeGlyphPack[] packs)
        {
            _packs = packs ?? Array.Empty<WireframeGlyphPack>();
            GlyphEdits.Record();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Inspector edits, undo and reimports all come through here.
            GlyphEdits.Record();
        }

        private void OnValidate()
        {
            GlyphEdits.Record();
        }

        private void EnsureCache()
        {
            int version = GlyphEdits.Version;
            if (_asciiCharacters != null && _cacheVersion == version)
            {
                return;
            }
            _cacheVersion = version;
            _asciiCharacters ??= new ResolvedGlyph[AsciiCount];
            Array.Clear(_asciiCharacters, 0, AsciiCount);
            _characters ??= new Dictionary<int, ResolvedGlyph>();
            _characters.Clear();
            _symbols ??= new Dictionary<int, ResolvedGlyph>();
            _symbols.Clear();

            // Highest priority first, and a glyph already found is kept, so the first pack that has one wins.
            foreach (WireframeGlyphPack pack in Packs)
            {
                if (pack != null)
                {
                    AddCharacters(pack);
                    AddSymbols(pack);
                }
            }
        }

        private void AddCharacters(WireframeGlyphPack pack)
        {
            CharacterGlyph[] characters = pack.Characters;
            for (int i = 0; i < characters.Length; i++)
            {
                int character = characters[i].Character;
                // A pack that repeats a character keeps its first glyph.
                if (!pack.TryFindCharacter(character, out int first) || first != i)
                {
                    continue;
                }
                ResolvedGlyph glyph = new(characters[i].Strokes, pack.CharacterMetricsAt(i), pack);
                if ((uint)character < AsciiCount)
                {
                    if (_asciiCharacters[character].Strokes == null)
                    {
                        _asciiCharacters[character] = glyph;
                    }
                }
                else
                {
                    _characters.TryAdd(character, glyph);
                }
            }
        }

        private void AddSymbols(WireframeGlyphPack pack)
        {
            SymbolGlyph[] symbols = pack.Symbols;
            for (int i = 0; i < symbols.Length; i++)
            {
                string keyword = symbols[i].Keyword;
                if (SymbolKeys.FindProblem(keyword) != null)
                {
                    continue;
                }
                int key = SymbolKeys.Of(keyword);
                // A pack that repeats a keyword keeps its first symbol.
                if (pack.TryFindSymbol(key, out int first) && first == i)
                {
                    _symbols.TryAdd(key, new ResolvedGlyph(symbols[i].Strokes, pack.SymbolMetricsAt(i), pack));
                }
            }
        }
    }
}
