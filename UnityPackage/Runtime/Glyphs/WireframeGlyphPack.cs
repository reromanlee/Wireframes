using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A pack of glyphs drawn with lines: characters of text, found by character, and symbols, found by the members of
    /// the enum the Glyph Editor generates for the pack. Every glyph is drawn in a box from 0 to 1 on both axes, with the
    /// baseline 0.25 up from its bottom in every pack. A <see cref="WireframeGlyphs"/> lists the packs that texts and
    /// symbols draw from. Create one with <b>Create > Wireframes > Glyph Pack</b> and edit it in the Glyph Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "Glyph Pack", menuName = "Wireframes/Glyph Pack", order = 1)]
    [HelpURL(HelpUrl)]
    public sealed class WireframeGlyphPack : ScriptableObject, ISerializationCallbackReceiver
    {
        internal const string HelpUrl = "https://github.com/reromanlee/Wireframes/blob/main/Documentation/USAGE.md#glyph-packs";

        /// <summary>Version of the pack's data, saved with it so older data can be brought up to date.</summary>
        internal const int FormatVersion = 1;

        /// <summary>Height of the baseline in every pack's glyph box, so characters from different packs line up.</summary>
        internal const float Baseline = 0.25f;

        internal const float DefaultSpaceWidth = 0.5f;
        internal const float DefaultXHeight = 14f / 24f;
        internal const float DefaultCapHeight = 20f / 24f;
        internal const int DefaultGridDivisions = 12;
        internal const int MinimumGridDivisions = 2;
        internal const int MaximumGridDivisions = 24;

        internal const string SpaceWidthField = nameof(_spaceWidth);
        internal const string XHeightField = nameof(_xHeight);
        internal const string CapHeightField = nameof(_capHeight);
        internal const string GridDivisionsField = nameof(_gridDivisions);
        internal const string CharactersField = nameof(_characters);
        internal const string SymbolsField = nameof(_symbols);
        internal const string EnumSettingsField = nameof(_enumSettings);

        [SerializeField, HideInInspector] private int _formatVersion = FormatVersion;

        [Tooltip("Width proportional text gives a character without lines, such as a space, in glyph boxes.")]
        [SerializeField] private float _spaceWidth = DefaultSpaceWidth;

        [Tooltip("Height of the x-height guide the Glyph Editor draws in the glyph box. Text layout doesn't use it.")]
        [SerializeField] private float _xHeight = DefaultXHeight;

        [Tooltip("Height of the cap-height guide the Glyph Editor draws in the glyph box. Text layout doesn't use it.")]
        [SerializeField] private float _capHeight = DefaultCapHeight;

        [Tooltip("Divisions of the Glyph Editor's grid across the glyph box, which points snap to.")]
        [SerializeField] private int _gridDivisions = DefaultGridDivisions;

        [SerializeField] private CharacterGlyph[] _characters = Array.Empty<CharacterGlyph>();
        [SerializeField] private SymbolGlyph[] _symbols = Array.Empty<SymbolGlyph>();
        [SerializeField] private GlyphEnumSettings _enumSettings;

        // Built on first use and again after any glyph data changes, which happens only in the Editor.
        [NonSerialized] private Dictionary<int, int> _characterIndices;
        [NonSerialized] private Dictionary<int, int> _symbolIndices;
        [NonSerialized] private GlyphMetrics[] _characterMetrics;
        [NonSerialized] private GlyphMetrics[] _symbolMetrics;
        [NonSerialized] private int _cacheVersion;
        [NonSerialized] private bool _hasWarnedAboutFormat;

        /// <summary>Width proportional text gives a character without lines, such as a space, in glyph boxes.</summary>
        internal float SpaceWidth
        {
            get => _spaceWidth;
        }

        internal float XHeight
        {
            get => _xHeight;
        }

        internal float CapHeight
        {
            get => _capHeight;
        }

        internal int GridDivisions
        {
            get => _gridDivisions;
        }

        /// <summary>The characters, in the order they are saved; never null.</summary>
        internal CharacterGlyph[] Characters
        {
            get => _characters ?? Array.Empty<CharacterGlyph>();
        }

        /// <summary>The symbols, in the order they are saved, which is the order of their enum's members; never null.</summary>
        internal SymbolGlyph[] Symbols
        {
            get => _symbols ?? Array.Empty<SymbolGlyph>();
        }

        internal GlyphEnumSettings EnumSettings
        {
            get => _enumSettings;
            set => _enumSettings = value;
        }

        /// <summary>Finds the glyph of <paramref name="character"/>, a Unicode code point; the first one when the pack repeats it.</summary>
        internal bool TryFindCharacter(int character, out int index)
        {
            EnsureCache();
            return _characterIndices.TryGetValue(character, out index);
        }

        /// <summary>Finds the symbol whose keyword hashes to <paramref name="key"/>; the first one when the pack repeats it.</summary>
        internal bool TryFindSymbol(int key, out int index)
        {
            EnsureCache();
            return _symbolIndices.TryGetValue(key, out index);
        }

        internal GlyphMetrics CharacterMetricsAt(int index)
        {
            EnsureCache();
            return _characterMetrics[index];
        }

        internal GlyphMetrics SymbolMetricsAt(int index)
        {
            EnsureCache();
            return _symbolMetrics[index];
        }

        /// <summary>Replaces every glyph, for tests and tools that build packs from code.</summary>
        internal void SetGlyphs(CharacterGlyph[] characters, SymbolGlyph[] symbols)
        {
            _characters = characters ?? Array.Empty<CharacterGlyph>();
            _symbols = symbols ?? Array.Empty<SymbolGlyph>();
            GlyphEdits.Record();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Inspector and Glyph Editor edits, undo and reimports all come through here.
            GlyphEdits.Record();
        }

        private void OnValidate()
        {
            if (!float.IsFinite(_spaceWidth) || _spaceWidth < 0f)
            {
                _spaceWidth = DefaultSpaceWidth;
            }
            _xHeight = float.IsFinite(_xHeight) ? Mathf.Clamp01(_xHeight) : DefaultXHeight;
            _capHeight = float.IsFinite(_capHeight) ? Mathf.Clamp01(_capHeight) : DefaultCapHeight;
            _gridDivisions = Mathf.Clamp(_gridDivisions, MinimumGridDivisions, MaximumGridDivisions);
            GlyphEdits.Record();
        }

        private void EnsureCache()
        {
            int version = GlyphEdits.Version;
            if (_characterIndices != null && _cacheVersion == version)
            {
                return;
            }
            _cacheVersion = version;
            WarnAboutNewerFormat();

            CharacterGlyph[] characters = Characters;
            _characterIndices ??= new Dictionary<int, int>(characters.Length);
            _characterIndices.Clear();
            Array.Resize(ref _characterMetrics, characters.Length);
            for (int i = 0; i < characters.Length; i++)
            {
                _characterIndices.TryAdd(characters[i].Character, i);
                _characterMetrics[i] = GlyphMetrics.Measure(characters[i].Strokes);
            }

            SymbolGlyph[] symbols = Symbols;
            _symbolIndices ??= new Dictionary<int, int>(symbols.Length);
            _symbolIndices.Clear();
            Array.Resize(ref _symbolMetrics, symbols.Length);
            for (int i = 0; i < symbols.Length; i++)
            {
                string keyword = symbols[i].Keyword;
                if (SymbolKeys.FindProblem(keyword) == null)
                {
                    _symbolIndices.TryAdd(SymbolKeys.Of(keyword), i);
                }
                _symbolMetrics[i] = GlyphMetrics.Measure(symbols[i].Strokes);
            }
        }

        private void WarnAboutNewerFormat()
        {
            if (_formatVersion <= FormatVersion || _hasWarnedAboutFormat)
            {
                return;
            }
            _hasWarnedAboutFormat = true;
            WireframesLog.Warning(
                $"The glyph pack '{name}' was saved by a newer version of Wireframes, so some of its glyphs may draw "
                + "wrong. Update the package to draw it as it was made.", this);
        }
    }
}
