using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// A pack's glyphs in two sections, characters sorted by code and symbols in pack order or by name, each a grid of
    /// tiles that wraps to the width, with tiles that add glyphs at the end. Symbols in pack order can be dragged into
    /// another order, which is the order of their enum's members.
    /// </summary>
    internal sealed class GlyphGallery : ScrollView
    {
        private const float DragThreshold = 6f;

        private readonly Foldout _characterSection = new() { value = true };
        private readonly Foldout _symbolSection = new() { value = true };
        private readonly VisualElement _characterGrid = new();
        private readonly VisualElement _symbolGrid = new();
        private readonly Button _addCharacter;
        private readonly Button _addMissingAscii;
        private readonly Button _addSymbol;
        private readonly VisualElement _dropMarker = new();
        private readonly List<GlyphTile> _characterTiles = new();
        private readonly List<GlyphTile> _symbolTiles = new();
        private readonly List<int> _order = new();

        private WireframeGlyphPack _pack;
        private bool _isReadOnly;
        private string _filter = string.Empty;
        private float _tileSize = 64f;
        private bool _sortsSymbolsByName;
        private bool _lacksAscii;
        private GlyphReference _selected = GlyphReference.None;

        private GlyphTile _pressedTile;
        private Vector2 _pressPosition;
        private bool _isDragging;
        private int _dropIndex = -1;

        internal GlyphGallery()
        {
            AddToClassList("glyph-gallery");
            _characterSection.AddToClassList("glyph-gallery__section");
            _symbolSection.AddToClassList("glyph-gallery__section");
            _characterGrid.AddToClassList("glyph-gallery__grid");
            _symbolGrid.AddToClassList("glyph-gallery__grid");
            _dropMarker.AddToClassList("glyph-gallery__drop-marker");
            _dropMarker.pickingMode = PickingMode.Ignore;
            _addCharacter = CreateAddButton("+", "Add a character.", () => AddCharacterClicked?.Invoke(_addCharacter));
            _addMissingAscii = CreateAddButton(
                "+ ASCII", "Add an empty glyph for every printable ASCII character the pack lacks.",
                () => AddMissingAsciiClicked?.Invoke());
            _addSymbol = CreateAddButton("+", "Add a symbol.", () => AddSymbolClicked?.Invoke(_addSymbol));
            _characterSection.Add(_characterGrid);
            _symbolSection.Add(_symbolGrid);
            Add(_characterSection);
            Add(_symbolSection);
        }

        /// <summary>Raised when a tile is clicked, to open its glyph.</summary>
        internal event Action<GlyphReference> GlyphClicked;

        /// <summary>Raised with the button to show a popup at, to add a character.</summary>
        internal event Action<VisualElement> AddCharacterClicked;

        internal event Action AddMissingAsciiClicked;

        /// <summary>Raised with the button to show a popup at, to add a symbol.</summary>
        internal event Action<VisualElement> AddSymbolClicked;

        /// <summary>Raised when a symbol is dropped elsewhere in pack order: its index, and the index it moves to.</summary>
        internal event Action<int, int> SymbolMoved;

        /// <summary>Fills the context menu of a tile's glyph.</summary>
        internal event Action<GlyphReference, VisualElement, DropdownMenu> TileMenuBuilding;

        /// <summary>Text that tiles must contain to show: part of a character, a code or a keyword.</summary>
        internal string Filter
        {
            set
            {
                _filter = value ?? string.Empty;
                ApplyFilter();
            }
        }

        /// <summary>Width of a tile's preview in pixels.</summary>
        internal float TileSize
        {
            set
            {
                _tileSize = value;
                foreach (GlyphTile tile in _characterTiles)
                {
                    tile.SetSize(value);
                }
                foreach (GlyphTile tile in _symbolTiles)
                {
                    tile.SetSize(value);
                }
            }
        }

        /// <summary>True lists symbols by keyword, false in pack order, the order they can be dragged into.</summary>
        internal bool SortsSymbolsByName
        {
            set
            {
                _sortsSymbolsByName = value;
                Refresh();
            }
        }

        /// <summary>The glyph whose tile shows as selected.</summary>
        internal GlyphReference Selected
        {
            set
            {
                _selected = value;
                ApplySelection();
            }
        }

        internal void Show(WireframeGlyphPack pack, bool isReadOnly)
        {
            _pack = pack;
            _isReadOnly = isReadOnly;
            Refresh();
        }

        /// <summary>Brings every tile in line with the pack, reusing the tiles it has.</summary>
        internal void Refresh()
        {
            EndPress(-1);
            if (_pack == null)
            {
                return;
            }
            CharacterGlyph[] characters = _pack.Characters;
            _order.Clear();
            for (int i = 0; i < characters.Length; i++)
            {
                _order.Add(i);
            }
            _order.Sort((a, b) => characters[a].Character.CompareTo(characters[b].Character));
            Fill(_characterGrid, _characterTiles, false);
            _characterSection.text = $"Characters ({characters.Length})";
            _characterGrid.Add(_addCharacter);
            _characterGrid.Add(_addMissingAscii);
            _lacksAscii = !HasAllAscii(characters);

            SymbolGlyph[] symbols = _pack.Symbols;
            _order.Clear();
            for (int i = 0; i < symbols.Length; i++)
            {
                _order.Add(i);
            }
            if (_sortsSymbolsByName)
            {
                _order.Sort((a, b) => string.Compare(symbols[a].Keyword, symbols[b].Keyword, StringComparison.OrdinalIgnoreCase));
            }
            Fill(_symbolGrid, _symbolTiles, true);
            _symbolSection.text = $"Symbols ({symbols.Length})";
            _symbolGrid.Add(_addSymbol);

            ApplyFilter();
            ApplySelection();
        }

        private void Fill(VisualElement grid, List<GlyphTile> tiles, bool isSymbol)
        {
            while (tiles.Count < _order.Count)
            {
                tiles.Add(CreateTile());
            }
            tiles.RemoveRange(_order.Count, tiles.Count - _order.Count);
            grid.Clear();
            for (int i = 0; i < _order.Count; i++)
            {
                GlyphTile tile = tiles[i];
                tile.Show(new GlyphReference(isSymbol, _order[i]), _pack);
                tile.SetSize(_tileSize);
                grid.Add(tile);
            }
        }

        private void ApplyFilter()
        {
            bool isFiltered = _filter.Length > 0;
            foreach (GlyphTile tile in _characterTiles)
            {
                SetShown(tile, !isFiltered || tile.SearchText.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            foreach (GlyphTile tile in _symbolTiles)
            {
                SetShown(tile, !isFiltered || tile.SearchText.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            // Adding glyphs while searching would put them out of sight.
            bool canAdd = !isFiltered && !_isReadOnly;
            SetShown(_addCharacter, canAdd);
            SetShown(_addMissingAscii, canAdd && _lacksAscii);
            SetShown(_addSymbol, canAdd);
        }

        private void ApplySelection()
        {
            foreach (GlyphTile tile in _characterTiles)
            {
                tile.EnableInClassList(GlyphTile.SelectedClass, tile.Glyph.Equals(_selected));
            }
            foreach (GlyphTile tile in _symbolTiles)
            {
                tile.EnableInClassList(GlyphTile.SelectedClass, tile.Glyph.Equals(_selected));
            }
        }

        private GlyphTile CreateTile()
        {
            GlyphTile tile = new();
            tile.RegisterCallback<PointerDownEvent>(OnTilePointerDown);
            tile.RegisterCallback<PointerMoveEvent>(OnTilePointerMove);
            tile.RegisterCallback<PointerUpEvent>(OnTilePointerUp);
            tile.RegisterCallback<PointerCaptureOutEvent>(_ => EndPress(-1));
            tile.AddManipulator(new ContextualMenuManipulator(menuEvent =>
                TileMenuBuilding?.Invoke(tile.Glyph, tile, menuEvent.menu)));
            return tile;
        }

        private void OnTilePointerDown(PointerDownEvent pointerEvent)
        {
            if (pointerEvent.button != 0)
            {
                return;
            }
            _pressedTile = (GlyphTile)pointerEvent.currentTarget;
            _pressPosition = pointerEvent.position;
            _isDragging = false;
            _pressedTile.CapturePointer(pointerEvent.pointerId);
            pointerEvent.StopPropagation();
        }

        private void OnTilePointerMove(PointerMoveEvent pointerEvent)
        {
            if (_pressedTile == null || !_pressedTile.HasPointerCapture(pointerEvent.pointerId))
            {
                return;
            }
            if (!_isDragging)
            {
                Vector2 moved = (Vector2)pointerEvent.position - _pressPosition;
                if (!CanDrag(_pressedTile) || moved.magnitude < DragThreshold)
                {
                    return;
                }
                _isDragging = true;
                _pressedTile.AddToClassList(GlyphTile.DraggedClass);
                _symbolGrid.Add(_dropMarker);
            }
            MoveDropMarker(pointerEvent.position);
        }

        private void OnTilePointerUp(PointerUpEvent pointerEvent)
        {
            if (_pressedTile == null || pointerEvent.button != 0)
            {
                return;
            }
            GlyphReference glyph = _pressedTile.Glyph;
            bool wasDragging = _isDragging;
            int dropIndex = _dropIndex;
            EndPress(pointerEvent.pointerId);
            if (!wasDragging)
            {
                GlyphClicked?.Invoke(glyph);
                return;
            }
            // Dropping after itself takes the symbol out of the list first, which shifts the later ones back.
            int index = dropIndex > glyph.Index ? dropIndex - 1 : dropIndex;
            if (dropIndex >= 0 && index != glyph.Index)
            {
                SymbolMoved?.Invoke(glyph.Index, index);
            }
        }

        /// <summary>Puts the drop marker before or after the symbol tile under the pointer.</summary>
        private void MoveDropMarker(Vector2 pointer)
        {
            _dropIndex = -1;
            foreach (GlyphTile tile in _symbolTiles)
            {
                Rect bounds = tile.worldBound;
                if (!bounds.Contains(pointer))
                {
                    continue;
                }
                bool isBefore = pointer.x < bounds.center.x;
                _dropIndex = isBefore ? tile.Glyph.Index : tile.Glyph.Index + 1;
                Rect layout = tile.layout;
                _dropMarker.style.left = isBefore ? layout.xMin - 2f : layout.xMax;
                _dropMarker.style.top = layout.yMin;
                _dropMarker.style.height = layout.height;
                break;
            }
            _dropMarker.style.visibility = _dropIndex >= 0 ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>Ends a press or drag, releasing <paramref name="pointerId"/>, or every pointer when it's -1.</summary>
        private void EndPress(int pointerId)
        {
            if (_pressedTile != null)
            {
                if (pointerId >= 0 && _pressedTile.HasPointerCapture(pointerId))
                {
                    _pressedTile.ReleasePointer(pointerId);
                }
                _pressedTile.RemoveFromClassList(GlyphTile.DraggedClass);
            }
            _dropMarker.RemoveFromHierarchy();
            _pressedTile = null;
            _isDragging = false;
            _dropIndex = -1;
        }

        private bool CanDrag(GlyphTile tile)
        {
            return tile.Glyph.IsSymbol && !_sortsSymbolsByName && !_isReadOnly && _filter.Length == 0;
        }

        private static bool HasAllAscii(CharacterGlyph[] characters)
        {
            int count = 0;
            foreach (CharacterGlyph character in characters)
            {
                if (character.Character is >= 0x20 and <= 0x7E)
                {
                    count++;
                }
            }
            return count >= 0x7E - 0x20 + 1;
        }

        private static Button CreateAddButton(string text, string tooltip, Action clicked)
        {
            Button button = new(clicked) { text = text, tooltip = tooltip };
            button.AddToClassList("glyph-gallery__add");
            return button;
        }

        private static void SetShown(VisualElement element, bool isShown)
        {
            element.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
