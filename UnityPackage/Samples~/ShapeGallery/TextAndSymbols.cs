using System;
using System.Globalization;
using UnityEngine;

namespace reromanlee.Wireframes.Samples
{
    /// <summary>
    /// Draws text and symbols from code: a clock rewritten every frame without allocating, and a symbol that turns while
    /// it steps through the Default Symbols, with its name below it. In the TextAndSymbols scene, it stands beside text
    /// and symbol components, which draw in Edit Mode too.
    /// </summary>
    public sealed class TextAndSymbols : MonoBehaviour
    {
        [Tooltip("Seconds each symbol shows before the next one.")]
        [SerializeField, Min(0.1f)] private float _symbolDuration = 1.5f;

        [Tooltip("Degrees per second the symbol turns around the world's up axis.")]
        [SerializeField] private float _turnSpeed = 45f;

        private readonly char[] _clock = new char[48];
        private WireframeContainer _container;
        private IText _clockText;
        private ISymbol _symbol;
        private IText _symbolName;
        private Transform _symbolBone;
        private DefaultSymbols[] _symbols;
        private string[] _symbolNames;
        private int _symbolIndex;
        private float _nextSymbolTime;

        private void Start()
        {
            _container = new WireframeContainer();
            _clockText = _container.CreateText(transform, new Vector3(0f, 2.4f, 0f), Quaternion.identity, string.Empty);
            _clockText.CharacterWidth = WireframeCharacterWidth.Monospace;
            // Monospace gives each character a whole glyph box, so tighter spacing reads better with the Default Font.
            _clockText.CharacterSpacing = -0.45f;
            _clockText.CharacterSize = 0.6f;
            _clockText.Bounds = new Vector2(4.5f, 1.2f);
            _clockText.HorizontalAlignment = WireframeHorizontalAlignment.Left;
            _clockText.SetColor(new Color(0.55f, 0.9f, 1f));

            _symbolBone = new GameObject("Turning Symbol").transform;
            _symbolBone.SetParent(transform, false);
            _symbol = _container.CreateSymbol(_symbolBone, Vector3.zero, Quaternion.identity, DefaultSymbols.Star);
            _symbol.Size = 2f;
            _symbol.SetColor(new Color(1f, 0.8f, 0.35f));
            _symbolName = _container.CreateText(transform, new Vector3(0f, -1.5f, 0f), Quaternion.identity, string.Empty);
            _symbolName.CharacterSize = 0.45f;

            // Names are read once here, so stepping through the symbols allocates nothing either. GetValues sorts the
            // members by value, so None, which is 0 and draws nothing, comes first and the steps skip it.
            _symbols = (DefaultSymbols[])Enum.GetValues(typeof(DefaultSymbols));
            _symbolNames = Array.ConvertAll(_symbols, symbol => symbol.ToString());
            ShowSymbol(Array.IndexOf(_symbols, DefaultSymbols.Star));
        }

        private void Update()
        {
            int length = Append("Frame ", 0);
            Time.frameCount.TryFormat(_clock.AsSpan(length), out int written, default, CultureInfo.InvariantCulture);
            length = Append("\nTime  ", length + written);
            Time.time.TryFormat(_clock.AsSpan(length), out written, "F1", CultureInfo.InvariantCulture);
            length = Append(" s", length + written);
            _clockText.SetText(_clock.AsSpan(0, length));

            _symbolBone.Rotate(0f, _turnSpeed * Time.deltaTime, 0f, Space.World);
            if (Time.time >= _nextSymbolTime)
            {
                ShowSymbol(_symbolIndex % (_symbols.Length - 1) + 1);
            }
        }

        private void OnDestroy()
        {
            _container?.Dispose();
        }

        private void ShowSymbol(int index)
        {
            _symbolIndex = index;
            _symbol.SetSymbol(_symbols[index]);
            _symbolName.Text = _symbolNames[index];
            _nextSymbolTime = Time.time + _symbolDuration;
        }

        /// <summary>Copies <paramref name="text"/> into the clock buffer at <paramref name="start"/>; returns where it ends.</summary>
        private int Append(string text, int start)
        {
            text.AsSpan().CopyTo(_clock.AsSpan(start));
            return start + text.Length;
        }
    }
}
